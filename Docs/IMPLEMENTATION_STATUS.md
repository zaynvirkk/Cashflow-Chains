# CashflowChains implementation status

**20 October 2026 — playable P00–P02 mechanic slice; desktop verified.** This is the package's explicitly scoped kickoff deliverable, not the complete commercial game. Working names remain provisional.

- Independent Unity **6000.3.26f1 LTS Universal 2D** project and local Git repository. Native uGUI/TextMeshPro, fixed orthographic camera, zero scene MeshRenderers, original flat geometry, licensed DejaVu Sans. No dependency on the other game's runtime or assets.
- Three free laboratory puzzles, placement/rotation, Undo, exact preview, eight-beat event timeline and first-clear ledger. Domain remains pure C#.
- Checksummed atomic local save, backup recovery, full-game JSON, duplicate-operation protection. Exact round/heist state survives a real process stop/restart; read failures do not silently reset progress.
- **21/21 EditMode tests pass**, including one 10,000-case randomized test. A real four-router cycle retains its five goods; stable node IDs preserve event order. All three lab fixtures have legal free witness solutions.
- Linux development player at `Builds/Linux/CashflowChains.x86_64`; launcher `./play.sh`. Actual mouse gestures/results/cold resume and 720×1280, 720×1600, 720×1024 captures: path in `Artifacts/latest-native-review.txt`. Visual review: [actual capture gallery](REVIEW.html).
- 17 original package files byte-preserved. Supplied 72-level catalogs remain unvalidated preproduction material; their full solvability and balance are not asserted.

| Milestone | Current evidence | Remaining boundary |
|---|---|---|
| P00 setup | Desktop compile + native Linux build, package lock, scene generator, save schema, CI entrypoint | Headless CI invocation and Android/iOS build not run; mobile hardware not tested |
| P01 interaction | Actual desktop drag/tap/rotation, invalid placement rules and native captures | Physical touch/notch/accessibility target-size acceptance unknown |
| P02 pure mechanic | 21 tests + 10k invariant cases, actual native result and cold resume | Mobile performance and subjective replay value unknown |
| P03–P10 | Plan, risks and traceability recorded | Production art/audio, guided onboarding, full progression/content, commercial services and release work remain |

**Stop boundary from the supplied master specification:** “Implement only P00-P02.” The next game-specific gate is a representative original art pipeline and approval of the playable slice; no mass art batch or store was built.

Known limitations: prototype graphics and simple laboratory selection; no full hub/collection, complete 72-level campaign, audio, localization catalog, IAP, game analytics uploader, ad SDK, external account or publishing. Reduced motion is available for Cashflow animation; further accessibility/localization/device work remains. `Tools/ci.sh` is prepared but has not been invoked on a clean headless worker. Android portrait/API36/min26/ARM64 IL2CPP is configured; no Android artifact or physical phone result is claimed. Human novice testing, visual approval, engagement, retention, sales and profit remain UNKNOWN.
