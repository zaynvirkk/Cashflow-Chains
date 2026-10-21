# Google Play commerce setup

Code is present; native store acceptance is not complete. Nothing has been submitted or purchased.

1. In the verified developer account, create the new app with package `com.virkgames.cashflowchains` and set up Play App Signing. Use the local upload certificate in `Release/Signing`; privately back up `.local-signing`.
2. Create the nine active product IDs from `CommerceCatalog.cs` / C12: `cc.start.01`, `cc.gems.small`, `cc.gems.medium`, `cc.gems.large`, `cc.reroll.5`, `cc.blueprint.4`, `cc.skin.neon`, `cc.scenario.harbor`, `cc.scenario.finance`. Set types exactly as the catalog defines. Store/region prices are configured in Play Console; the USD prices in the supplied design remain hypotheses. Do not create or enable `cc.pass.s1`.
3. Get the app's **public licensing/receipt-validation key** from Play Console. This is separate from the private upload signing key. Use `Tools/configure-play-store.py --public-key-file /path/to/public-key.txt --enable`. Do not put signing passwords or backend secrets in that file. The tool creates runtime configuration; no code edit or fabricated key is required.
4. Rebuild Android APK/AAB, run `Tools/sign-release.py`, validate via `Tools/audit-android-release.py`, then upload to an authorized test track. Metadata is never faked when a product is missing or offline.
5. With genuine license testers, cover cancel, pending/deferred approval, duplicate redelivery, kill before confirmation, save failure, restore after reinstall and refunds/revocations. Match actual product metadata and localized prices to the UI. Record product IDs and hashed order references, never raw receipts or card data.
6. Review SDK data practices and update the public privacy policy/Data safety/content rating/target audience. Complete the account's actual tester requirements. Publication is a separate operator action.

Eligibility follows the spec: no purchase before a successful first chain, voluntary store entry, founder after equivalent early progression or ten minutes/three clears, extra blueprints after two used slots, neon after the first district, relevant expansions after base progress. No fake countdowns, subscription, paid random rewards, forced ads or mandatory refills. Draft Practice keeps required solution pieces and starts with a free optional reroll.

Local-only consumables cannot promise cross-device restoration. The purchase ledger is retained and never pruned to permit duplicate grants. A failed save rolls memory back and leaves the order unconfirmed for safe redelivery. Ownership is reconciled only after a successfully verified purchase-history response; offline/unknown history preserves access.
