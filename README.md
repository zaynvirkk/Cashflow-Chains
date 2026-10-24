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

You enter a fully populated city with no owned properties. The8×8 core already has62 existing parcels:restaurants,cafés,bars,clubs,hotels,houses,apartments,offices and industry. Municipal services and supply/transport infrastructure are already present. Only two outskirts lots start vacant. Other owners’ income never enters your wallet.

Choose an advertised existing asset. The full in-game price is charged once,and a real three-second closing transfers ownership. The existing rental tenants and total population stay in place. Initial deals include a rental,a café and a grocer;later offers follow actual operating days. Building materials are not charged for acquisition. You can renovate owned assets and later develop rare vacant outskirts land.

Finite household budgets,ages,preferences and staffing affect demand. Real goods,delivery distance,utility usage,tax and financing reduce private income. A forecast shows the asset’s own income/bills/net and the change to the whole portfolio. The career path advances from actual ownership,positive settlement,diversification and development. Public civic services are setting/context;full emergency,medical,legal and auction systems are not claimed.

The map-led HUD uses compact money/people/day instruments,a cashflow strip,sale pins and ownership badges. Detailed bills and terms remain inspectable. Private properties,saves and earlier committed region0/1 trading promises survive the metropolis transition. There is no skip button.

The deterministic 104-puzzle archive remains preserved for compatibility and regression testing. It no longer supplies a sequence of replacement cities. Standard puzzle outcomes still ignore purchased power.

## Build and verify

```bash
./Tools/test.sh
./Tools/build.sh android
python3 Tools/cc2_content_validate.py
python3 Tools/cc2_economy_sim.py
```

The 104 reference layouts and deterministic eight-step resolver are retained. The current investor-context suite passes204 Editor tests and25 local authority tests. Its new signed Android replay is pending during compilation;older runtime evidence is historical. Native and Android acceptance are recorded separately in [persistent-city progress](Docs/CC2/PERSISTENT_CITY_PROGRESS.md). Native scripts use real input and isolated saves, never purchase grants or edited clocks. The preliminary 180-day simulator rejects the five-tier progression for content exhaustion and excess Cash; this is not a commercially approved economy.

Main and archive/pre-cc2-revision5 preserve baseline3ac5b62. Current work is on feature/cashflow-chains-2-economy. Builds, raw replays, private signing material and caches are local artifacts. No Play submission, hosting deployment, paid transaction or external account creation has occurred.

## Investor reward network

The rewards chest on the city map opens daily supplies,permanent investor milestones,a 28-day season,earned chest inventory/history and optional product previews. The seven-claim calendar retains progress after missed visits. Real acquisitions,upgrades and positive operating days award capped season XP. Claims are durable and idempotent;earned chest odds are inspectable. New paid tracks have tested entitlement rules but checkout is unavailable in this local build. See [commercial status](Docs/CC2/COMMERCIAL_RETENTION_PORTFOLIO.md).
