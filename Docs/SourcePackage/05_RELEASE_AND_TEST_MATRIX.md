# TWO-GAME SHIP MATRIX AND PUBLICATION CHECKLIST
Version 1.0 | 20 October 2026

## R01. Ship blockers: never release with any P0
- Lost player progress after app kill, suspend, reinstall; unreported corruption or reset.
- Wrong IAP product, duplicated consumable credits, unlock failed after verified purchase, failed restore nonconsumables or repeated charge/prompt.
- Crash or hang at first run, touch controls inaccessible on supported screens, illegible board states, unsolvable mandatory missions.
- Selling promised gameplay content that is absent or dependent on unshipped DLC.
- Screenshots falsely representing actual gameplay, perspective/non-2D assets violating product spec, third-party unlicensed art/fonts/music.
- Missing/false Data Safety, privacy labels, consent, age rating, disclosures or inaccurate product price presentation.
- App bundle failing current API requirements, unapproved release signing, absent tested billing, inappropriate ad SDK handling.

## R02. Minimum functional pre-submission matrix
| ID | Scenario | Pass condition | Evidence |
|---|---|---|---|
| T01 | Fresh first install offline | Launch playable/no account/intro complete | Video on device |
| T02 | Resume after kill mid-round | Exact board + inventory + risk restored | State diff + screenshot |
| T03 | 10 fast taps on reward/Activate/Take | One idempotent grant and one state change | Domain log |
| T04 | Offline 24h, 72h, month | No impossible rewards/corruption, transparent cap | Simulation report |
| T05 | Clock moved back/forward | No negative earning or exploit | Deterministic test |
| T06 | Low-end device memory pressure | Return gracefully; no save loss | Logs, screenshot |
| T07 | Narrow phone/tall phone/tablet | Main mechanics legible, safe-area respected | Screenshot atlas |
| T08 | Locale long strings / RTL readiness | No text crop or wrong number/currency | Localization run |
| T09 | Music off/haptics off/reduced motion | Toggle works throughout game | Accessibility video |
| T10 | Airplane mode full tutorial + 20 runs | Gameplay remains operational | Offline device test |
| T11 | Store offline/unavailable | Correct message, no fake money, core play remains | Store mock |
| T12 | Purchase canceled | No entitlement, button restored | Sandbox log |
| T13 | Purchase pending/deferred | No duplicate/early grant | Sandbox log |
| T14 | Paid and app killed before confirmation | Exactly-once grant on recovery | Ledger diff |
| T15 | Restore after reinstall/new device | Nonconsumable restored; consumable behavior documented/tested | Purchase sandbox |
| T16 | Refund and entitlement revocation | Correct state reconciliation; no data loss | Test refund |
| T17 | Crashed telemetry/privacy SDK | Game proceeds; consent respected | Integration test |
| T18 | Ads disabled/no network | No required content blocked | Feature flag test |
| T19 | 10k randomized rounds | No crashed, infinite scores, invalid bounds | Test summary |
| T20 | Real screenshot/advert evidence | Shows shipping mechanics, not fabricated render | Asset provenance |
| T21 | Content additions after patch | Old saves work; IDs retained | Migration suite |
| T22 | Reset/backup/consent support | Data control and help routes work | Manual review |

## R03. Cashflow-specific tests
- Exactly 8 simulation beats, deterministic event order and integer money logic.
- Resources aren't minted in conversion without source; no graph cycle profit amplification.
- Rotated pieces connect through correct ports; undo then preview yields correct graph.
- 72 core challenges: legal pieces, budgets, constraints and reachable 1-star results; optional 3-star reachable by free mode when advertised.
- At least two distinct viable layouts on expert seeded levels where alternate strategies promised; no 90% dominant solved layout across trial seeds.
- All boost products optional; challenge still finishable without purchases.
- Profit visual matches domain ledger; no misleading “per second” display if metric is `credits per round`.
- Earned district trophy visual changes only after actual unlock and persists.

## R04. Black Bag-specific tests
- Item legal placement equal to displayed occupied cells, in all 4 rotations.
- Overlap/blocked/out-of-range prevented; invalid move restores exact original state.
- Preview moves free; Take and Open costs visible and deterministic; Escape persists exact secured haul.
- All 72 core contracts have free solvable 1-star goal in at least one earnable bag.
- No alert tick during inactivity, pause/background, or many drags. Additional Take's alert warning is shown.
- Weight and fragility effects displayed and calculated correctly. Unique artifacts aren't silently destroyed by UI or failed unrelated heist.
- Collection sets possible via earnable content; set completion once, not duplicate by resume.
- No content requiring real firearms/real-world stealth methods or unwanted store ratings.

