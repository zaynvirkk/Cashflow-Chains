# Cashflow Chains: local acceptance

21 October 2026. **PASS for the delivered local Linux and Android playtest edition.** This records observed engineering behavior; real-player preference and store release acceptance remain separate.

| Check | Result | Exact evidence |
|---|---|---|
| Fresh Unity EditMode | 39 passed, 0 failed; includes 10,000 randomized cases | [Artifacts/Tests/ci.xml](../Artifacts/Tests/ci.xml) |
| Native campaign | 72/72, 216 stars, ordinary visible controls, isolated saves; source/binary hashes bound | [Artifacts/CampaignReplay/20261021T002538Z/report.json](../Artifacts/CampaignReplay/20261021T002538Z/report.json) |
| Native menus and lifecycle | Input, free preview, results, complete catalog pagination, settings, restart, modal blocking and archive/reset pass | [Artifacts/Playtest/20261021T002230Z/report.json](../Artifacts/Playtest/20261021T002230Z/report.json) |
| Earned features | Actual completed-campaign saves copied byte for byte, ordinary input, unchanged wallet, zero overflow | [Artifacts/EarnedPlaytest/20261021T035614Z/report.json](../Artifacts/EarnedPlaytest/20261021T035614Z/report.json) |
| Android actual APK | ARM64 installed on Google APIs API 35 x86_64 emulator with ARM64 translation; touch, result, airplane-mode cold restart, Back, modal blocking and sizes pass | [Artifacts/Android/20261021T033949Z/report.json](../Artifacts/Android/20261021T033949Z/report.json) |
| Build and distribution | Linux/Android exit 0; ZIP integrity checked; exact delivered SHA-256 recorded | [Builds/Deliverables/DELIVERY.json](../Builds/Deliverables/DELIVERY.json) |
| Original package | 17 files byte-preserved per project; seed structure verifier intentionally makes no production-solvability claim | Tools/verify-package.py |

All 24 building types are reachable; the production campaign and eight combo families have deterministic reference coverage. Blueprint slot 8, all six palettes, the last commission page and all achievement pages were exercised with legitimately earned progress.

Native menus were inspected at 540×960, 720×1600, 720×1024 and 1080×720. Android captures include the default 1080×2400 guest display, 720×1600 and 720×1280. Visible controls and native text stay within the fitted portrait area; safe-area letterboxing is intentional. No persistent text-overflow findings occurred in accepted menu/campaign/earned/Android reports. Captures are actual player output, not mockups.

Native menu frame sample: median **16.67 ms**, p95 **28.40 ms**, allocated Unity memory **90.8 MiB**. This menu run used the primary NVIDIA hardware display. Full campaign's final segment on the primary hardware display: median **16.67 ms**, p95 **18.58 ms**. Android guest sample: median **16.67 ms**, p95 **33.33 ms**, maximum **2796.61 ms**. First menu/font work and restarts can spike; these are measured samples, not universal phone guarantees.

Actual Android AudioListener mix: peak **0.181**, RMS **0.033**; nonzero and below clipping. Native mixes and source audio measurements are also retained. Recording taps game output, never a microphone. This checks signal/output and controls; human musical preference is unmeasured.

Campaign interruptions and cold-resume segments are recorded in the report and sibling driver logs. Earlier failed/intermediate runs remain preserved. No profile grants, weakened scoring oracle or edited puzzle witnesses were used to produce winning campaign results. Test-driver focus/input timing fixes do not change game mechanics.

The Android Back defect found during this work was fixed with an original GameActivity subclass forwarding hardware/gesture callbacks into the game. Final APK tests use `com.virkgames.platform.GameEntryActivity` and verify Back from a board and overlaid dialogs. Actual gesture regression: [Artifacts/AndroidGesture/20261021T035540Z/report.json](../Artifacts/AndroidGesture/20261021T035540Z/report.json). Android immersive mode first requires a bottom-edge reveal of system navigation, followed by the edge-Back gesture; the test records SystemUI allowing Back and uses actual swipes without substituting a key event.

**Release boundaries:** English only; no physical-phone thermal/battery/haptic/TalkBack proof, genuine store transactions, signed store submission, Windows/iOS/Steam/Steam Deck/controller acceptance, external closed test, retention or revenue. Real checkout is unavailable in this local edition. Local credits are game currency. No accounts, ads, live analytics uploader, publishing or spending occurred.
