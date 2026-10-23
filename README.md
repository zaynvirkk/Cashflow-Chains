> Governing product source: [23October complete Game Bible](Docs/GameBible/CASHFLOW_CHAINS_FINAL_GAME_BIBLE.md), including both player experience and technical specification. [Source reconciliation](Docs/GameBible/SOURCE_REVIEW.md). Active implementation remains in progress.

# Cashflow Chains 2.0

An Android-first economic puzzle inside a persistent city progression game. Black Bag is on hold.

The [complete Game Bible](Docs/GameBible/CASHFLOW_CHAINS_FINAL_GAME_BIBLE.md) governs;the earlier [CC2 handoff](Docs/CC2/CASHFLOW_CHAINS_2_MASTER_REVAMP_SPEC.md) remains preserved alongside the [tracker](Docs/CC2/CC2_IMPLEMENTATION_TRACKER.csv), [system graph](Docs/CC2/CC2_SYSTEM_GRAPH.mmd) and [execution directive](Docs/CC2/CODEX_START_HERE.md). The older fixed store is a preserved baseline, not the completed monetization design. [Release status](Docs/CC2/RELEASE_STATUS.md) records what works and what remains.

The commercial goal is interconnected progression: legitimate chains feed owned businesses, rewards and recurring objectives; contextual acquisition routes and optional purchases serve those goals. Managers, unified chests/recruitment, merchants, offers, VIP, cards, events, passes and production billing remain tracked work. No retention or revenue outcome has been demonstrated.

## Run locally

- `./PlayEmpireSandbox.sh`: current CC2 development player, isolated save under Builds/EmpireSandbox/LocalEmpire. Actual local puzzle rewards,construction,production,dispatch/research and saved personal development plans with real material sources. Explicit local sandbox label; no cloud or purchases.
- `./Play.sh`: regular certified-puzzle companion. Existing saves and baseline entitlements remain separate. It ignores sandbox activation flags.
- `./PlayContentQA.sh`: separately labelled content fixture. It is not a normal progression or payment test.

Unity 6000.3.26f1. Run one Editor/build at a time; scripts bound CPU workers after an earlier laptop crash. F11 switches the Linux companion between full screen and a window. Android uses portrait touch controls and safe areas.

## Understand a connection

The board is a neighborhood plan. Homes represent residents; their path arrows lead to neighboring shop entrances. Cafés need customers and supplies to make sales. The opening café starts with 14 supplies; the next lesson introduces farm deliveries. Moving a piece edits the plan and does not charge the player rent. Rounds measure eight trading steps; round sales value and saved rewards are separate. An owned residential property in the permanent empire earns rental income separately.

Drag, or select then tap a destination. Rotate changes route direction. Undo and forecast are free. Standard puzzle results ignore purchased empire power.

## Build and verify

```bash
./Tools/test.sh
./Tools/build.sh linux
./Tools/build.sh empire-sandbox
./Tools/build.sh android
python3 Tools/cc2_content_validate.py
python3 Tools/cc2_economy_sim.py
```

The 104 reference layouts and deterministic eight-step resolver are retained. The latest domain suite passes 104 tests; fresh UI/Android acceptance is tracked separately. Native scripts use real input and isolated saves, never purchase grants or edited clocks. The preliminary 180-day simulator rejects the five-tier progression for content exhaustion and excess Cash; this is not a commercially approved economy.

Main and archive/pre-cc2-revision5 preserve baseline3ac5b62. Current work is on feature/cashflow-chains-2-economy. Builds, raw replays, private signing material and caches are local artifacts. No Play submission, hosting deployment, paid transaction or external account creation has occurred.
