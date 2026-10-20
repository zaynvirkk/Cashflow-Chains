# Kickoff test log

Editor: Unity 6000.3.26f1 LTS, Universal 2D template / URP 17.3.0, local Pipeline 0.8.0-exp.1. Date: 20 October 2026. Exact actual commands from project root:

```bash
unity recompile --project-path "$PWD" --timeout 120 --format json
./Tools/test.sh
./Tools/build.sh linux
python3 Tools/verify-package.py
python3 Tools/native-review.py --display :93
```

`Tools/test.sh` calls `unity command run_tests --caller plugin --skill unity-cli --project-path "$PWD" --timeout 300 --format json -- --mode editor --filter CashflowChains.Tests --filter_type assembly --timeout 300`. Latest structured response: `Artifacts/Tests/latest.json`, **21 passed, zero failed/skipped**. The 10,000-case test is included in that count, not claimed as 10,000 separate NUnit tests. Three lab puzzles have legal free witness tests. No full 72-mission solvability claim.

`Tools/build.sh linux` runs the actual editor BuildPipeline through the pinned project. Exact build response: `Artifacts/Logs/build-linux.json`. Player paths in README. API36/ARM64/IL2CPP are Android settings, not proof of a tested Android APK.

`Tools/native-review.py` starts the actual Linux development player on an existing private Xephyr X display, sends normal xdotool drag/click/key events, captures from ScreenCapture, checks exported state, terminates that exact process, resumes from the same durable save and checks wallet/result identity. Actual captures and source hashes are in the directory named by `Artifacts/latest-native-review.txt`. Portrait geometry checked at 720×1280, 720×1600 and 720×1024. These are desktop virtual-device-style resolutions, not physical phone acceptance. Script requires xdotool and a private X display; it does not contact or publish externally.

Package integrity: `Tools/verify-package.py` verifies original bytes and expected 24/72/48/72 catalog counts and shape connectivity; outputs solvability NOT_TESTED and releaseBalanced=false. All 48 Black Bag seed footprints get actual C# rotation/mask tests in Black Bag; they are not promoted to balanced release content.

Known tooling history: authenticated Pipeline 0.9 download failed; used verified embedded local 0.8, independent in each project. Initial material-upgrade/import dialogs delayed some CLI calls. First editor-screen capture used a stretched landscape Game view; it is not accepted as portrait proof. Actual native captures supersede that failed capture. Build/test acceptance uses final passing reports, not those transient failures.

Save recovery defect found during implementation: corrupt primary could overwrite the good backup on the next write. Recovery writes now preserve the good backup and archive the bad primary; regression and full-session JSON tests pass.
