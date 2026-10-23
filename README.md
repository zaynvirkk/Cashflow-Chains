> Governing product source: [23October complete Game Bible](Docs/GameBible/CASHFLOW_CHAINS_FINAL_GAME_BIBLE.md), including both player experience and technical specification. [Source reconciliation](Docs/GameBible/SOURCE_REVIEW.md). Active implementation remains in progress.

# Cashflow Chains 2.0

An Android-first game about developing one persistent city and improving its cashflow. Black Bag is on hold.

The [complete Game Bible](Docs/GameBible/CASHFLOW_CHAINS_FINAL_GAME_BIBLE.md) governs;the earlier [CC2 handoff](Docs/CC2/CASHFLOW_CHAINS_2_MASTER_REVAMP_SPEC.md) remains preserved alongside the [tracker](Docs/CC2/CC2_IMPLEMENTATION_TRACKER.csv), [system graph](Docs/CC2/CC2_SYSTEM_GRAPH.mmd) and [execution directive](Docs/CC2/CODEX_START_HERE.md). The older fixed store is a preserved baseline, not the completed monetization design. [Release status](Docs/CC2/RELEASE_STATUS.md) records what works and what remains.

The commercial goal is interconnected progression: legitimate chains feed owned businesses, rewards and recurring objectives; contextual acquisition routes and optional purchases serve those goals. Managers, unified chests/recruitment, merchants, offers, VIP, cards, events, passes and production billing remain tracked work. No retention or revenue outcome has been demonstrated.

## Run locally

- `./PlayNewCity.sh`: try the new empty-city opening with its own ordinary persistent save; existing cities are preserved.
- `./Play.sh`: regular permanent-city player. Your named city, owned plots, buildings, obligations and operating days persist on this device.
- `./PlayEmpireSandbox.sh`: separately built developer player with an isolated save and visible local-sandbox label. Rebuild with `Tools/build.sh empire-sandbox` before using it after source changes.
- `./PlayContentQA.sh`: separately labelled historical puzzle content fixture; it is not a normal progression or payment test.

Unity 6000.3.26f1. Run one Editor/build at a time; scripts bound CPU workers after an earlier laptop crash. F11 switches the Linux companion between full screen and a window. Android uses portrait touch controls and safe areas.

## Understand your city

You begin with empty land and zero residents. Homes are rental properties you build and own. Actual households vary in size,age,retirement,budget and preferences;capacity is not occupancy. Only occupied dwellings pay rent. Children create no coffee demand;workers must be18–64. Adult preferences and household budgets limit purchases;gender is identity,not a wage or price multiplier. A café competes for those customers and needs actual beans, cups, staff and power. A farther address increases travel time and freight costs; it does not make a nearby-only connection mandatory.

Coffee/service flows travel out; customer payments travel in. Forecasts show sales and rent less operating bills, imported supplies, tax and debt payments. Advertising competes for existing demand and costs money even when it fails. Farms and factories produce physical stock rather than magical income. Warehouses, utilities, ports and hotels have distinct capacities and running costs. Expand owned land, move properties, set prices, hire employees and compare investments while the city operates.

Construction consumes Cash and materials and occupies a builder. An owned construction company produces materials and earns another builder at level2. Optional fictional financing has a fixed disclosed fee, ten scheduled installments and early repayment. It is separate from real-money purchases. Initial builds and operating days are quick; later days become longer. There is no skip button. Name the city, retain its layout, pan/zoom and develop several income streams on the same map.

The deterministic 104-puzzle archive remains preserved for compatibility and regression testing. It no longer supplies a sequence of replacement cities. Standard puzzle outcomes still ignore purchased power.

## Build and verify

```bash
./Tools/test.sh
./Tools/build.sh linux
./Tools/build.sh empire-sandbox
./Tools/build.sh android
python3 Tools/cc2_content_validate.py
python3 Tools/cc2_economy_sim.py
```

The 104 reference layouts and deterministic eight-step resolver are retained. The current Unity suite passes175 tests; native and Android acceptance are recorded separately in [persistent-city progress](Docs/CC2/PERSISTENT_CITY_PROGRESS.md). Native scripts use real input and isolated saves, never purchase grants or edited clocks. The preliminary 180-day simulator rejects the five-tier progression for content exhaustion and excess Cash; this is not a commercially approved economy.

Main and archive/pre-cc2-revision5 preserve baseline3ac5b62. Current work is on feature/cashflow-chains-2-economy. Builds, raw replays, private signing material and caches are local artifacts. No Play submission, hosting deployment, paid transaction or external account creation has occurred.
