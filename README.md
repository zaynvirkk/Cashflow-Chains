# CashflowChains

Independent Unity 6.3 LTS pure 2D mobile game, created from the operator's 20 October package. Current deliverable is the P00–P02 mechanic slice: three small laboratory boards, local save/resume, deterministic rules and original flat prototype visuals. Full launch content and commercial systems are not implemented.

Run locally: `./play.sh` (Linux build). Open this project root in Unity **6000.3.26f1**, then open `Assets/_Game/Scenes/Main.unity`. Rebuild scene through **Game → Rebuild scene**. With that editor running, `Tools/test.sh`, `Tools/build.sh linux`, or `Tools/build.sh android`. Android target API 36, min 26, ARM64 IL2CPP; a configured target does not prove physical-device support.

F12 captures in development builds when launched with `-capture-dir /absolute/path`. `-save-dir /absolute/path` isolates test saves; normal play uses Unity's persistent data directory. These diagnostics cannot grant loot or credits.

Read [implementation status](Docs/IMPLEMENTATION_STATUS.md), [test evidence](Docs/TEST_LOG.md), [plan](Docs/IMPLEMENTATION_PLAN.md), [decisions](Docs/DECISIONS.md), [risks](Docs/RISKS.md). Supplied specification and seed catalogs preserved in Docs/SourcePackage; no seed catalog is labelled solved or release-balanced.

No billing, advertising, account requirement or runtime server. Human visual acceptance, phone touch/performance, enjoyment, retention and revenue remain unverified.
