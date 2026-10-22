# Cashflow Chains implementation status

> Historical V5 summary. The complete CC2 handoff now governs, including its expanded progression, offers, paid/earned acquisition and LiveOps. See [CC2 release status](CC2/RELEASE_STATUS.md) for current implementation and acceptance; do not use the old monetization restrictions or test counts below as current claims.

21 October 2026. Visual revision 5 production candidate; final local replay is in progress. Black Bag is on hold. The original package remains byte-preserved in Docs/SourcePackage.

Implemented: 72 free puzzles plus 12 optional Harbor and 20 optional Finance puzzles, eight districts, 32 buildings, 16 combos evaluated from actual deliveries/sales, eight collectible sets, eight earnable blueprint slots plus four optional purchased slots, 15 offline commissions, 30 achievements and six earned palettes. All 104 certified reference layouts and all 16 combo conditions pass domain checks. Original campaign identities/scoring and legacy saves are preserved.

Presentation uses native uGUI/TMP, original painted sprites and world art, Bungee display type and Barlow Condensed body type, directional traffic, building-specific ambient motion, flowing resource tokens, value plates, particles and original stereo music/cues. Drag lift, legal-cell attraction, ghosts, snap landing, press feedback and result count-up are implemented. Portrait and wide layouts fill the screen. Reduced motion, high contrast, separate audio controls and safe-area adaptation are saved. English only; expanded translation and physical accessibility acceptance remain open.

Unity 6000.3.26f1; URP 17.3; Input System 1.20; Unity Purchasing 5.4.4. Core rules and economy remain pure C#, integer-accounted and conservation checked. Atomic checksummed saves, migrations, backups, archive/reset, idempotent grants, purchase delivery rollback and bounded randomized tests are present. The current test suite has 62 cases, including two separate 10,000-board invariant runs. See MOBILE_ACCEPTANCE.md for current replay evidence; earlier manifests are historical until matching source and binary hashes pass.

The nine optional commerce products follow C12/S08/S09. Real metadata supplies prices; no fabricated price or checkout success is shown. Pending/deferred/canceled orders grant nothing; verified grants save before confirmation; receipt references are hashed. Consumables are local-only and this limitation is disclosed. Passes and advertisements are disabled. Google Play public licensing key/products, real sandbox transactions, restore/refund matrix and SDK privacy review remain external gates. No purchase, upload, publication or spending occurred.

A restricted local upload key and public certificate exist. Signed ARM64 IL2CPP APK/AAB target API36, minimum26. The packaging audit checks every native library and bundle for 16 KB alignment. Emulator acceptance uses API36 with actual 16 KB pages and ARM64 translation; physical phones remain untested. Build/test scripts cap workers and run one Editor at a time.

Play.sh launches the regular candidate. PlayContentQA.sh launches a separately built, watermarked development preview using an isolated domain fixture, never genuine earned progress or billing evidence. The regular release rejects that fixture command and fixture saves. See CONTENT_QA.md.

P0 external release gates: real Google Play configuration/payment matrix, account-specific closed testing, approved listing/privacy disclosures. P1: physical-device performance/thermal/haptic/accessibility tests and real-player comprehension/engagement study. Steam/Windows/iOS/controller support is not accepted. No claim of profitability, retention or addictive play follows from engineering tests.
