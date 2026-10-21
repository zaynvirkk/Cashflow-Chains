# Cashflow Chains: final local test log

Unity **6000.3.26f1 LTS**, URP 17.3.0, Input System 1.20.0, uGUI/TMP 2.0, Test Framework 1.6.0, IAP 5.4.4, embedded Pipeline 0.8.0-exp.1. Fresh test start: `2026-10-21 04:57:10Z`. **39 passed / 0 failed.** See Artifacts/Tests/ci.xml. Original kickoff log is preserved as KICKOFF_TEST_LOG.md.

Commands from this project root, one Unity process at a time:

```bash
python3 Tools/verify-package.py
DISPLAY=:1 Tools/test.sh
DISPLAY=:1 Tools/build.sh linux
DISPLAY=:1 Tools/build.sh android
python3 Tools/playtest.py --display :1
python3 Tools/campaign-playtest.py --display :1
python3 Tools/android-playtest.py
python3 Tools/android-gesture-test.py
python3 Tools/earned-playtest.py --display :1 --campaign Artifacts/CampaignReplay/20261021T043558Z
python3 Tools/package-playtest.py
```

Campaign continuation uses `--resume Artifacts/CampaignReplay/20261021T043558Z` against the driver's actual saved progress with unchanged source/binary hashes. Intermediate stops and retained segments are explicit in the report. Final current artifacts: native `Artifacts/Playtest/20261021T043218Z`, campaign `Artifacts/CampaignReplay/20261021T043558Z`, Android `Artifacts/Android/20261021T050403Z`, earned `Artifacts/EarnedPlaytest/20261021T045932Z`. Actual commands use isolated review saves, pointer/key events and native captures; no synthetic game rewards or real store receipts are substituted.

Build scripts call the Unity CLI with target/BuildProject methods, DOTNET_PROCESSOR_COUNT=4, affinity cores 0–3 and two job workers. Both release-style local builds report success/exit 0. Nonfatal authoring IAP cloud-sync warnings remain in the sanitized build logs; player exception and overflow checks pass. A previous global OOM with concurrent Editors/builds/emulator led to the sequential resource cap; no machine-wide process killer is used.

Tests cover legal/illegal placement, rotations, economy conservation, duplicate operations, full campaign witnesses, validated current saves and corruption recovery, unconfigured-store failure and paid-entitlement boundary fixtures. These fixtures do not prove real platform billing. All 24 building types are reachable; the production campaign and eight combo families have deterministic reference coverage. Blueprint slot 8, all six palettes, the last commission page and all achievement pages were exercised with legitimately earned progress.

Actual output/audio/performance samples and limitations: MOBILE_ACCEPTANCE.md. Portable ZIP integrity and executable permissions were checked. Genuine billing, physical devices, human engagement and commercial results are UNKNOWN.
