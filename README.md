> Governing product source: [23October complete Game Bible](Docs/GameBible/CASHFLOW_CHAINS_FINAL_GAME_BIBLE.md), including both player experience and technical specification. [Source reconciliation](Docs/GameBible/SOURCE_REVIEW.md). Active implementation remains in progress.

# Cashflow Chains 2.0

An Android-first game about building a private investment portfolio inside an established city. Black Bag is on hold.

The latest [investor-in-an-existing-city decision](Docs/Decisions/2026-10-24-Investor-In-An-Existing-City.md) supersedes empty-city founding and the mandatory home→café opening. The [complete Game Bible](Docs/GameBible/CASHFLOW_CHAINS_FINAL_GAME_BIBLE.md) governs;the earlier [CC2 handoff](Docs/CC2/CASHFLOW_CHAINS_2_MASTER_REVAMP_SPEC.md) remains preserved alongside the [tracker](Docs/CC2/CC2_IMPLEMENTATION_TRACKER.csv), [system graph](Docs/CC2/CC2_SYSTEM_GRAPH.mmd) and [execution directive](Docs/CC2/CODEX_START_HERE.md). The older fixed store is a preserved baseline, not the completed monetization design. [Release status](Docs/CC2/RELEASE_STATUS.md) records what works and what remains.

The commercial goal is interconnected progression: legitimate chains feed owned businesses, rewards and recurring objectives; contextual acquisition routes and optional purchases serve those goals. Managers, unified chests/recruitment, merchants, offers, VIP, cards, events, passes and production billing remain tracked work. No retention or revenue outcome has been demonstrated.

## Run locally

- Android test APK:`Builds/Release/CashflowChains-1.0.0.apk`. Current verification targets Android only;the visible emulator uses an ordinary separate PlayCity3 save.
- `./PlayNewCity.sh`: historical desktop companion launcher;older saves are preserved.
- `./Play.sh`: historical desktop companion;not the current runtime test target. Your named city, owned plots, buildings, obligations and operating days persist on this device.
- `./PlayEmpireSandbox.sh`: separately built developer player with an isolated save and visible local-sandbox label. Rebuild with `Tools/build.sh empire-sandbox` before using it after source changes.
- `./PlayContentQA.sh`: separately labelled historical puzzle content fixture; it is not a normal progression or payment test.

Unity 6000.3.26f1. Run one Editor/build at a time; scripts bound CPU workers after an earlier laptop crash. F11 switches the Linux companion between full screen and a window. Android uses portrait touch controls and safe areas.

## Understand your city

You enter an established district. Other owners’ residential blocks,municipal services,industry,utilities,wholesale supply and transport already exist. The modeled local catchment starts with48 actual residents in18 homes owned by others. Those homes and civic buildings pay you nothing. Only private assets you acquire and develop contribute to your saved cashflow.

Compare a rental property or a grocery business as your first investment. A rental can operate without owning a café;the café becomes available after your first real positive operating day. Households have finite budgets,ages and preferences. Public/regional employment reserves workers before your businesses hire. Existing wholesale stock and electricity are usable services with actual metered/published costs. Farms,factories,warehouses,utilities,ports and hotels become private investment opportunities rather than prerequisites for creating a city from nothing.

The investor career gives six concrete milestones:an owned asset,positive settled cashflow,two income streams,$100 in positive operating cashflow,an upgraded asset,and$100 daily net. These use actual state and transactions. Existing transit supplies a small finite visitor pool;an owned port expands transport rather than creating tourism from nothing. A predictable operating-day calendar changes import capacity and grocery demand while budgets remain finite. Public services supply context;this is not a complete emergency,medical,legal or auction simulation.

Customer payments flow toward your businesses;coffee/services flow back. Actual supply crates come from producers or wholesale storage. Forecasts separate income,costs and net;prices are shown per sale. Construction/fit-out consumes Cash,materials and builder time. Optional fictional financing discloses its fixed fee,installments and early repayment. Retain one portfolio,pan/zoom the existing district and acquire adjacent investment plots. Old owned assets,wallets and committed trading-day promises are preserved. There is no skip button.

The deterministic 104-puzzle archive remains preserved for compatibility and regression testing. It no longer supplies a sequence of replacement cities. Standard puzzle outcomes still ignore purchased power.

## Build and verify

```bash
./Tools/test.sh
./Tools/build.sh android
python3 Tools/cc2_content_validate.py
python3 Tools/cc2_economy_sim.py
```

The 104 reference layouts and deterministic eight-step resolver are retained. The current investor-context suite passes196 Editor tests and24 local authority tests. Its new signed Android replay is pending during compilation;older runtime evidence is historical. Native and Android acceptance are recorded separately in [persistent-city progress](Docs/CC2/PERSISTENT_CITY_PROGRESS.md). Native scripts use real input and isolated saves, never purchase grants or edited clocks. The preliminary 180-day simulator rejects the five-tier progression for content exhaustion and excess Cash; this is not a commercially approved economy.

Main and archive/pre-cc2-revision5 preserve baseline3ac5b62. Current work is on feature/cashflow-chains-2-economy. Builds, raw replays, private signing material and caches are local artifacts. No Play submission, hosting deployment, paid transaction or external account creation has occurred.
