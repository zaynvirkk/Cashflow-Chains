"""Synthetic provider documents prove validation, not real Play billing acceptance."""
import copy,hashlib,sys
from pathlib import Path
import pytest
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from cc2.play import GooglePlay,PlayRejected,PlayPending,PlayUnavailable
NOW=1800000000

def document():
 return {'obfuscatedExternalAccountId':hashlib.sha256(b'account').hexdigest(),
         'obfuscatedExternalProfileId':hashlib.sha256(b'intent').hexdigest(),
         'purchaseStateContext':{'purchaseState':'PURCHASED'},'orderId':'GPA.synthetic-test',
         'purchaseCompletionTime':'2027-01-15T08:00:00Z','regionCode':'IN',
         'acknowledgementState':'ACKNOWLEDGEMENT_STATE_PENDING',
         'productLineItem':[{'productId':'cc2.starter','productOfferDetails':{
          'quantity':1,'refundableQuantity':1,'consumptionState':'CONSUMPTION_STATE_YET_TO_BE_CONSUMED'}}]}

def validate(d):return GooglePlay.validate(d,'cc2.starter','account','intent',NOW)

def test_completed_purchase_is_bound_to_exact_account_intent_product_and_quantity():
 p=validate(document());assert p.product=='cc2.starter' and p.region=='IN' and not p.consumed and not p.acknowledged

@pytest.mark.parametrize('key,value',[
 ('obfuscatedExternalAccountId','wrong'),('obfuscatedExternalProfileId','wrong'),
 ('orderId',''),('regionCode',''),('regionCode','in'),('purchaseCompletionTime','bad'),
 ('purchaseCompletionTime','2027-01-15T08:00:00'),('purchaseCompletionTime','2099-01-01T00:00:00Z'),
 ('acknowledgementState','unknown')])
def test_bad_platform_bindings_and_states_fail_closed(key,value):
 d=document();d[key]=value
 with pytest.raises(PlayRejected):validate(d)

@pytest.mark.parametrize('state',['CANCELLED','PURCHASE_STATE_UNSPECIFIED','unknown',None])
def test_nonpurchased_states_never_grant(state):
 d=document();d['purchaseStateContext']['purchaseState']=state
 with pytest.raises(PlayRejected):validate(d)

def test_pending_is_a_distinct_non_grant_state():
 d=document();d['purchaseStateContext']['purchaseState']='PENDING'
 with pytest.raises(PlayPending):validate(d)

@pytest.mark.parametrize('mutation',['product','multiple','quantity','refund','rent','preorder','consumption','test'])
def test_unsupported_or_refunded_product_cannot_be_fulfilled(mutation):
 d=document();line=d['productLineItem'][0];o=line['productOfferDetails']
 if mutation=='product':line['productId']='another'
 elif mutation=='multiple':d['productLineItem'].append(copy.deepcopy(line))
 elif mutation=='quantity':o['quantity']=2
 elif mutation=='refund':o['refundableQuantity']=0
 elif mutation=='rent':o['rentOfferDetails']={}
 elif mutation=='preorder':o['preorderOfferDetails']={}
 elif mutation=='consumption':o['consumptionState']='unknown'
 elif mutation=='test':d['testPurchaseContext']={'fopType':'TEST'}
 with pytest.raises(PlayRejected):validate(d)

def test_test_purchase_requires_explicit_server_environment_flag():
 d=document();d['testPurchaseContext']={'fopType':'TEST'}
 assert GooglePlay.validate(d,'cc2.starter','account','intent',NOW,True).test_purchase

def test_disabled_provider_performs_no_network_request():
 class NoNetwork:
  def request(self,*a,**k):raise AssertionError('Must not send')
 with pytest.raises(PlayUnavailable):GooglePlay(transport=NoNetwork()).verify('token-never-leaks','cc2.starter','account','intent',NOW)

def test_transport_errors_do_not_leak_purchase_token_or_provider_url():
 class Broken:
  def request(self,*a,**k):raise RuntimeError('secret-token in https://provider/path')
 with pytest.raises(PlayUnavailable) as err:GooglePlay(enabled=True,transport=Broken()).verify('secret-token-123456','cc2.starter','account','intent',NOW)
 assert 'secret-token' not in str(err.value) and 'https://' not in str(err.value)

def test_verified_provider_call_uses_fixed_host_quoted_token_and_bounded_no_redirect_transport():
 class Response:
  status_code=200;content=b'{}'
  def json(self):return document()
 class Transport:
  calls=[]
  def request(self,*a,**k):self.calls.append((a,k));return Response()
 t=Transport();p=GooglePlay(enabled=True,transport=t).verify('token-with/path?data','cc2.starter','account','intent',NOW)
 assert p.product=='cc2.starter';args,kw=t.calls[0]
 assert args[1].startswith(GooglePlay.ROOT) and '%2F' in args[1] and '%3F' in args[1]
 assert kw=={'timeout':(3,10),'allow_redirects':False}

@pytest.mark.parametrize('key,value',[
 ('purchaseStateContext',[]),('productLineItem',[None]),('productLineItem','bad'),
 ('testPurchaseContext',[]),('productLineItem',[{'productId':'cc2.starter','productOfferDetails':[]}])])
def test_malformed_platform_documents_are_rejected_without_uncaught_type_errors(key,value):
 d=document();d[key]=value
 with pytest.raises(PlayRejected):validate(d)
