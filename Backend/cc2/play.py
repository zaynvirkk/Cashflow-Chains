"""Google Play boundary. Credentials and tokens never cross into the Unity build or logs.

The verifier has no grant authority. Its result is consumed by the durable purchase
transaction; acknowledgement/consumption happens only after that commit.
"""
from dataclasses import dataclass
from datetime import datetime, timezone
import hashlib
import hmac
import os
from urllib.parse import quote

class PlayUnavailable(Exception):
    """Retryable verification/configuration failure; no grant is permissible."""

class PlayRejected(Exception):
    """A response failed binding/state checks; no grant is permissible."""

class PlayPending(Exception):
    """The platform has not charged the customer yet."""

@dataclass(frozen=True)
class VerifiedPurchase:
    product: str
    order_id: str
    completed_at: int
    region: str
    acknowledged: bool
    consumed: bool
    test_purchase: bool

class GooglePlay:
    ROOT = 'https://androidpublisher.googleapis.com/androidpublisher/v3/applications/'
    SCOPE = 'https://www.googleapis.com/auth/androidpublisher'

    def __init__(self, package='com.virkgames.cashflowchains', enabled=False,
                 allow_tests=False, transport=None):
        if package != 'com.virkgames.cashflowchains':
            raise ValueError('Unexpected application package')
        self.package, self.enabled, self.allow_tests = package, enabled, allow_tests
        self._transport = transport

    @classmethod
    def from_environment(cls):
        return cls(enabled=os.environ.get('CC2_PLAY_ENABLED') == 'true',
                   allow_tests=os.environ.get('CC2_PLAY_ACCEPT_TESTS') == 'true')

    def _request(self, method, path):
        if not self.enabled:
            raise PlayUnavailable('Google Play verification is not configured')
        try:
            if self._transport is None:
                import google.auth
                from google.auth.transport.requests import AuthorizedSession
                credentials, _ = google.auth.default(scopes=[self.SCOPE])
                self._transport = AuthorizedSession(credentials, refresh_timeout=10)
            response = self._transport.request(method, self.ROOT + quote(self.package, safe='') + path,
                                               timeout=(3, 10), allow_redirects=False)
            if response.status_code == 404:
                raise PlayRejected('Purchase was not found')
            if response.status_code in (400, 409):
                raise PlayRejected('Platform rejected this purchase operation')
            if response.status_code < 200 or response.status_code >= 300:
                raise PlayUnavailable('Google Play verification is temporarily unavailable')
            if not response.content:
                return {}
            value = response.json()
            if not isinstance(value, dict):
                raise PlayUnavailable('Invalid platform response')
            return value
        except (PlayUnavailable, PlayRejected):
            raise
        except Exception:
            # Provider exception strings can contain the purchase token in the URL.
            raise PlayUnavailable('Google Play verification failed') from None

    def verify(self, token, product, account, intent, now):
        if not isinstance(token, str) or not 16 <= len(token) <= 4096:
            raise PlayRejected('Invalid purchase token')
        document = self._request('GET', '/purchases/productsv2/tokens/' + quote(token, safe=''))
        return self.validate(document, product, account, intent, now, self.allow_tests)

    @staticmethod
    def validate(doc, product, account, intent, now, allow_tests=False):
        if not isinstance(doc, dict):
            raise PlayRejected('Invalid purchase document')
        expected_account = hashlib.sha256(account.encode()).hexdigest()
        expected_intent = hashlib.sha256(intent.encode()).hexdigest()
        if not hmac.compare_digest(str(doc.get('obfuscatedExternalAccountId', '')).lower(), expected_account):
            raise PlayRejected('Purchase belongs to another account')
        if not hmac.compare_digest(str(doc.get('obfuscatedExternalProfileId', '')).lower(), expected_intent):
            raise PlayRejected('Purchase does not match the checkout intent')
        context = doc.get('purchaseStateContext', {})
        if not isinstance(context, dict):
            raise PlayRejected('Invalid purchase state context')
        state = context.get('purchaseState')
        if state == 'PENDING':
            raise PlayPending('Awaiting platform approval')
        if state != 'PURCHASED':
            raise PlayRejected('Purchase is not completed')
        lines = doc.get('productLineItem', [])
        if not isinstance(lines, list) or len(lines) != 1 or not isinstance(lines[0], dict) or lines[0].get('productId') != product:
            raise PlayRejected('Purchase product mismatch')
        offer = lines[0].get('productOfferDetails', {})
        if not isinstance(offer, dict):
            raise PlayRejected('Invalid product offer')
        if offer.get('quantity') != 1 or offer.get('refundableQuantity') != 1:
            raise PlayRejected('Unsupported quantity or refunded purchase')
        if 'rentOfferDetails' in offer or 'preorderOfferDetails' in offer:
            raise PlayRejected('Unsupported purchase option')
        test_context = doc.get('testPurchaseContext', {})
        if not isinstance(test_context, dict):
            raise PlayRejected('Invalid test purchase context')
        test = test_context.get('fopType') == 'TEST'
        if test and not allow_tests:
            raise PlayRejected('Test purchases are not enabled in this environment')
        order = doc.get('orderId')
        if not isinstance(order, str) or not order.startswith('GPA.') or len(order) > 160:
            raise PlayRejected('Missing platform order reference')
        try:
            stamp = datetime.fromisoformat(doc['purchaseCompletionTime'].replace('Z', '+00:00'))
            if stamp.tzinfo is None:
                raise ValueError()
            completed = int(stamp.astimezone(timezone.utc).timestamp())
        except (ValueError, TypeError, KeyError):
            raise PlayRejected('Invalid platform completion time') from None
        if completed < 1 or completed > now + 300:
            raise PlayRejected('Invalid platform completion time')
        ack = doc.get('acknowledgementState')
        consumed = offer.get('consumptionState')
        if ack not in ('ACKNOWLEDGEMENT_STATE_PENDING', 'ACKNOWLEDGEMENT_STATE_ACKNOWLEDGED'):
            raise PlayRejected('Unknown acknowledgement state')
        if consumed not in ('CONSUMPTION_STATE_YET_TO_BE_CONSUMED', 'CONSUMPTION_STATE_CONSUMED'):
            raise PlayRejected('Unknown consumption state')
        region = doc.get('regionCode', '')
        if not isinstance(region, str) or len(region) != 2 or not region.isalpha() or region != region.upper():
            raise PlayRejected('Invalid billing region')
        return VerifiedPurchase(product, order, completed, region,
                                ack == 'ACKNOWLEDGEMENT_STATE_ACKNOWLEDGED',
                                consumed == 'CONSUMPTION_STATE_CONSUMED', test)

    def complete(self, token, product, consumable, purchase):
        if consumable and purchase.consumed or not consumable and purchase.acknowledged:
            return
        action = 'consume' if consumable else 'acknowledge'
        self._request('POST', '/purchases/products/' + quote(product, safe='') +
                      '/tokens/' + quote(token, safe='') + ':' + action)