## R05. Device matrix
Android priority devices: low RAM/low GPU device with minimum supported OS; mainstream Snapdragon/MediaTek 1080x2400; modern high refresh 120Hz; foldable/tablet where layout claims support. Test at least two physical Android devices before open beta; use emulator for additional variants, but not as substitute for touch and thermal testing. For iOS, test at least one older supported iPhone and one current iPhone with Xcode/TestFlight before publication.

Checklist per device: cold start, first touch, 20 rounds, install/update, pause/resume, phone call/audio interruptions, airplane mode, data network flapping, thermal after 30 minutes, battery estimate, accessibility settings, product restore, privacy toggles. Collect actual frame times and peak memory, not estimates.

## R06. Content and asset provenance
- Original sprites, source paths and export IDs committed and linked to corresponding game item IDs.
- Font license path recorded; generated audio and image rights/terms reviewed; no asset from copyrighted game or artist imitation pipeline.
- No rendered reference screenshot used as actual sprite atlas. No UI text burned into images.
- Scene's textures have no 3D perspective. Sprite artwork consistent at real final phone resolution and reduced-motion.
- Store listing screenshots captured from shipping Unity build; text may add store-required contextual captions that don't claim nonexistent features.

## R07. Player study checklist
5–10 qualitative users is enough for spotting major onboarding defects, not monetization proof. 30–100 voluntary users can reveal early behavioral dropoffs, still noisy for purchases. Any conversion inference must show sample size and uncertainty. Track install cohort properly. Beta script: novice plays unaided until 3min; player narrates understanding; continue to 15/30 min, note confusion, voluntary replay; return after 24h and again after 7 days if willing; optionally genuine real-priced purchases in a compliant limited release. Record game version and device.

Signal red flags: tutorial completion <70%, most players can't describe objective, common mis-taps, first round takes >5min, players never find Replay/Next, players never look at progress, or store value unexplained. Success is measured, not invented to justify sunk cost.

## R08. Store listing production deliverables
For each game produce:
- Unique final name after trademark/search clearance; 512x512 logo/icon; feature graphic where required; portrait store images from actual 2D gameplay; 15–30s truthful gameplay video; concise full/short description; keywords and tag suggestions; age rating; privacy policy and support URL; IAP SKU list with accurate descriptions; applicable localization files; release notes; store screenshots and promo creative source files.
- Google Play: AAB, target API ≥36 as of Oct 2026, app signing, Data Safety reflecting SDKs, merchant profile/payout, registered 12 testers 14 days if account qualifies, production request, staged rollout and feedback.
- Apple: App ID, signing provisioning, current SDK requirements, App Store Connect metadata, subscription rules if ever added, privacy labels/manifests, TestFlight build, user support, IAP approval and restore.

## R09. Engineering signoff and business signoff
Engineering signoff requires: 0 P0 and 0 P1 crashes/payment/data-loss, all acceptance tests pass, performance within target, screenshots match actual device, privacy/legal declarations reviewed and signed by authorized person. Business signoff requires: declared KPI observation window, real users and acquisition source, costs and return measured. A release can be engineering-successful and financially unsuccessful. Neither should be conflated.

## R10. Live operations after release
Day 1/7/14: crash/IAP/negative review triage, data consistency, delayed notifications, verified pricing. At least monthly content update only if current retention warrants. Always support turning off event/store offers remotely and rolling back defect configurations. Track refund/chargebacks in secure ledger. Re-evaluate whether more game content or advertising spending improves **net profit**, not vanity installs.

## R11. Human responsibilities Codex cannot verify on its own
- Corporate/individual developer eligibility and KYC, contracts, taxes, payout accounts and identity.
- Trademarks in target markets; final store descriptions and legal/privacy policy approved for actual SDK behavior.
- Pay for permitted developer accounts, test devices, ads and domain if any.
- Real tester recruitment, moderation of feedback, physical-device review, human visual/art acceptances.
- iOS build signing and Mac toolchain access (or supported managed build); store submission/approval outcome.
- App review objections, local consumer protection and child-directed product considerations.

**Sources:** see the dated official links in `03_SHARED_TECH_MONETIZATION_LAUNCH.md`; verify again at actual submission.
