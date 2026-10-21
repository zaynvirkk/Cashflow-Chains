# Cashflow Chains

A colorful, offline city-chain puzzle for Android. Connect customers and supplies to businesses, turn delivery arrows, and watch a useful layout turn into cash.

This private repository contains the ongoing **Android / Google Play production expansion**. The 72-city core remains free. The operator has requested the optional monetization model in the supplied master specification: starter, gems, draft rerolls, extra blueprints, cosmetic skins and tested scenario packs. Black Bag is a separate project and is on hold. See [the current work packages](Docs/EXPANSION_AND_COMMERCIAL_PLAN.md).

## Run locally

On Linux, build with Unity 6000.3.26f1 and run `./Play.sh`. The laptop player fills the screen; F11 switches to a window. Android uses portrait touch controls and adapts to the safe area.

Drag existing buildings or drag an available building from the tray. Tap a building to see its role, rotate its delivery arrows, and use Undo freely. Forecasts explain real consequences. Activate the chain to resolve eight deterministic beats and record your best result.

## Build and verify

```bash
DISPLAY=:1 ./Tools/test.sh
DISPLAY=:1 ./Tools/build.sh linux
DISPLAY=:1 ./Tools/build.sh android
python3 Tools/playtest.py --display :1
python3 Tools/campaign-playtest.py --display :1
python3 Tools/android-playtest.py
```

Run one Unity process at a time. The build scripts bound CPU workers after an earlier laptop memory incident. Builds, caches, private signing files and local review saves are not source dependencies.

The expanded domain contains 32 buildings, 72 free puzzles plus 32 optional expansion puzzles across eight districts, integer accounting, deterministic event logs, conservation checks and compatible atomic schema-2 saves. The first three starts teach connection, rotation and combining them. Outcomes depend on the layout; feedback never grants or changes money.

[Current release direction](Docs/ANDROID_RELEASE_DIRECTION.md) · [Project state](STATE.md) · [Engineering evidence](Docs/MOBILE_ACCEPTANCE.md) · [Art provenance](Docs/ASSET_PROVENANCE.md)

**Work in progress:** this checkpoint includes new content, presentation and billing integration that are still being compiled and replayed. The last accepted expansion domain run passed 52 tests, including every one of the 104 reference solutions and all 16 combo conditions. This is not a final release signoff. Historical packages and screenshots are not proof for changed source. Play Console signing/account steps, public listing/privacy details and any required external closed test are separate from local engineering verification. No store submission has occurred.
