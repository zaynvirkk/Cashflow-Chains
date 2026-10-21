# Cashflow Chains

Independent Unity 6.3 LTS pure 2D offline puzzle game. Full local playtest campaign: six locations and 72 free challenges, original flat artwork and music, progression, collections, settings and atomic save recovery. Original package and early prototype evidence remain preserved.

Run the laptop application with [`./Play.sh`](Play.sh). The Linux player opens at 540×960 and can be resized. Install the ARM64 APK from `Builds/Android/CashflowChains-Playtest-1.0.apk` for Android testing. These are local playtest builds, not published store releases.

Open this project root in Unity **6000.3.26f1** and open `Assets/_Game/Scenes/Main.unity`. Scene/settings are reproducible through Game → Rebuild scene. Close the Editor before running `Tools/test.sh` or `Tools/build.sh linux` / `Tools/build.sh android`. Run one Unity process at a time; the scripts cap CPU workers. Android: portrait, API 36 target, API 26 minimum, ARM64 IL2CPP.

Actual native input replays: `python3 Tools/playtest.py --display :1` and `python3 Tools/campaign-playtest.py --display :1`. They use an isolated review save directory. Android replay: `python3 Tools/android-playtest.py` with one compatible connected device (or use any Python environment with Pillow). F12 screenshots require explicit `-capture-dir /absolute/path`; `-save-dir /absolute/path` isolates saves. These options cannot grant progress or currency.

See [acceptance evidence](Docs/MOBILE_ACCEPTANCE.md), [implementation status](Docs/IMPLEMENTATION_STATUS.md), [original assets](Docs/ASSET_PROVENANCE.md), and [dependency notes](Docs/DEPENDENCY_ADDITIONS.md). Daily puzzles select from the certified offline campaign by UTC date. English is the current supported language.

No account, advertising, real cash redemption or gameplay server. Optional IAP code is pinned to Unity IAP 5.4.4 and fails closed without genuine store configuration; purchases are unavailable in this local edition. Publishing, genuine billing, physical phone QA, localization expansion and real-player retention/revenue remain separate release work. Steam, Windows and iOS releases are not asserted.
