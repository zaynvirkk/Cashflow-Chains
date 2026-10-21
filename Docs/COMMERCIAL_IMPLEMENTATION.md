# Commercial implementation and acceptance

The model follows C12, S08 and S09 of the supplied documents. The earlier free-only direction is superseded. The first 72 puzzles remain free. No fake timer, subscription, randomized paid reward or ad SDK was introduced.

`Domain/CommerceCatalog.cs` defines nine real product IDs/types and exact delivery amounts. Google Play metadata is the only purchase-price source. The store is voluntary and milestone gated. No checkout can begin with an absent product, missing public validation key or unavailable store.

`Infrastructure/LocalStore.cs` uses Unity Purchasing 5.4.4: listeners precede Connect, products/history are fetched, pending orders are validated, deferred/canceled states grant nothing, and purchases confirm only after `PurchaseDelivery.PersistVerified` has durably saved. Receipt transaction references are hashed; raw receipts are not logged. The local ledger deduplicates redelivery. A failed save rolls memory back and leaves the platform order pending. History errors preserve existing ownership; only verified successful history can reconcile missing entitlements. No fake provider runs in a player build.

Permanent products: founder 500 city coins once + two material finishes + badge, four extra blueprint slots (eventually twelve including eight earnable), eight neon building sprites, twelve Harbor puzzles and twenty Finance expert puzzles with eight new piece recipes. Gems grant 90/520/1150, five-reroll pack grants five tickets. The pass remains disabled. Purchases do not change the free campaign's authoritative scoring or required layouts.

Draft Practice keeps every required solution piece, offers a variable optional tray and one free reroll. Further rerolls explicitly spend one ticket or ten gems. Returning to a fresh practice city resets its free reroll; no mandatory refill or wait. Undo cannot reverse paid reroll consumption. Gems can also buy three permanent decorative particle finishes. Consumables are local-only with an explicit warning; no cross-device cloud-restore promise.

Internal tests cover exact fulfillment, duplicate receipt/product mismatch, malformed premium balance, legacy migration, entitlement reconciliation, save failure and redelivery, free/paid draft reroll and Undo, and collection rewards. These fixtures are not evidence of real payment, Google Play pending/refund behavior or purchase restoration. The real platform matrix remains external until app/products/testers and the public licensing key exist.

See Release/Store/GOOGLE_PLAY_SETUP.md and the current privacy draft. The public licensing key is not the private upload signing key. Do not guess Data safety: Unity IAP 5.4+ data practices must be reviewed for the actual native-only configuration. Offline local gameplay remains available when commerce cannot connect.

Primary API sources verified 21 October 2026: https://docs.unity.com/en-us/iap/purchases and https://docs.unity.com/en-us/iap/receipt-validation . Package signatures were checked against the installed 5.4.4 source as well.
