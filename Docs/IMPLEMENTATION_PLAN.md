# CashflowChains implementation plan

Authority: operator request dated 20 October 2026 and supplied package. Build two independent projects; no shared runtime dependency. Kickoff scope is CC-P00 through CC-P02 only. Supplied catalogs remain explicitly unvalidated in Docs/SourcePackage/data.

| Milestone | Files / work | Required proof |
|---|---|---|
| CC-P00 | ProjectSettings, Packages, Assets/_Game/Editor/BuildProject.cs, Scripts/Infrastructure/AtomicSave.cs | Unity 6000.3.26f1 import, pinned lock, scene, Linux build, atomic save tests |
| CC-P01 | Scripts/Domain, Scripts/Presentation, Tests/EditMode | placement/rotation/undo or cancellation, actual portrait runtime captures, pointer interaction |
| CC-P02 | deterministic resolver, application session and persistence | independent conservation/packing oracles, exact replay, duplicate operation tests, 10,000 cases |
| CC-P03 | representative original art/audio pipeline | inspect and approve genuine Unity captures; not authorized to mass-generate art in kickoff |
| CC-P04 | guided first session and curated opening | real novice feedback; timing and voluntary replay unknown until study |
| CC-P05 | progression, catalogs and save expansion | game-specific master specification gates |
| CC-P06 | full content / tooling / progression | exact game-specific inventory and free solutions |
| CC-P07 | polish and accessibility | device/audio/localization checks |
| CC-P08 | analytics and optional verified IAP | pinned v5 API + real platform sandbox; no fake billing |
| CC-P09 | device QA and closed test | actual participant/device/privacy approval |
| CC-P10 | release artifact and submission packet | signing and publishing need operator approval |

Tests: Assets/_Game/Tests/EditMode/CoreTests.cs; exact commands recorded in Docs/TEST_LOG.md. Build scene authored reproducibly by BuildProject.CreateScene, inspect through live Unity. Runtime captures are proof of the mechanic slice only.
