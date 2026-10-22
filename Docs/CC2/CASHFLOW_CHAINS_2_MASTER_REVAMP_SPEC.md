# CASHFLOW CHAINS 2.0 — COMPLETE GAME-ECONOMY, MONETIZATION, RETENTION & PRODUCTION MASTER SPEC

**Status:** Implementation directive; supersedes conflicting gameplay/commercial direction in older repository documents after change-control/migration checks.  
**Prepared:** 21 October 2026  
**Target:** Existing Unity 6.3 / Unity IAP 5.4.4 Android-first project, repository `https://github.com/zaynvirkk/Cashflow-Chains`  
**Owner decision:** Preserve working chain puzzle and content; comprehensively rebuild persistent economy, progression, rewards, UX, LiveOps, shops, offer engine, backend and telemetry.  
**Primary business goal:** Maximize *sustainable* contribution-margin-adjusted player lifetime value (LTV) while maintaining satisfying free play, strong retention, trust and feasible acquisition cost. No guaranteed revenue result exists; implement, measure, iterate.

---

## 0. CODEX EXECUTION CONTRACT — READ BEFORE EDITING

This is an **implementable product/engineering specification**, not permission to report success from mock screenshots, local-only billing or unchecked code generation. Codex must implement every subsystem, content definition, interface, event, migration, acceptance test and service boundary defined here. The work can be broken into sequential commits. Do not silently omit an item because it is expensive. Where a live external prerequisite is unavailable, finish the local implementation, tests, fixtures and deployment instructions, mark `BLOCKED_EXTERNAL` with the exact missing input, and continue independent work. Do not pretend the blocking prerequisite was satisfied.

1. Read `AGENTS.md`, `STATE.md`, `PRODUCT.md`, `DESIGN.md`, `Docs/01_CASHFLOW_CHAINS_MASTER_SPEC.md`, `Docs/SHARED_TECH_MONETIZATION_LAUNCH.md`, `Docs/COMMERCIAL_IMPLEMENTATION.md`, `Docs/EXPANSION_AND_COMMERCIAL_PLAN.md` and the **actual current sources**. Preserve original historical docs; write `Docs/Decisions/2026-10-21-CC2-Commercial-Rebuild.md` explicitly superseding incompatible rules (e.g. old ban on paid randomized rewards, server, ads, seasons) while **retaining** security, playability, accessibility and provenance requirements. Modify `AGENTS.md` to point at this specification. If source has changed from the audited baseline, reconcile differences and document them before editing.
2. Create git branch `feature/cashflow-chains-2-economy`; baseline commit, test report, baseline screenshots and reproducible current build. Never discard current authored puzzles, valid player saves, purchased entitlements or art rights documentation. Maintain a deterministic offline puzzle mode even when backend is down.
3. Develop in **gated vertical slices**, not a speculative one-shot rewrite. Each slice must compile, run, test, and generate actual screenshots. Ensure every advertised control operates; no fake products, invented prices, decorative buttons, simulated IAP success, invisible blockers or hardcoded countdowns in release.
4. Use strongly typed C# domain commands and events. Domain logic cannot depend on Unity UI. Presentation may never directly grant items, mutate purchased balances, decide random rewards, or alter authoritative outcomes.
5. Paid/economically valuable assets require server-authoritative state and transactions. Retain free offline puzzles; reconcile locally earned free progress via replay proofs with bounded, anti-replay backend acceptance. Never trust device clocks, client-selected premium rewards, client-calculated payments or client event scores.
6. All chance-based purchasable outcomes and paid random refreshes must disclose actual effective odds **prior to and close to purchase**. Clear real-money prices from platform metadata; unambiguous currency conversions and quantity. Paid premium currency does not expire. No redeemable cash, gambling payouts, fake urgency, misleading discounts, hidden automatic renewal, fake player activity or compelled ad viewing.
7. Build for adult/general audiences with an age-appropriate rating. Do not target minors with personalized high-pressure offers. Implement age/region policy feature flags and privacy/consent flows as applicable. All offers can be ignored without breaking ordinary puzzles.
8. Report scope, code changes, tests and failing tests, build IDs, captured screens, known defects, metrics, backend setup and external blockers after **every phase**. `DONE` requires all stated gates and platform dependencies, not merely successfully generating code.

### 0.1 Specific baseline established from repository audit

- Domain sources: `Assets/_Game/Scripts/Domain/Cashflow.cs` (`Pieces`, `Board`, deterministic 8-beat `Resolver`, conservation) and `Campaign.cs` (`CityRun`, forecasts, missions, optional draft reroll).
- Existing: **32 piece recipes**, **72 free + 32 optional expansion challenges**, **16 flow-validated combo definitions**, **8 discovery sets**, **15 one-time route contracts**, **30 milestone achievements**; Unity 6000.3.26f1, URP 2D, Unity Purchasing 5.4.4 per repository snapshots.
- Key sources: `Profile.cs`, `CommerceCatalog.cs` (9 current product IDs), `CityCollections.cs`, `Contracts.cs`, `Daily.cs`, `ExpansionCampaign.cs`, `MobileGameApp.cs`, `CityProgressionUI.cs`, `LocalStore.cs`, `PurchaseDelivery.cs`, `AtomicSave.cs`.
- Current `Profile` schema 2 assumes small finite credit income, a limited receipt ledger, at most 104 level results, and narrow gem usage; it **must not** be extended by bypassing validation. Introduce an independent, versioned CC2 account/economy state and migration.
- Existing score from each puzzle is **not** autonomous idle income; persistent production is new and must be real, capped and separately accounted. Round cashflow is a **score**, not money to be withdrawn.
- Current IAP code is platform-sensitive and stores consumable state locally; use it only as an adapter starting point and reconcile server-authoritatively. Preserve ownership of `cc.start.01`, `cc.blueprint.4`, `cc.skin.neon`, `cc.scenario.harbor`, `cc.scenario.finance` plus any legitimately purchased consumables (where receipts and local history permit).
- Preserve `Campaign.Get`, `ExpansionCampaign.Get` and `Resolver` semantics for existing levels. No purchased power may silently alter certified original solutions, conservation or pre-existing results. The richer engine applies power in explicitly labeled empire/expedition modes, not fixed puzzle scoring.
- The screenshot's bright, lush, high-contrast miniature city, bold CTA and readable flow arrows are the **visual identity**, not proof of currently implemented economics.

### 0.2 Definition of success and non-guarantees

Success requires: (i) fun tactile 2D puzzling, (ii) sustainable persistent progression, (iii) numerous meaningful free reward/paid acceleration paths, (iv) robust commerce and authoritative inventory, (v) evolving event content, (vi) measured player retention and monetization. No mechanic guarantees revenue. Optimize cohort contribution margin: `LTV(IAP + ads) - acquisition spend - platform/payment fees - backend/support/LiveOps costs` with retention and refund risk monitored.

---

## 1. NON-NEGOTIABLE GAME VISION

**Pitch:** Build clever miniature-city economic chains, watch cash and goods pulse through them, then use their output to grow a permanent business empire: recruit executives, own/upskill franchise businesses, research technologies, operate deliveries, complete collections, explore rotating merchants and participate in live economic festivals.

Three coupled experiences:

1. **Moment-to-moment play (30 seconds–5 minutes):** drag, snap, rotate, reroute, forecast and activate 8-beat flows. Skill, experimentation, surprising but explainable combinations and lush tactile feedback remain the core entertainment.
2. **Persistent operating empire (minutes to months):** owned businesses, levels, stars, equipment, manager recruitment, tech, construction slots, dispatch contracts, storage and capped passive output. Upgrades influence eligible non-certified empire sessions/expeditions; original puzzle campaign stays fair and deterministic.
3. **Recurring LiveOps (days to seasons):** quests, collectible albums, weekly challenges, prize tracks, VIP, event shops, rotating merchant inventory, seasons, special generations, asynchronous leagues and cooperative projects. One valid action can advance several tracks.

**Core principle:** Every significant game action produces relevant gameplay/collection/progression/reward feedback. Every permanent goal has at least one real free acquisition route; premium products offer speed, extra capacity, flexibility, cosmetics or stronger eligible account development. The game must remain enjoyable if a player never purchases or watches an advertisement. Not every tap opens commerce; commerce is **available contextually**, not interruptive by default.

### 1.1 Preserve versus change

PRESERVE: deterministic 8-beat solver and trace; conservation and caps; 72+32 puzzles and their legitimate ownership; real connection logic; free forecast and undo; building/art/audio assets and visual identity; city/contract concepts; responsive Android UX and accessibility; atomic local saves; test harness.

REBUILD: finite credits-only progression; one-time contract economy; flattened profile; hardcoded nine-SKU shop; narrow gem sinks; discovery-only collections; generic result screen; non-replenishing reward surface; hardcoded monolithic mobile UI controller; lack of server config, telemetry, content versioning, live offers and event engine.

### 1.2 Modes (explicit paid-power boundaries)

| Mode | Eligibility/power | Rewards and purpose |
|---|---|---|
| Certified Campaign | Fixed challenge board and owned-free rules; paid power excluded | Original 72 free + 32 owned extra levels, stars, first-clear rewards, mastery. |
| Open City / Empire Operations | Owned businesses, managers, equipment and researched perks apply | Repeatable cash/materials/XP within caps and daily budgets. |
| Dispatch Contracts | Send **validated saved layouts** into timed renewable commissions | Materials, tokens, fragments, cash, executive experience. |
| Daily Skill Puzzle | Same predetermined puzzle for everyone; paid power excluded | Earned reward, skill ranking where server verified, free entry. |
| Expeditions / Festivals | Optional limited repeatable objectives; owned development may count | Seasonal rewards, event-score contributions and tokens. |
| Cooperative Projects | Asynchronous verified resource/effort donations | Shared milestones and alliance gifts, no simulated members. |
| Empire League | Explicitly progression-influenced leaderboard | Account power competitions; separate from equitable skill leaderboard. |

No invented mobile real estate or tradeable actual financial product; all currency is purely fictional gameplay value.

---

## 2. THE ECONOMIC NETWORK: SOURCES, ROUTES, SINKS, REWARDS

The player-centric graph is:

`meaningful action -> deterministic result -> GameDomainEvent -> RewardTransaction -> inventory/progression/event/collection/pass/VIP -> new unlocked objective -> acquisition routes (play, wait, exchange, earn, random draw, optional pay) -> action`.

Each item is a canonical `ItemDefinition`. Each acquisition route is a `SourceDefinition`. Each consumption is a `SinkDefinition`. Each reward container is a `LootTableDefinition`. Each real-money arrangement is an `OfferDefinition`. **Do not conflate these.** The *same* franchise fragment can be obtained through a contract, merchant, choice chest, random chest, event, alliance milestone and purchase.

### 2.1 Mandatory currency/item taxonomy

| Canonical ID / display | Free sources | Principal sinks | Rules |
|---|---|---|---|
| `CASH` / City Cash | Puzzle completion grants; capped open-city production; dispatch, quests | Buildings, research, upgrade fees, trades | Inflation-sensitive; cap and tune sinks. Distinct from `ROUND_SCORE`. |
| `GEMS` / Spark Gems | Achievements, events, pass free milestones, very modest login/quest earnings; IAP | Recruitment pulls, paid merchant refresh, flexible speedup, selected packs | Purchased Gems cannot expire. Show exact gem price; no client authority. |
| `WOOD`,`STEEL`,`PLANS` / construction materials | Production, dispatch, puzzle rewards, chests, merchant | Build/upgrade business/facilities, advanced construction | Three materials max in early game. |
| `RESEARCH_DOCS` | Contracts, free event missions, chests | Technology projects | Separate research time. |
| `BIZ_FRAG:{id}` | Franchise contracts, chests, collections, merchant, events | Business acquisition and star upgrades | Specific to business ID; duplicate conversion. |
| `BIZ_FRAG_UNIVERSAL:{rarity}` | Rare milestones, eligible events, choice chests | Eligible selected business fragments | Conversion rate explicit and limited. |
| `MANAGER_FRAG:{id}` | Recruitment, contracts, event awards, exchange | Acquire/promote manager | Prominent collection progress. |
| `MANAGER_FRAG_UNIVERSAL:{rarity}` | Free milestone, shop, premium tracks | Selected manager promotions | Universal substitutes scarce and valuable. |
| `MANAGER_XP` | Ordinary use, training contracts, chests | Level manager | Subject to level cap per star rank. |
| `SKILL_BOOK` | Events, dispatch, chest pools | Manager active/passive skill levels | Costs scale distinctly. |
| `GEAR_PARTS`,`GEAR_CORES` | Salvage, expedition, chests | Equipment improvement and mastery | Rare cores later game. |
| `CONSTRUCTION_SPEEDUP_MIN` | Earned missions, VIP gifts, event rewards, packs | Construction time reductions | Time denomination recorded in minutes. |
| `RESEARCH_SPEEDUP_MIN`,`DISPATCH_SPEEDUP_MIN` | Similar | Specialized timer reductions | Never secretly interchangeable unless `GENERAL_SPEEDUP_MIN`. |
| `RECRUIT_TICKET` | Onboarding milestone, missions, events, choice packs | Manager recruitment | 1 ticket = 1 draw; no cash-out. |
| `MERCHANT_VOUCHER` | Daily missions, merchant exchange, events | Random shop reroll | One initial free daily reroll by rule, escalating paid extra refresh if active. |
| `CONTRACT_TOKEN`,`ARENA_TOKEN`,`ALLIANCE_TOKEN` | Respective activities | Respective exchange shops | Not broadly convertible to Gems. |
| `EVENT_TOKEN:{season}` | Event tasks and passes | Time-bounded event reward shop | Distinguish earned temporary tokens from any paid Gem balance. Explicit expiration on earned event-only tokens. |
| `VIP_XP` / Influence | Limited gameplay milestones and eligible purchases | Permanent VIP level advancement | This is XP, not a spendable coin; no decay. |
| `COLLECTION_FRAGMENT:{album}:{piece}` | Pack opening, expedition, event, gifting where allowed | Collection completion/exchange | Rare trade restrictions explicit. |
| `THEME_TOKEN` | Play/collections and cosmetics bundles | Non-power skins and personalizations | Cosmetics are genuine visible upgrades. |

Avoid exposing all currency names at once. HUD default: Cash + Gems, current limited event currency if relevant; others live in context/inventory.

### 2.2 Wallet/ledger invariants

- Every item grant/spend is `LedgerEntry{entryId,accountId,itemId,delta,reason,sourceId,correlationId,configVersion,createdAt}` with unique idempotency key. No negative spend balance, no unbounded integer overflow, no doubly claimed mission/receipt.
- `ROUND_SCORE` is a computed puzzle output; `CASH` only changes through explicit `RewardPolicy` after eligibility and replay verification. Do not grant unlimited cash proportional to arbitrarily repeated easy-score runs.
- Server stores purchased versus earned balances where laws/expiry/transfer rules differ. Purchased Gems always spent last or first under a published, consistent rule; no expiry. Never consume bought Gems for a free-only reward without explicit action.
- Award on complete/claim is atomic: ledger + inventory + milestone progress + relevant counters in one transaction. Event listener retries are idempotent.
- Economy events emitted once with stable `roundId`, `contractId`, `chestOpenId`, `purchaseTransactionId` or `rewardClaimId`.
- Offline proof: signed/challenge seed + board placements + base content version; replay `Resolver` server-side or in a trusted verifier; require budget, legal pieces, timeline and mission constraints. Rate-limit rewards and disallow editing stored result IDs.

### 2.3 Acquisition route matrix — must be implemented

Every important scarce material requires 2+ non-purchase routes (at least one repeatable, sustainable route, except explicitly cosmetic/exclusive items). Source finder shows earned activity, shop currency, random chest odds, event availability and optional offers in that order. Source finder must filter impossible routes and show next availability accurately. No dead-end 'GET' button.

### 2.4 Sinks and coupled value

- Faster construction increases material consumption, which increases reward demand; do not fabricate scarcity by secretly throttling after purchases.
- More contracts raise resource opportunities subject to dispatch capacity and daily server cap.
- More business stars unlock ability breakpoints and higher content; managers/equipment complement business quality rather than replicate it.
- Manager gear uses its own parts and cores; skill books separate skill progression from XP.
- Research improves defined systems through a transparent stacking and cap formula.
- New generations add new **optional** advanced progression and controlled new resources, not mandatory replacements that invalidate all existing heroes.
- Coins gained by repeatable play need renewable sinks: dispatch costs, construction, research, crafting, trader exchanges and maintenance of optional operations (not punitive loss of owned items).

---
## 3. PERSISTENT EMPIRE — EXACT FEATURE SPEC

### 3.1 Distinguish recipes, owned assets and board instances

- `PieceDefinition` = deterministic existing recipe: `home`, `cafe`, `farm`, `warehouse`, etc., with graph inputs/outputs and legal ports.
- `BusinessDefinition` = progression metadata for a franchise type: rarity, role, unlock and fragments, upgrade tree, production outputs, optional active ability, cosmetics and permitted modes.
- `OwnedBusinessState` = player's level, stars, XP, equipment, skin and one or more location installations.
- `BoardNode` = an instance of a puzzle piece. Certified puzzles ignore owned upgrade bonuses. Open-City operations attach snapshot of eligible owned benefits *after checking stacking caps*, never rewrite base `Piece` recipes.

For each existing 32 recipe: map into residential, retail, industrial, logistics, energy, civic and financial groups. Introduce 8-12 fully designed initial persistent business assets; retain the other recipes available on fixed puzzles and unlock permanent empire progression gradually. New businesses must add functional differentiation: extra throughput, longer permitted paths, better energy efficiency, conversion flexibility, contract yield, synergy tags or storage behavior. Avoid 32 reskinned +5% stat sticks. Strong uncommon business ownership starts via earned progress after the player understands chains; do NOT hand out max-rarity hero or franchise on installation / invent a Day 1 grant.

### 3.2 Business progression model

`BusinessState{id,owned,level,starRank,fragmentCount,abilityLevel,equipmentSlots,skinId,assignedManagerId,installationIds}`. Rarity: Common, Rare, Epic, Legendary. Star progression is separate from level and requires specific or eligible universal fragments. Stars can unlock ability levels and level caps. Upgrade result and sources shown before spending.

**Default tuning hypotheses (all numbers are content-config and must be simulated):**

| Milestone | New fragments | Cumulative including unlock | Meaning |
|---|---:|---:|---|
| Acquire | 10 | 10 | Usable permanent business |
| Star 1 | 10 | 20 | Efficiency breakpoint |
| Star 2 | 25 | 45 | Additional operating capability |
| Star 3 | 55 | 100 | Distinct special ability |
| Star 4 | 110 | 210 | Better cross-system synergy |
| Star 5 | 220 | 430 | Late mastery |

Upgrades take Cash + materials + queue time; first few are near-instant to teach mechanics, later timers meaningful. Level cap by star, with server-validated acceleration. Rarity influences reward pools and resource requirements but must not create an inaccessible required certified puzzle.

### 3.3 Persistent buildings and production

Persistent Empire map with 6–12 initially available plots (more unlockable). Install franchises and facilities. Each facility `ConstructionProject{startedAt,finishAt,config,queueSlot}`. Base 1 builder; earn temporary speedups; sell/earn a permanent second builder, later carefully gated further capacity. 1–2 freely unlockable plots early, additional via milestones. Construction can be started during simultaneous play; no mandatory puzzle wait.

Production model: each installation produces bounded material/cash output while active, subject to hourly rate, storage cap, production eligibility and **maximum 8 hours of offline accumulation by default**. Calculate in trusted service using timestamp and facility state; avoid infinite passive compounding and reject wall-clock tampering. UI shows real rate, capacity, accumulated output and when storage fills. `Collect` is a satisfying claim action, but support convenient batch-collection and optional auto-collection without using intentionally tedious micromanagement as the primary paywall. Persistent economy must never permit value overflow or exponential runaway.

### 3.4 Managers (hero analogue)

Initial pool: 12 named original managers, 4 basic/common, 4 rare, 3 epic, 1 legendary; additional generations data-driven. Actual names, voice and illustrations original; no copied Whiteout/Kingshot characters. Roles: Operator (throughput), Developer (build), Financier (cash/research), Logistics Chief (dispatch), Merchant (exchange), Specialist (conditional new graph abilities). Each manager has levels, star ranks, specific + universal fragments, XP, two skill categories (operations/expedition), optional exclusive equipment at upper rarity, assignment limit and clearly described bonus formula. Starter manager earned through normal milestone, desirable higher-rarity manager acquired later through a guaranteed *free* progression reward or earnable recruitment, then developed through ongoing play. An introductory paid manager may be optional alongside the earnable manager; never falsely call it exclusive if available free.

Player assigns managers to owned businesses; disallow duplicate manager assignments where physically impossible. Unassigned managers retain collection/progression utility. Strategy: reassign based on contract type, business synergy or event requirements; presets save combinations. Guaranteed recruitment milestone provides reliable long-term target. Repeated manager level-up shouldn't require paid materials. Sell faster acquisition, equipment and flexible selection through multiple routes.

### 3.5 Equipment and specialist materials

3 generic gear slots per manager at first unlock, expand to 4 only after engagement supports it. Gear rarities, levels, mastery and conditional set bonuses; stat caps and diminishing returns. `GEAR_PARTS` (frequent), `GEAR_CORES` (rare), duplicate salvage, crafted choice boxes. Equipment must visibly improve eligible output/dispatch/battle analogues, not certified puzzle score. A special tool for logistics may increase delivery slots; a finance tool may improve contract profitability. No gambling-dependent indispensable tool for basic play.

### 3.6 Research and technology

Three initial trees: `Operations` (production throughput, storage, collection), `Logistics` (dispatch time, fleet capacity, contract returns) and `Innovation` (construction/research efficiency, higher progression access). Each node: costs, duration, prerequisites, explicit benefit tags, max level and cap. Show graph, currently affordable research and free versus paid routes. Research speedups and optional specialization. VIP/manager bonuses stack in *additive basis points with documented caps* (e.g. max 50% time reduction excluding explicit time-skip consumables) to avoid runaway synergy.

### 3.7 Dispatch contracts and renewal

Replace 15 one-time commissions with recurring contract templates + rotating offers. Reuse `ContractDesk`'s saved-layout verification and scheduled claim concepts. Contract eligibility requires an actually demonstrated chain/layout or sufficiently leveled business. Contract: `id, template, tier, requirements, seed, cost, duration, yieldPool, assignedManager, dispatchSlots, eventTags, start, finish, claimId`.

- Base 1 active dispatch, unlock second through free progression, optional purchased third or premium utility; cap according to economy simulation.
- Free common contracts always available. Rare contracts can appear via rotating board or earned contract tickets; selective rerolls using vouchers/Gems only for optional better opportunities.
- Durations vary by tier: initial 60s/3min/15min, then 1h/4h/8h for recurring progression; selected players may shorten via earned/purchased speedup, VIP or manager bonus.
- One-click claim, reroute/restart, auto-restart convenience after legitimate unlock; no paid mandatory background feature for reasonable enjoyment.
- Rewards determined on dispatch by versioned server rules or on claim by predetermined seed; never generated multiple times upon returning online.

### 3.8 Storage and capacities

Expose exactly what limits production: builder slots, storage capacity, operating slots, active dispatch slots, management assignments, optional event attempts. Show both upgrade requirements and source finder. Limit scarcity to meaningful decisions, not arbitrary interruptions. Optional boost types: +production speed, +contract capacity, +research speed, +gathering/collection yield, temporary merchant discount, earned skill bonus. Keep certified skill mode neutral.

---

## 4. UNIFIED REWARD ENGINE — CHESTS, GACHA, COLLECTIONS AND REVEALS

### 4.1 Mandatory reward delivery modes

1. Direct deterministic item grant (cash/materials/fragments).
2. Earned randomized loot chest; 3–5 rarity tiers; table-driven weights; guaranteed rewards as separate line items.
3. Purchased randomized chest: exact odds disclosed nearby, explicit contents, server RNG and receipt-linked fulfillment; no hidden adaptive odds.
4. Rarity-themed containers (business, manager, equipment, research, event and collection chests).
5. Choice chest with allowed selection list and explicit quantities; consume chest only on confirmed selection.
6. Ten-open batch and fast-open with identical results/odds and full accessible result history.
7. Chest merge: choose to open N low-tier chests or exchange a disclosed quantity for one higher tier; verify expected-value fairness and exact distributions. No misleading visual odds.
8. Nested chest only if explicit nested probabilities and outcome disclosure supported; do not deploy if actual odds are too complex to communicate reliably.
9. Duplicate conversion (eligible duplicates -> universal shards or album tokens) with published ratios.
10. Fragment albums and collect-sets: partial progress, targeted earned exchange, optional gifting of eligible common fragments, rare trade restrictions made explicit; full set gives permanent modest bonus/cosmetic, not compulsory strength.
11. Claimable milestones (daily, weekly, account, seasonal, purchase-linked) separate from random draws.
12. Animation is a visualization of already committed reward result; reveal never determines outcome client-side.

### 4.2 Initial chest definitions (simulator defaults; NOT real-money offers until approved, tested, odds displayed)

**Bronze Supply Chest**: Cash 50% (two quantity bands), WOOD 20%, STEEL 15%, PLANS 10%, common business fragments 5%; all outcomes values explicit. One guaranteed result. **Silver Operations Chest:** materials 45%, Cash 20%, Business fragments 20%, Manager XP 10%, Speedups 5%. **Gold Business Chest:** business fragments 50%, universal rare business fragments 15%, rare materials 25%, premium choice ticket 10%. Item contents and quantities data-driven; weight sum exactly 10,000 basis points. Rare reward highlight only on genuine high rarity. Tutorial grants earned bronze chest after first meaningful victory; premium equipment and advanced chest systems unlock later.

For a purchased randomized container, define exactly one disclosed item pool and quantity range per outcome; calculate effective odds for any multi-stage/nested draw. Pity protection (if any) must be real, stored, auditable and included in disclosures. Show item lists, duplicate rules, guarantee and any relevant regional availability before the purchase action.

### 4.3 Manager recruitment / gacha (concrete default design)

Unlock only after completing initial core chain concepts and earning an introductory ticket (trigger-based, no fabricated calendar day). Recruitment `RecruitBanner{id,activeFrom,activeUntil,cohort,lootTable,pityGroup,rateUpIds,featuredGuaranteeRules,earnedFreeDraws,eligibility}`.

- Example banner odds per draw: 2.00% complete Legendary manager, 8.00% Epic manager fragments, 20.00% Rare manager fragments, 25.00% Universal manager fragments, 25.00% Manager XP, 20.00% Skill Books. **Total 100.00%; all exact quantities/featured sub-odds published**. This is a tuning starting point, not copied from another game.
- `RECRUIT_TICKET` or 150 Gems per draw; 10 draws for 1,350 Gems (illustrative). Earn at least one early ticket plus recurring free ticket sources; no perpetual obligation to buy to use recruitment.
- Every 10 draws: deterministic milestone chest; 30 draws: featured fragments; 60 draws: choice chest; guaranteed Legendary by 80 eligible draws. Pity counter carries within disclosed banner group; counter persists after logout/event transition according to explicit rule; no stealth reset. Guarantee upgrades *actual outcome distribution* and must be reflected on confirmation.
- Show visible 1/10/30/60/80 progress. Accept single/batch claims, duplicate recycling, draw log and replayable animation. Do not use paid gacha for an essential campaign solution.
- Draws can add progression to recruitment-related events, but avoid stacking so many mandatory queues that players cannot understand value.

### 4.4 Free unlock with long development path

Teach basic business before first high-value earned unlock. Award an appealing Rare/Epic franchise or manager at a fixed **achievement trigger** after player has learned placement, rotation, building combination and a short contract. Give 10 unlock fragments/whole entitlement when milestone completed; show star progression and legitimate free sources, not a surprise paywall. Follow-on stars use fragment requirements in §3.2 and optional targeted chest/merchant/pack sources. Maintain additional advanced layers: XP, skill, equipment, collection. This is a staged unlock and upgrade engine, **not** a Day-1 giveaway.

### 4.5 Reward reveal UX

- Chests: squash -> lid unlock -> light expands -> weighted-color reveal -> item cards land -> inventory counters fly to destinations. 0.8–1.8 seconds default; `Skip` immediately reveals full true results; reduced-motion renders a calm static receipt.
- Single pull: anticipation limited and tasteful, rarity ring + sound chord, result history; no psychological deception like preselected near-miss symbols that imply false odds.
- Batch pull: tiles cascade in ~0.1–0.18s per item, expandable full details, one-tap Claim All, no dead waiting.
- Milestone: short, distinct claim animation; no synthetic delays solely to encourage further draws.
- Duplicate: original item visibly converts at published rate, not quietly discarded.
- Choice reward: side-by-side value and ownership context, clear confirm/back; no accidental spending.

---

## 5. SHOPS, EXCHANGES, RANDOM STOCK AND CONVERSION PATHS

### 5.1 One engine, multiple shop identities

`ShopDefinition{id,availability,inventoryPool,slotCount,refreshRule,freeRefreshCount,refreshCostCurve,stockLimit,currencyTypes,eligibilityTags,expiryPolicy,purchaseLimits,oddsVersion}`. Generate inventory **server-side** from auditable CSPRNG or signed seed; stock remains fixed until scheduled refresh or confirmed reroll. Paid rerolls always change shop offer sampling, but never guarantee better inventory unless explicitly stated. Show pool/rarity probabilities before paid reroll.

Create shops: Daily Merchant (Cash/Gems), Franchise Exchange (business tokens), Manager Guild (recruit currency), Equipment Workshop (salvage), VIP Merchant (prestige credits), Event Market (season-specific tokens), Alliance Exchange (group tokens), Collection Exchange (duplicates), Contract Board (variable offered missions). Only activate shops when their associated activities exist; they should not appear as ten useless unlocked buttons at once.

### 5.2 Daily Merchant rules (default)

- 6 slots; seeded stock; daily UTC reset announced locally; no fake reset countdown.
- First *manual* refresh per reset free. Optional additional rerolls: 1 `MERCHANT_VOUCHER`, then 2, then 3, then cap 5 refreshes/day; if using Gems, explicit alternative cost 10/20/30/50 with cap. The player must choose source before confirmation, never silent fallback to premium Gems.
- Rarity pool displayed, no guaranteed upgrade except if defined (e.g. sixth refresh rare guarantee in a specific seasonal merchant). Favor deterministic first useful offers after unlock and varied subsequent stock.
- Purchase limits per item; notify at refresh not every action; stock purchase persisted independently. Lost internet means cached view read-only, not pretend transaction success.
- Offering purchased manager fragments in the merchant requires those managers actually usable; do not include future-generation junk.

### 5.3 Exchange and sunk-cost balancing

The same item can be obtainable through different currency stores. No unrestricted arbitrage cycles (`A -> B -> A` with positive net balance). Build automated directed-cycle detection / bounded simulation. Define exchange fees/weekly caps where necessary. Paid Gems -> virtual items allowed; **no virtual currency -> cash-out**, crypto, external trade market or real-money redemption.

### 5.4 Contextual `Find Sources` sheet

Any material icon is pressable and opens: amount owned, upgrade destination, amount missing, exact free routes, earned-currency stores, active reward pools with odds, next legitimate availability, and optional eligible direct purchase. Source ranking: relevant free playable action -> saved inventory -> earned shop -> earnable chest -> optional paid acquisition. If player has enough resources, primary CTA is `Upgrade/Use` rather than `Buy`.

---

## 6. VIP, PERMANENT UTILITY, SUBSCRIPTIONS/CARDS, PASSES

### 6.1 VIP / Influence 1–12

VIP permanent levels gained from gameplay milestones and qualifying purchases, with extra high-tier shop eligibility. `VipTier{thresholdXp,benefits,freeDailyChest,eligibleOfferTags}`; XP never expires. Benefits compound **within explicitly capped categories**; no arbitrary hard-coded percentage stacking. Initial design (test numbers, subject to economy sim):

| Tier | Proposed new/stronger perk | Mechanically useful to |
|---|---|---|
| 1 | Claimable daily VIP gift | Collection loop |
| 2 | +2% construction speed | Build timing |
| 3 | +1 blueprint convenience preset | Layout management |
| 4 | +3% dispatch efficiency | Contracts |
| 5 | +3% production efficiency | Idle economy |
| 6 | +1 earned free dispatch slot via VIP OR same capacity from progression | Contracts; avoid exclusive lockout |
| 7 | Prestige merchant access | Dedicated currency spend |
| 8 | +1 manager preset | Assignment flexibility |
| 9 | Further +3% construction speed | Upgrades |
| 10 | Modest equipment-upgrade material discount | Equipment |
| 11 | Better daily gift, advanced trade choices | Late game |
| 12 | Prestige cosmetics and advanced shop inventory | Status and late progression |

VIP activation voucher, if implemented, unlocks *extra* temporary benefits of achieved tier, not required access to already-earned core features. Free activation earned; Gems optional. Direct paid VIP XP in eligible bundles can be tested but exact total cost and free pace must be visible.

### 6.2 Permanent utility

Prioritize `Second Builder`, extra `Dispatch Slot`, additional blueprint templates, batch collection/auto-claim, optional advanced manager preset, cosmetic city display slots. Each product meaningful even without other purchases. No paid undo, paid basic preview or paid correction of touch mistakes.

### 6.3 Weekly/monthly card

- Weekly Operations Card: daily small resource bundle for 7 calendar claims; optional missed-day buffer/claim-all policy disclosed. Clearly **not** an auto-renewing subscription unless actually sold through Play subscription plumbing.
- Monthly Enterprise Card: Gems + daily material/utility benefit + automatic eligible routine claims or saved layouts, 30 days; no endless 'unlimited' claims. Calendar, daily benefit, expiration and platform billing model explicit.
- Paid premium Gems never expire. Time-limited card *benefits* may end per disclosed terms, but unspent purchased universal Gems remain.
- Nonrecurring card versus renewing subscription must be differentiated in product/checkout code. Support cancellation/account status, refunds, grace, and store-based subscriptions only if genuinely implemented and tested.

### 6.4 Season pass

28-day initial season. 30 levels, rewards on **free and paid tracks**. XP earned from existing puzzles, contracts, business upgrades, manager training, collections, merchants and events; appropriate daily+weekly caps to avoid infinite farming. Premium purchase unlocks already completed milestones retroactively. Premium purchase never blocks free track. Support late entry catch-up, claim-all, expired-season handling and refund/ownership reconciliation. Offer tiers: basic pass and optional higher pass with clearly stated immediate tier skip, never compulsory to finish.

---

## 7. LIVEOPS, EVENT STACKING, CONTENT GENERATIONS

### 7.1 Multiple independent clocks

- **Account progression** (objective unlocks): chain tutorials, first contract, first manager, empire upgrades.
- **Account tenure** (truthful UTC elapsed): starter offer, comeback rewards.
- **Daily/weekly reset**: mission, shop refresh, VIP gifts, contract inventory; show accurate server time and local display.
- **Event cadence**: daily mini-objective, weekly theme, 28-day season, 4–8 week generation cadence *only after measured readiness*.
- **Content cohort age**: group of players receiving a shared progression season; do not assume an account is on 'Day 30' because a server is 30 days old.
- **Transaction eligibility**: first purchase, owned permanent unlock, pack limits, cumulative top-up milestone.
- **VIP/feature unlock**: thresholds and eligibility independent of install time.

### 7.2 Event catalog: ship configurations and corresponding screens

1. **Construction Rush:** qualifying build starts/completions and used speedups; cap farming; economy materials, queue benefits.
2. **Franchise Festival:** acquire and star-promote actual owned businesses.
3. **Executive Recruitment:** draw, promote, improve manager skills; credit *eligible* account actions.
4. **Research Week:** completed research nodes; research documents and speedups.
5. **Delivery Derby:** valid dispatch arrivals and claims; route choice matters.
6. **Chain Masters:** score, actual resource throughput, specific earned combos; certified skill division no paid power.
7. **Merchant Festival:** verified exchanges and optional refresh mechanics; no requirement to purchase currency.
8. **Collection Expedition:** album fragments, complete themes, duplicate conversion.
9. **Equipment Workshop:** enhancement and set completion.
10. **Investment League:** account-development milestones and ranking with openly progression-influenced scoring.
11. **Shared City Project:** asynchronous real-player group projects, contribution caps and rewards.
12. **Seasonal Festival:** event tasks, chest keys, event currencies, featured generation rewards.
13. **New Manager Generation:** new earnable characters, featured banners, choice exchanges.
14. **Returner Catch-up:** reversible engagement aids after absence, no shame/punishment copy.

Each event definition contains `eventId,contentVersion,cohortIds,openAt,closeAt,graceUntil,qualifiedEventTypes,pointFormula,milestoneRewards,freeObjectives,leaderboardDivision,shopId,passId,packTags,participationLimits,currencyExpiryPolicy`.

### 7.3 Event stacking rules

A single **verified** event (e.g. `ManagerPromoted`) can trigger permanent stats, daily objective, weekly event, season pass, collection, achievement and cooperative project, **each once**, using separate subscriber grants. Every subscriber must inspect eligibility and prevent replay. Never award points twice because an offer is purchased and the inventory is replayed; purchase milestone scoring and material-consumption scoring are distinct and transparently documented. Event countdowns genuine. Show relevant overlapping events without clogging reward UI. Include saving materials as strategy; no unexpected event retroactivity unless defined.

### 7.4 Events as micro-economies

Support event keys, token shops, reward ladders, optional paid pass, event chest, guaranteed activity milestones, limited inventory, shared/individual rankings. Each sub-economy references the canonical inventory. Define currencies either persistent across reprises or explicitly earn-only expiring; no paid gems expiry. Reward loops cannot feed unlimited positive arbitrage.

### 7.5 Generation releases

Server/config-cohort controlled introduction of new manager pool, 1–2 new businesses, research branch, gear tier and dedicated advanced crafting material only when sufficient player population reaches the prerequisites. Content is real and testable before event announcement. Existing veteran achievements remain relevant; catch-up path for new accounts. Feature flags for release, rollback and economy emergency pause.

---
## 8. THE OFFER ENGINE — RELEVANCE, ROTATION, PURCHASE PATHS

### 8.1 Data model and ownership

`OfferDefinition{id,revision,platformSku?,offerFamily,bundleItemIds,quantities,choiceGroups,eligibilityRules,displaySurfaces,priority,validFrom,validTo,cohorts,perPlayerLimit,globalLimit?,cooldown,stepIndex,prerequisiteOfferId?,pricePresentation,promoEvidence,experimentId}`. Platform SKUs configured in store; never invent localized currency strings. An offer may spend earned currency or paid Gems rather than require direct IAP.

`OfferEligibilityService` reads account unlocks, missing *useful* resources, relevant events, owned entitlements, stock, last presentation timestamps, locale, age/region restrictions and real store availability. `OfferRanker` scores contextual fit, not hidden vulnerability. A rules engine selects up to N contextual offers; no personal price discrimination; no changing paid loot odds by spend history. Keep all offer decisions audit-logged with reason and version.

### 8.2 Catalog family inventory — mandatory engine support

| Family / ID prefix | Offer examples | Primary triggering context | Duration and alternatives |
|---|---|---|---|
| `starter.*` | $0.99 first-purchase combo of useful resources + original intro manager fragments | First meaningful empire progression; non-payer only | Permanent eligibility until purchased or declared event end; don't fake countdown |
| `permanent.builder` | Extra permanent construction queue | Player has shown second viable project | Always available after feature unlock; earned temporary speedups alternative |
| `permanent.dispatch` | Extra dispatch/ops slot | Regular concurrent operations | Permanent plus achievable free capacity over time |
| `permanent.blueprint` | Extra templates / profiles | Existing free slots used | Existing purchased SKU restored |
| `gems.*` | 4–6 top-up tiers | Gem balance view, eligible sources | Real localized tier SKUs |
| `daily.choice` | Small choice chest / fragment selection | Targeted owned progression, reset | One/day stock, useful options |
| `daily.mixed` | Hero, materials and equipment small pack | Progression + shop rotation | Daily; don't repeatedly interrupt |
| `weekly.card` | 7-day operational resources | Renewable operations | Non-renewable card initial |
| `monthly.card` | 30-day daily Gems/efficiency | Multi-week account use | Clearly shows terms |
| `growth.fund` | Retroactive rewards at empire levels | Enough milestones completed | Purchase once; free base economy remains |
| `pass.standard` | Season reward track | Season is active, player completing XP | Inherit completed milestones retroactively |
| `pass.premium` | Plus specified level advance, not hidden subscription | Already understands season | Transparent alternative tier |
| `vip.milestone` | Tier-special selection rewards | VIP threshold | Purchase limit per tier; permanent VIP unaffected |
| `biz.targeted` | Relevant business fragments / choice pack | Owned/usable specific franchise under development | Free contract and merchant routes |
| `manager.targeted` | Manager shards/XP/books | Eligible character exists or recruitment unlocked | Free recruitment / XP routes |
| `gear.*` | Materials and choice gear | Gear available | Free salvage/events |
| `build.speed` | Speedups + construction supplies | Eligible active construction | Free waiting remains usable |
| `research.speed` | Research materials + speedups | Queued research | Free progression |
| `dispatch.speed` | Shorten / increase dispatch opportunity | Active contract | Free wait and earned boost |
| `event.*` | Tokens, event materials, optional attempts | Relevant event active | Explicit duration and limits |
| `event.staircase` | Sequential $0.99/$4.99/$9.99/$19.99 tiers with free claim items between | Special declared event | Each next tier pre-disclosed, no misleading hidden price |
| `event.recharge` | Guaranteed cumulative rewards for qualifying purchases | Promotional event | Transaction milestones logged, real calendar |
| `chest.choice` | Known choice chest, not random | Multiple material bottlenecks | Guarantees exactly selected items |
| `chest.random` | Purchasable loot container | Specific reward category, allowed region | Real odds displayed, pity if stated |
| `recruit.ticket` | 1/10 recruitment draw currency | Recruitment unlocked | Earned tickets and free draw options |
| `merchant.refresh` | Paid reroll voucher/Gem option | Current stock rejected, refreshes available | Disclosed odds / exact cost and cap |
| `collection.complete` | Eligible fragment selection | Album nearly complete | Free trade/exchange sources |
| `cosmetic.*` | Building art, particle trails, city themes, frames | Cosmetic editor/profile | Cosmetic only; no false stat claim |
| `prestige.*` | Shop access/decoration choice | High VIP/late stage | Progression-based |
| `returner.*` | Catch-up choice pack + useful earned missions | Actual prolonged account absence | No shaming or countdown without actual schedule |
| `advanced.*` | Late-tier materials, specialized research bundles | Specific unlocked technology/generation | Never sell unusable materials |
| `expansion.*` | Preserve existing Harbor and Finance puzzle packs | Challenge content progression | Existing entitlements honored |

**Pricing test tiers** (USD equivalent, **hypotheses only**, localized from Play metadata): entry $0.99; impulse $1.99/$2.99/$4.99; mid $9.99/$19.99; high $49.99/$99.99. App store product config controls actual price. Use reference value only if verifiable from exact non-discounted product within lawful promotion; do not manufacture “90% off” anchors.

### 8.3 Dynamic pack lifecycle

`Locked -> Eligible -> Available -> Displayed -> Opened -> Purchased/Declined -> Cooldown/LimitReached -> Restocked/Expired`. A sequence-specific offer adds `NextTierUnlocked` transition. Lifecycle state persisted server-side. Product's contents displayed exactly and localized; current gear/generation substitutions must use versioned catalog snapshots per eligibility, not retroactively alter an offer already purchased. Track impressions and actions accurately (not every UI repaint).

### 8.4 Rotation schedules and selection

- Baseline daily 4–6 merchant inventory slots refreshed at announced reset.
- Weekly material/gear/manager pack families in a deterministic, server-configured rotation. Event packs exist only during genuine event windows. Group server/cohort schedule and individual eligibility both apply.
- Level/unlock targeting: the same pack *template* can substitute eligible early resources with advanced resources only with explicit `offerVariantId` and exact contents.
- New player pack after relevant feature: no Day 1 Legendary guarantee. Intro offer arrives after skill tutorial + first permanent objective.
- Returning account: show concise `Welcome back` report with true elapsed capped production, catch-up missions and relevant products (at most one polite offer suggestion).
- VIP threshold: unlock its exact special offer and shop inventory. Top-up milestone should be progression toward a published promotion; a purchase may earn normal contents + VIP XP + alliance gift + recharge progress **only where all four are genuine eligibility rules**.

### 8.5 Contextual placement policy

| Surface | Context | Primary route | Optional premium route | Interrupt rule |
|---|---|---|---|---|
| Construction project | Missing STEEL or timer remaining | Contracts/production/wait | Materials/speedup offer | Embedded acquisition sheet; no modal after every build |
| Franchise details | 25/40 fragments | Source finder / available chests | Targeted fragments / choice pack | Never auto popup |
| Manager roster | Star cap / missing books | Missions/recruitment | Training pack | Contextual button |
| Chest inventory | Earned chest count | Open chest | Obtain more eligible chests | No purchase prompt during reveal |
| Gacha | Earned ticket shortage | Ways to earn ticket | Gem/ticket top-up | Odds displayed; user initiates |
| Merchant | Unwanted stock | Wait/free daily reroll | Voucher/Gem refresh | Confirm price each paid reroll |
| Contract list | All slots occupied | Wait, claim, free unlock | Optional extra slot/speedup | Relevant, no nagging |
| Event | Near meaningful milestone | Play qualifying tasks | Event pack or pass | Event details shelf only |
| Season pass | Free XP progress | Free track | Premium unlock retroactive rewards | On user-open; optional one-time preview after proven interest |
| VIP | New tier reached | Claim earned gift | Tier pack | Tier detail screen |
| Result | Earned resources and next action | Claim, continue, choose | Optional rewarded ad for bounded bonus | One opt-in surface, not blocking Next |
| General Store | User deliberately browses | Full catalog | Any eligible SKU | No interruption |

**Offer presentation frequency budget:** No mandatory purchase prompts during first 3 successful puzzles. At most 1 unsolicited offer introduction per session after relevant unlock and no more than 1 per 24h in early lifecycle, remotely tunable. Contextual static buttons do not count. Never cover active board manipulation, chain animation or earned reward claim with real-money offers. User can decline in one clear action; declines are respected for reasonable cooldowns.

### 8.6 Recharge/purchase milestone design

Offer guaranteed additional milestone gifts for aggregate eligible purchases during a **truthful** event: e.g., purchases 1/3/5 qualifying transactions or clearly disclosed currency spend amount (jurisdiction/age restrictions may disable). Cap rewards; no misleading 'free gift' where a purchase required. Purchase-related reward track cannot create an infinite grant loop; idempotency keyed to transaction IDs. Transactions also can grant alliance gifts via server where co-op feature is enabled; never fake other members' contributions.

---

## 9. RETENTION OPERATING SYSTEM — RETURNING FOR ACTUAL VALUE

### 9.1 Nested loops

| Loop | Cadence | Player desire | Economy and purchase relationship |
|---|---|---|---|
| Action | Each touch | Precise control and immediate feedback | Core value of game; never charge for touch correction |
| Round | 30s–5m | Watch skillful chain reaction, beat objective | Cash, material, XP, missions, earned chest |
| Short return | Minutes/hours | Collect completed build or dispatch | Optional acceleration/capacity |
| Session | 5–20m | Finish several upgrade targets, collect milestones | Contextual resource acquisition |
| Daily | Real daily reset | Fresh merchant, missions, claims, stage | Earned tokens, optional pass progression |
| Weekly | 7d | Focused event / collection / league | Event pass/eligible packs |
| Seasonal | 28d | Complete reward track/album, new goals | Premium track/monthly benefit |
| Long horizon | 2–6 months | Perfect build, VIP, managers, franchise generations | New resource sinks, specializations |

### 9.2 Daily return without compulsion

Morning-style report on next actual app open: finished queue, capped offline output, a ready contract, daily objectives, changed shop stock and valid event. Daily reward free claim, no broken streak or loss of purchased property for missing days. Weekly chest based on cumulative activity rather than punitive perfect attendance. Push notifications **opt in** per category (construction finished, event started, contract ready, season end), with quiet hours, limits and region-safe permission UX; no fake urgency or notification spam. Email not required for play.

### 9.3 Daily/weekly missions

3 daily missions after unlock, involving different ordinary gameplay systems, plus 1 optional stretch mission. 5–7 weekly goals and seasonal pass XP. No duplicate credit for rapidly repeating trivial no-cost actions; use source/event quotas. First daily reward easy, second varied, third meaningful, weekly jackpot guaranteed from real progress. Progress tracked server-side and replay safe.

### 9.4 Collections, completion and identity

Business catalog, manager album, city cosmetics, gear sets, chain combo encyclopedia, franchise sets and themed fragment album. Reward partial milestones *and* set completion; show missing pieces and credible sources. Preserve existing eight sets, add deeper fragment-based albums separately. Public prestige identity: cosmetics, profile frames, city skins, event-earned badges; no virtual-value redemption.

### 9.5 Social/competitive progression

MVP: asynchronous opt-in score boards on a deterministic, server-verified challenge, and progression-weighted Empire League distinctly labeled. Required later-phase deliverable: real alliances (join/leave, shared progress, member roles, gifts from eligible purchases, weekly cooperative projects) with reporting, privacy and protection against duplicate or fabricated members. Public chat is an optional disabled-by-default module; if enabled, build moderation, blocking and reporting before launch. Players must be able to thrive in solo mode. Avoid pushy alliance obligations, fake war emergencies or fabricated ranks. Competitive paid-power modes explicitly separated from verified equal-rules puzzle leaderboards.

### 9.6 Personalization and onboarding

Teach based on **completed capability triggers**, not invented Day 1/7/30 assumptions. Player who races ahead sees advanced tutorial earlier; a slow player never gets an irrelevant banner for a feature not unlocked. Account tenure, feature unlock, event cohort and transaction eligibility are independent clocks. Returning players see real catch-up features with honest descriptions.

---

## 10. FIRST EXPERIENCE AND PROGRESSION UNLOCK GRAPH

### 10.1 First 90 seconds

App loads immediately into existing tutorial board; no registration, paywall or optional shop distraction. First meaningful drag target in 5–10 seconds after control; first successful chain within 45s for reasonable first-time testers (measure). Show input/output paths and what changed. First result reveals **earned** Cash, small material bundle and a basic chest, not a top-tier manager. A 2–3-step coach guides second rotation and third combined chain. Undo/forecast remain free. No ad or purchase interstitial here.

### 10.2 Trigger-based unlock sequence (hard dependency, not elapsed-day promise)

| Gate ID | Verified trigger | Newly visible feature | Player-facing reward/case | Commerce visibility |
|---|---|---|---|---|
| G00 | First app open | Fixed Puzzle Board | Interactive connection | None |
| G01 | First successful chain | Earned result chest/inventory | 1 Bronze Supply Chest | No real-money CTA |
| G02 | Complete rotation lesson | Materials and empire hub preview | Starter materials | No real-money CTA |
| G03 | Finish three tutorial concepts | Permanent starter franchise + first build | Initial plot and fast builder project | Basic Earn Sources / Store accessible |
| G04 | Complete first persistent business construction | Dispatch contract board and first repeatable contract | Small free contract | Eligible builder utility appears *after* need demonstrated |
| G05 | First contract completed/claimed | Manager introduction and introductory recruit ticket | Guaranteed useful starter manager via earnable onboarding path | Optional first-purchase starter offer becomes eligible |
| G06 | Manager assigned to a business | Franchise fragments and star screen | Progress sample from real mission | Relevant fragments acquisition routes |
| G07 | First eligible 2-star business or 3 contracts | Merchant stock + one free refresh | One useful shop discount | Optional further rerolls |
| G08 | Qualifying 6+ puzzle outcomes / varied activities | Daily Missions, Collection completion | Free milestone chest | Daily deals, weekly cards optionally visible |
| G09 | Empire Level 5 and manager developed | Recruitment banner + pity progress | Earned recruitment ticket | Gem top-ups/tickets if initiated |
| G10 | First participating weekly event | Event board, event reward track | Free event milestone | Optional event pack and pass |
| G11 | Demonstrated simultaneous tasks | Research, extra dispatch/builder upgrade routes | First research completed | Targeted acceleration |
| G12 | After sufficient actual play and completion | VIP progress, growth fund, season pass | Earned VIP XP and free track | Relevant eligible card/fund/pass |
| G13 | Late progression thresholds | Gear, advanced materials, prestige shop | New useful economy systems | Advanced packs only when usable |

All gate thresholds are editable in versioned config, and tests guarantee no circular dependency or impossible free route. Player should experience interesting collection/rewards before being asked to buy gacha. Skill tutorials never teach 'pay to succeed'.

### 10.3 Long progression

Empire levels 1–50 initial content (finite authored upgrade tables, no infinite overflow). Content-generation unlocks after level/research/cohort eligibility. First 72 certified missions remain free. Existing 32 paid expansion missions remain owned under original entitlement rules; do not charge again. New repeatable expedition seeds must have solveable cases. First 12 authored empire operations tuned manually; generated repeatable templates verified by property-based solver before enabled.

---

## 11. FULL SCREEN AND NAVIGATION SPECIFICATION

**Navigation:** Bottom tabs `Board`, `Empire`, `Managers`, `Events`, `Shop`. Global top bar only `Cash`, `Gems`, current timed event if active, Inbox/Notifications and user settings. Secondary routed features accessible within appropriate tabs or context. No six-currency clutter. Every screen: locked/empty/loading/ready/error/offline/owned/refunded/region-disabled states; back navigation; safe area; Android Back; accessibility; honest progress; consistent close buttons.

| Screen ID | Surface | Key actions and components | Acquisition/monetization relationships |
|---|---|---|---|
| S00 | Boot / resume | Version check, offline mode, background sync | No buy prompt |
| S01 | Tutorial board | Drag house, rotate, connect, forecast, activate | Unlock real earned result |
| S02 | Main board | Existing 2D city, available pieces, actual ports, score preview, Undo, Activate | Booster inventory for permitted modes only; no puzzle paywall |
| S03 | Inspection drawer | Inputs, outputs, level effects, related business | Source finder / legitimate upgrade |
| S04 | Chain animation | True 8-beat flow tokens, coins, escalating but skippable reward animations | None during animation |
| S05 | Result | Score trace, earned items, contract/event/pass progress, earned chest, Next | Optional reward ad if available and disclosed |
| S06 | Empire hub | Persistent district plots, construction, ready collections, upgrades | Construction slots, materials, VIP |
| S07 | Business page | Level, rarity, star bar, manager, gear, related contracts | Fragments, speedups, choice pack |
| S08 | Construction queue | Active builds, real timers, scheduling, free/premium speedups | Extra builder unlock, materials |
| S09 | Warehouse | Output rates, caps, Collect All, material inventory | Storage and optional collection conveniences |
| S10 | Research tree | Prerequisites, costs, durations, effect details | Research sources and acceleration |
| S11 | Contract Board | Real rotated offers, refresh, duration, dispatch | Vouchers, Gems, capacity |
| S12 | Contract progress | Timers, assignment, claim, logs | Speedup and dispatch capacity |
| S13 | Manager roster | Filter, assign, compare, upgrade, manager collection | XP, books, fragments |
| S14 | Manager profile | Skills, gear, stars, sources, presets | Upgrade resources |
| S15 | Recruitment | Banner, rates, pity, ticket/Gem draw, guaranteed milestones | Ticket/Gem purchases via platform store |
| S16 | Chest inventory | Category, chest merge, open x1/x10, choice chest | Earn more chests/eligible bundles |
| S17 | Chest reveal | Accurate reward cards, skip/fast, duplicates, details | No forced upsell mid-reveal |
| S18 | Daily Merchant | 6-stock, free refresh, paid reroll, limits, reset time | Earned Cash/Gems, voucher, paid Gems |
| S19 | Specialty shops | Franchise/manager/gear/VIP/event/collection/alliance tabs | Earned token + eligible materials |
| S20 | Inventory/source finder | Owned, required, sources ranked, exact odds where relevant | Crosslinks to gameplay and optional eligible offer |
| S21 | Collections/album | Existing 8 sets + fragment albums, trade/exchange, complete rewards | Pack acquisition routes |
| S22 | Combo encyclopedia | Existing 16 real-flow combos and progress | Focus on fun mastery; optional customization |
| S23 | Daily objectives | 3 + stretch, claim, reset time | Event tokens, free Gems, chests |
| S24 | Events hub | Active calendar, upcoming genuine events, eligible reward tracks | Passes, event items |
| S25 | Event detail | Score formula, milestones, rankings, tasks, inventory | Matching resources, pass |
| S26 | Season pass | Free/paid tracks, level claims, remaining time, ownership | Premium pass, retroactive claims |
| S27 | VIP / Influence | Tier progress, earned bonuses, daily claim, prestige shop | VIP-eligible bundles |
| S28 | Achievements | Existing 30 achievements plus new economic achievements | Deterministic lifetime rewards |
| S29 | Asynchronous League | Fair skill board vs progression-weighted Empire League, verified scores | Optional progression (never alter fair mode) |
| S30 | Cooperative Project | True group progress, contribution limits, gifts | No fake teammates; alliance purchase gifts only when deployed |
| S31 | Direct Store | Price metadata, product gallery, permanent vs consumable, restore | Real IAP purchase sheet |
| S32 | Offer Detail | Exact item contents, currency/price, term, odds if random, eligibility | Confirm/cancel |
| S33 | Purchase history/restore | Pending, confirmed, restored, refunded, support | No fake success |
| S34 | Inbox / return report | Ready builds/contracts, earned claims, patch notes | One contextual offer at most |
| S35 | Districts/challenges | Current 72 core + 32 owned expansion, replay stars | Existing expansion ownership |
| S36 | Blueprints | Save/load layouts/presets; original purchased extra slots | Permanent convenience |
| S37 | Cosmetic editor | Owned skins, preview, filters, city palette | Cosmetic store |
| S38 | Settings/Help | Motion, contrast, controls, audio, privacy, odds explanation, account link, notifications | No hidden promotional traps |
| S39 | Account / cloud recovery | Guest identity, optional link, restore, export/delete as law requires | Durable recovery, no forced login before core play |
| S40 | LiveOps Inbox/Error | Graceful offline, disabled offer, stale version, network problems | Clear retry, no purchase charged twice |

**Screen-density rule:** Board remains dominant. Don't cram all systems onto one page. New menu unlocks include short coached demonstration and one free benefit. Notifications never cover essential board input. Modal opens and back navigation must be reliable on Android gestures.

---
## 12. TACTILE DESIGN: EVERY TAP, DRAG, SWIPE, REVEAL AND UPGRADE

A commercial economy will fail if the underlying interaction feels cheap. Existing image identity: lush green miniature blocks, teal road network, bright mint economic pulses, cobalt/orange typography, vivid gold activation. Maintain recognizable coherent style; no ripped reference assets. Design 2D sprites, shadows and highlights consistent with actual board coordinate system (no nonfunctional rendered fantasy perspective). All text stays true native TextMeshPro/uGUI, not baked into art. Support 720×1600, 1080×1920, 1080×2340, 1170×2532, tablet and desktop companion.

### 12.1 Touch response language (initial targets, tune by device playback)

| Interaction | Motion & feedback requirement | Sound/haptic | Actual logic |
|---|---|---|---|
| Tap button | Down-state scaling ~0.96 in <80ms, elastic return ~160ms | Soft tick optional | Action starts on valid release; no false state |
| Long press | Progress ring, contextual inspector without obstructing finger | Gentle subtle haptic | No purchase action initiated by long press |
| Pick up building | Lift/shadow, ghost placeholder, legal-slot preview, finger offset | Pickup chirp | Board unchanged until valid drop |
| Drag | 60fps where feasible; smooth movement; glowing live connections | Gentle subtle cue when valid snapping changes | Forecast can be incremental but final must match solver |
| Valid drop | Snap to center; tiny rebound; connection lines light up | Two-tone snap + haptic | One atomic board command; saved |
| Invalid drop | Ghost recoils and returns; exact reason label | Light deny tone | No currency or lost piece |
| Rotate | Pivot 90° around center; ports update and routes animate | Click haptic | Exactly one board rotation per action |
| Undo | Reverse motion/connection glow | Reversing click | Free; undo only board placements not irreversible paid spending |
| Forecast | Animated potential flow, colored throughput, bottleneck warnings | Soft scan cue | Derived from real `Resolver` output |
| Activate Chain | Golden button depresses; power surge follows actual routes | Short impact then rising tonal rhythm | Authoritative result computed and persisted first |
| Each beat | Resource token pulse travels actual port path; readable cash counters | Pitch increases with real legitimate combo | Token count and speed true to event log |
| Real combo discovered | Distinct mini splash, symbol chain and collecting ribbon | Unique success chord | `ComboBook` true condition only |
| Result reward | Cards and materials settle into inventory counters | Collecting flourish | Transaction already granted once |
| Claim chest | Chest opens with tier-specific light, no false preview | Chest tier cue | RNG result committed before reveal |
| Manager recruit | Reveal rarity ring and character entry pose | Rarity signature | Server-granted true reward |
| Upgrade building | Construction mini-animation, particles, tangible visible altered façade | Construction chord | Queue/result state authoritative |
| Star promotion | Fragment burst fills star, ability highlighted | Tier promotion flourish | Cost and ability state exact |
| Merchant refresh | Shelves flip/slide, new products reveal | Shuffle | Inventory change signed/paid once |
| Currency gain | Small pickup trails toward correct counter | Short coin pitch | Numbers reconcile with server ledger |
| VIP level | Crest fills and announces specific perk | Prestige sound | Tier rule applies exactly |
| Daily reward | Calendar/day stamp and item flyout | Cheerful collect | One real claim per reset |
| Error/offline | Clear inline explanation, retry affordance | No punishing noise | No false balances or frozen progress |

All animation values hypotheses, not rigid production performance commitments. Render functionally meaningful flows and hierarchy first, then decorative particles within measured frame-budget. `Reduce Motion` removes unnecessary shake/flash/long animation while retaining information. Support mute, haptics off, colorblind shapes/patterns and large accessible touch targets.

### 12.2 Cashflow chain spectacular moment (core visual product)

Use deterministic trace to animate each 8-beat cycle as a cohesive rising chain reaction: colored material tokens (people/goods/energy) flow from producers; converters pulse when truly fed; cash signs travel to top HUD; actual profit nodes get gold glows; legitimate completed combos light a short icon ribbon; at payout, a satisfying receipt enumerates both round objective and actual persistent reward earned. Keep a fast-skip button. Avoid excessive VFX obscuring readability or adding input lag. An excellent chain should look and sound noticeably better than a weak one while reflecting actual economics, not fabricated jackpot values.

### 12.3 Visual polish specification

- Build sprite/UI component atlas: nine-slice cards, highlighted buttons, default/rare/epic/legendary rim materials, progress bars, stock shelves, chest glyphs, currency tokens, character portraits, six or more building activity animations, VIP seals, collection album frames, actual outline indicator of connected ports.
- Micro-cinematics: chain activation, meaningful discovery, first manager acquisition, first franchise star, rare chest, new research tier, first island expansion. Aim 0.4–1.5 seconds for ordinary actions, skippable for repeated cases.
- Persistent map visual evolution: buildings get lights/rooftop details/vehicles/street life as real upgrades complete; a tangible city transformation reinforces investment.
- Dynamic ambience: bounded pedestrians, cars following correct lane direction, warm windows, neon signage, cargo movement, warehouse forklift icon movement; not distracting under HUD, disabled under reduced motion and performance profiles.
- Portraits/managers: visually distinct original silhouettes and roles, readable at 64–128px. Optional small idle animations; avoid expensive skeletal animation requirement until 2D pipeline stable.
- Audio: original varied 4–6 layers per sound category to avoid repetition; sound bus independent for UI, coin, effects and music; use platform haptics sparingly.
- Reward rarity colors + icons + text; visual semantics accessible beyond color.
- Pressable shapes have hitboxes ≥44dp-equivalent, top HUD always legible, game board receives substantial vertical area; one-thumb options for rotate/undo.

### 12.4 Interaction delight performance gates

Never block board drag on networking or store fetching. Drag feedback p95 <50ms on midrange Android test device, ideally 16.7ms frames at 60fps; accept stable 30fps on lower end without dropped inputs. Long-run 15-minute thermal/battery test. Control latency/perceived responsiveness matters more than count of particle systems. Test success/failure/audio-off/reduced-motion/short-screen every screen.

---

## 13. INITIAL ECONOMY NUMBERS — SIMULATOR BASELINE, NOT PROMISES

**Codex must implement these as editable `balance-v1.json` values and run the economy simulator before shipping.** Values are initial design hypotheses, chosen for coherent relative scale, not live game market facts.

### 13.1 First-session target rewards

- Certified first puzzle: `ROUND_SCORE` based on actual resolver; first-clear `CASH +80`, `WOOD +10`, 1 earned Bronze Supply Chest; never repeatable for unlimited Cash.
- Second/third tutorial first-clear: `CASH +100` each, materials +10 and +15; unlock first permanent starter franchise + construction project (small materials provided by tutorial so no deadlock).
- First contract: 60 seconds, requires actual verified unlocked chain; yields ~60 Cash + 5–10 material units; first claim grants starter manager ticket or predefined free starter manager.
- Default open-city operations: reward based on verified puzzle/combo skill with a **daily reward budget**; after cap, allow free practice and skill scores but no unlimited exploitable durable payout. Higher tiers expand budget through progress, not only payment.
- Starting empire: one builder, one contract slot, storage good for a few play sessions, a small manageable number of plots; faster early builds so initial learning doesn't stall.

### 13.2 Baseline construction progression

| Facility tier | Example Cash | Materials (`WOOD/STEEL/PLANS`) | Base build time | Gameplay effect |
|---|---:|---|---:|---|
| 1 | 100 | 10 / 0 / 0 | 30 sec | Initial production unlocked |
| 2 | 240 | 20 / 5 / 0 | 3 min | +8% output or slot |
| 3 | 550 | 35 / 15 / 3 | 12 min | New visual detail, +10% output |
| 4 | 1,200 | 60 / 30 / 8 | 45 min | Operational ability/breakpoint |
| 5 | 2,800 | 100 / 65 / 20 | 2 hours | Specialized production |

Beyond tier 5, use validated authored cost curves and caps; no simple unbounded exponential without simulation. Onboarding may grant time-limited construction vouchers honestly. First 3 projects ensure clear progression in first 10–15 minutes without forced wait.

### 13.3 Baseline distribution targets (to be empirically tuned)

- 5-minute new session: one meaningful board win, one claim/upgrade and clear next objective.
- 10–20-minute returning session: several puzzles/dispatch decisions, 1–3 visible progress milestones, at least one free acquisition route relevant to current scarce material.
- 7-day ordinary nonpayer: multiple permanent upgrades, at least one recruited/upgraded manager, several chest openings, one event pass free-track experience; no mandatory paid gates.
- 28-day engaged nonpayer: visible franchise and manager development, recurring merchant and season value, achievable rare reward, never permanently blocked.
- Payer: permanent builder and cards save actual time; fragments/gear packs offer calculable advancement; no product instantly finishes every content system.

**Do not turn these targets into fake achievements in production:** tune amounts/rates until genuine gameplay produces them.

### 13.4 Simulation cohorts

At minimum simulate 10,000 synthetic accounts/cohort over days 1–180 with: new casual 8 min/day, steady casual 15 min/day, frequent 45 min/day, optimizer 90 min/day; for each, nonpayer, entry payer, occasional payer, repeat payer, high spender; plus hiatus/returner and server-newcomer to advanced cohort. The spending cohorts must model voluntary demand rather than assume purchases happen by fiat: separate no-purchase counterfactual and utility/price-sensitivity Monte Carlo scenarios.

Produce distributions P10/P50/P90: time to build/manager stars; cash stockpile; shortage frequency; frequency of no-action states; per-day premium gems earned/spent; event accessibility; investment outcomes; content exhaustion; purchase-savings magnitude; variance from gacha; number of simultaneous eligible offers; amount lost from inventory caps; probabilities and effective acquisition cost. Detect arbitrage/infinite rewards, paywall dead ends, unaffordable upgrades and disproportionate gaps. Raise alerts and reject catalog if strong issues exist.

### 13.5 Formula and caps

Use integer/fixed-point basis-point mechanics; explicit rounding. For stacking, `effective = base * (10000 + min(sumBonusesBp, capBp)) / 10000`, unless a researched multiplicative system has a documented alternative. Build speed floor and cap; no 100% zero-duration accidentally. Active speedups subtract bounded remaining duration and write new server timestamp. All probability distributions in integer weights summing to 10,000; deterministic seeded RNG in tests, CSPRNG in authoritative production reward outcome, with replay/audit record.

---

## 14. DATA SCHEMAS, API CONTRACTS AND EVENT FLOW

### 14.1 New canonical domain separation

```text
Assets/_Game/Scripts/
  Domain/
    Cashflow/                # preserve solver, Board, Pieces, conservation
    Economy/                 # items, wallets, transactions, caps, sources/sinks
    Empire/                  # owned business, construction, production, storage
    Managers/                # recruit, stars, XP, skill, equipment
    Dispatch/                # recurring contracts/queue/eligibility
    Rewards/                 # chests, loot tables, choice, merge, duplication, pity
    Commerce/                # offer rules, pack composition, stock, refresh
    LiveOps/                 # events, passes, VIP, generations, collections
    Social/                  # leagues, alliances, group progress
  Application/
    CommandBus/              # typed commands, validation, idempotency
    EventBus/                # typed events, replay-safe subscribers
    ClientSync/              # online/offline coordination
  Infrastructure/
    LocalState/              # atomic save, schema migration and safe cache
    CloudApi/                # auth/account, secure REST/websocket if needed
    Purchasing/              # Unity IAP 5.4.4 adapter, server verification flow
    Analytics/               # consent-controlled typed telemetry
    RemoteConfig/            # revisioned content/catalog with rollback
    Clock/                   # trusted time, reset windows
  Presentation/
    Board/ Empire/ Managers/ Rewards/ Merchants/ LiveOps/
    Store/ Social/ Inventory/ Navigation/ Accessibility/
  Tests/
    EditMode/ PlayMode/ ContentValidation/ EconomySimulation/
```

Backend (separate deployable service and migrations): `account/auth`, `economy/ledger`, `inventory`, `progression`, `contracts/timers`, `loot/rng`, `commerce/verification`, `shops`, `liveops`, `cohorts`, `leaderboards`, `notifications`, `telemetry/admin`, `config`.

### 14.2 Example `ItemDefinition` JSON

```json
{
  "id": "BIZ_FRAG:cafe",
  "kind": "fragment",
  "displayKey": "item.cafe.fragment",
  "rarity": "rare",
  "stackable": true,
  "canExpire": false,
  "maxStack": 100000,
  "eligibleTargets": ["business:cafe"],
  "sources": ["contract:cafe", "loot:bronze", "shop:franchise", "event:franchise", "offer:biz.cafe"]
}
```

### 14.3 Example offer JSON (an IAP bundle; NOT priced inside game data)

```json
{
  "id": "offer.starter.v1",
  "revision": 1,
  "family": "starter",
  "platformProductId": "cc2.starter.01",
  "items": [
    {"itemId":"GEMS","quantity":90},
    {"itemId":"WOOD","quantity":30},
    {"itemId":"MANAGER_FRAG:starter_operator","quantity":10}
  ],
  "eligibility": {"all": ["first_contract_claimed", "zero_completed_purchases", "manager_system_unlocked"]},
  "surfaces": ["store", "manager_sources", "first_purchase_intro"],
  "maxPurchasesPerAccount": 1,
  "offerCalendarId": null,
  "experimentKey": "starter_01",
  "contentVersion": 1
}
```

### 14.4 Example loot table JSON (does not mean silently making it paid)

```json
{
  "id":"loot.recruit.standard.v1",
  "revision":1,
  "drawCurrency":"RECRUIT_TICKET",
  "totalWeight":10000,
  "outcomes":[
    {"id":"legendary_manager","weight":200,"quantity":1},
    {"id":"epic_manager_fragments","weight":800,"quantity":5},
    {"id":"rare_manager_fragments","weight":2000,"quantity":5},
    {"id":"universal_manager_fragments","weight":2500,"quantity":5},
    {"id":"manager_xp","weight":2500,"quantity":100},
    {"id":"skill_books","weight":2000,"quantity":2}
  ],
  "guarantees":{"legendaryByDraw":80,"milestoneDraws":[10,30,60]},
  "oddsDisclosureKey":"odds.recruit.standard.v1"
}
```

The example uses abstract reward category IDs; the production table must expand these to actual eligible manager IDs, published category-to-item probabilities and exact quantities. If pity alters distribution, present effective pity conditions and guarantee separately, and audit every roll.

### 14.5 Typed commands / events

Commands: `SolveRound`, `ClaimRoundRewards`, `StartConstruction`, `FinishConstruction`, `SpendSpeedup`, `CollectProduction`, `UpgradeBusiness`, `AssignManager`, `PromoteManager`, `UpgradeGear`, `ResearchTech`, `StartContract`, `ClaimContract`, `OpenChest`, `MergeChest`, `ChooseChestReward`, `Recruit`, `RefreshMerchant`, `BuyMerchantStock`, `ClaimDaily`, `ClaimEventMilestone`, `ClaimPassReward`, `ClaimVIPGift`, `RedeemFragment`, `SelectCosmetic`, `InitiateIapPurchase`, `ApplyVerifiedIap`, `Refund/RevokePurchase`.

Events: `RoundVerified`, `ChainComboAchieved`, `CashGranted`, `ConstructionCompleted`, `BusinessUpgraded`, `ManagerRecruited`, `ManagerPromoted`, `ContractClaimed`, `ChestOpened`, `ShopRefreshed`, `DailyMissionCompleted`, `EventPointsAwarded`, `PassLevelReached`, `VIPLevelReached`, `PurchaseVerified`, `EntitlementGranted`, `PurchaseReversed`.

**Critical sequence:** a command validates state/cost/config/trusted time -> one database transaction deducts cost/grants rewards/logs domain event -> async projection/notification consumes event idempotently -> client receives authoritative state diff and reward receipt -> animations render receipt. No server response => no false paid reward claim. Retried requests use same idempotency key and receive original result.

### 14.6 API shape (choose proven provider; do not hard-code proprietary SDK calls without checking)

```text
POST /v1/auth/guest
POST /v1/auth/link
GET  /v1/account/snapshot?sinceVersion=N
GET  /v1/config/manifest
POST /v1/puzzle/verify-and-claim
POST /v1/economy/command
GET  /v1/empire/status
GET  /v1/events/active
GET  /v1/shops/{id}
POST /v1/shops/{id}/refresh
POST /v1/loot/open
POST /v1/recruit/draw
POST /v1/iap/validate-and-fulfill
GET  /v1/iap/entitlements
GET  /v1/ledger?cursor=...
GET  /v1/leaderboards/{mode}
POST /v1/support/report-transaction
```

All mutation routes authenticated via rotating session token, TLS, rate limited, request ids, replay prevention and version checks; never ship service secrets in Unity app. Server clock authoritative. JSON schemas versioned. Backoffice requires separate privileged credentials. Avoid overengineering real-time sockets until truly needed; polling + refresh suffices for asynchronous model.

### 14.7 Local state and migration

New `PlayerProfileV3`/`EconomyV1` separated from old schema 2. Migrate stars, best scores, free/paid level entitlements, blueprint ownership, skins and documented consumable balances. Store migration snapshot/backups, checksum, one-time migration ID, restoration rollback path and automated legacy fixtures. Do not zero legitimately earned old credits without an explicit mapping: convert old credits to new Cash at a defined, logged, capped conversion and preserve their equivalent spending utility. Do not invent purchased balances with insufficient receipt history. Old offline saves remain playable if cloud unavailable; server sync conflict UX offers safe import and support path, never silent overwrites. Normalize old `cc.*` SKUs to new entitlement IDs without repurchase. Upgrade `Profile.Validate` carefully rather than simply raising max counters. Game board retains saved current round across app updates.

---
## 15. PURCHASE FULFILLMENT, PLATFORMS, REGIONAL SAFETY

### 15.1 Unity IAP and authority

Current `LocalStore.cs` and `PurchaseDelivery.cs` are a starting point. Unity IAP **5.4.4** uses asynchronous `StoreController` purchase callbacks and explicit order fulfillment. For new CC2 commerce:

1. Fetch real Play product metadata and registered catalog IDs, render only actual available prices. Purchase button disabled on missing/invalid product or offline billing.
2. `OnPurchasePending(PendingOrder)`: do **not** immediately grant client currency. Send receipt/transaction reference to backend. Backend authenticates with platform verification API and checks package/product, account binding, acknowledged state, transaction uniqueness and purchase status. Store exact transaction ID securely or digest plus platform proof as appropriate.
3. In one database transaction, create the grant ledger record and exact entitlement/inventory changes once; respond signed receipt/fulfillment status. Client updates from authoritative state and **only then confirms purchase** through Unity IAP with correct `PendingOrder`. A network crash before confirmation is redelivered, and duplicate transaction handled idempotently.
4. Deferred/pending payment: do not grant. Failure/cancel: restore UI, no penalty. Outages: keep purchase pending, reconcile later. Reconnect: fetch purchases + server entitlements. Refund/revoke: policies implemented with audit history and purchased entitlements adjusted; never silently confiscate more than is appropriate for refunded item.
5. Permanent entitlements restore across devices/account link; consumable receipt history tied to durable backend wallet. Reconcile server state over local display; no cross-device ‘consumables may be lost’ as the final architecture.
6. Do not mark purchase acceptance as complete until **real Play sandbox** runs for success, pending, deferred, canceled, duplicate redelivery, offline interruption, account change, restore, refund and stock exhaustion.

Use current Unity IAP v5 documentation, not old `IStoreListener`/v4 examples. Reference: `https://docs.unity.com/en-us/iap/purchases` and `https://docs.unity.com/en-us/iap/payment-providers/fulfill-purchases`.

### 15.2 Paid randomized rewards / shop refresh

Google Play: paid randomized virtual rewards must disclose odds beforehand and close to purchase; official `https://support.google.com/googleplay/android-developer/answer/9858738?hl=en`. Apple (future iOS): loot box odds must be disclosed before purchase; `https://developer.apple.com/app-store/review/guidelines/`. Implement odds drawers on real-money-priced randomized packs **and** on indirect purchasable routes where currency can be bought. All shop reroll outcome pools/probabilities disclosed when chance is purchased. Eligibility/featured sub-pools/caps and duplicate treatment part of versioned manifest. Region/age policy gating configurable; disallowed mechanisms fallback to deterministic choice rewards. Never misrepresent rarity visual presentation. No wager/cash-out economy.

### 15.3 Ads and rewarded value

Rewarded ads opt in, disabled until actual production ad SDK integrated, privacy/consent data flow documented. Possible placements: post-result +X bounded materials, one extra daily merchant voucher, single low-tier chest, modest contract-speed credit. No forced interstitial in tutorial/board, no double-skip trick, no misleading reward. `RewardedAdCompleted` requires trusted SDK callback and idempotent daily cap. Ads reward should be balanced against IAP without paywalling basic content; remove ad placements where privacy/region/child policy requires. Advertising income recorded separately from IAP and tested for retention harm.

### 15.4 Privacy, children, taxes and refunds

No forced email/account for opening game. Optional sign-in and transparent data handling for account recovery. Consent policy and GDPR/India DPDP/US state requirements reviewed for actual audience and SDKs. Data deletion/export, opt-out personalized offers where required, support and refunds. Avoid individualized discriminatory pricing. No exposure of player phone/email/precise location to other users. Publish actual privacy policy and Data Safety based on installed SDKs, not placeholder text. Age gate where necessary for random paid items/ads; safety mode deterministic rewards if needed. Backoffice must not expose purchase secrets. No gambling-like cash redemption.

---

## 16. ANALYTICS, EXPERIMENTS, MONETIZATION OPTIMIZATION

### 16.1 Typed event schema and provenance

Every telemetry event: `eventId,anonymousPlayerId,sessionId,clientVersion,configVersion,eventName,clientTime,serverTime?,cohortId,experimentAssignments,screen,context,properties,consentFlags`. Sanitize PII; no raw purchase receipts, secrets or private identifiers in logs. Offline telemetry queue bounded, encrypted/appropriate, sends only with policy permission and handles deletion/opt-out.

**Required event taxonomy:**

| Domain | Events |
|---|---|
| Acquisition/onboarding | `install_open`, `tutorial_step_view`, `tutorial_step_success`, `first_chain_success`, `tutorial_abandoned`, `first_empire_unlock` |
| Board/input | `board_enter`, `piece_drag_start`, `piece_drop_valid`, `piece_drop_invalid`, `rotate`, `undo`, `forecast_open`, `chain_activate`, `round_result`, `combo_discovered`, `result_next` |
| Empire | `business_unlock`, `construction_start`, `construction_finish`, `production_collect`, `inventory_full`, `upgrade_attempt`, `upgrade_complete`, `research_start`, `research_finish` |
| Dispatch | `contract_offer_view`, `contract_start`, `contract_complete`, `contract_claim`, `contract_refresh` |
| Rewards | `chest_obtained`, `chest_open`, `loot_rare`, `choice_chest_select`, `chest_merge`, `duplicate_convert`, `collection_complete` |
| Recruitment | `banner_view`, `odds_view`, `draw_start`, `draw_result`, `pity_progress`, `guarantee_claim`, `manager_promote` |
| LiveOps | `event_view`, `event_task`, `event_milestone`, `pass_xp_earned`, `pass_reward_claim`, `merchant_stock_view`, `merchant_reroll`, `vip_level` |
| Commerce | `offer_eligible`, `offer_impression`, `offer_open`, `offer_dismiss`, `purchase_initiated`, `purchase_pending`, `purchase_verified`, `purchase_failed`, `purchase_refunded`, `sku_ownership_restore`, `currency_spent` |
| Retention/support | `session_start`, `session_end`, `return_report_open`, `push_opt_in`, `sync_error`, `billing_error`, `save_recovered`, `account_linked`, `support_ticket` |

Event properties include logical screen path, item IDs (not arbitrary full payloads), actual price tier/ISO currency from platform, display context, resource shortage amount, earned/purchased acquisition type, time since milestone, valid experiment exposure. Impressions fire only when reasonably visible, not on screen construction or each frame.

### 16.2 KPI dashboard definitions

- `D1/D3/D7/D14/D30` cohort retention by installation UTC cohort; distinguish active app session and notification open.
- Puzzle tutorial completion, first chain median/p90 time, valid-drop %, drag abort %, forecast usage, median round duration and repeat plays.
- DAU, WAU, MAU, sessions/user/day, sessions duration p50/p90, eligible feature usage, contracts/session, chests/day, merchant visits/refreshes, early churn reasons.
- Pay conversion by day and feature gate, paying DAU, first-purchase conversion, repeat-purchase ratio, offer-impression-to-purchase, IAP ARPDAU, total ARPDAU, ARPPU, whale concentration (risk monitor), refunds, chargebacks, subscription/card renewals (if active), paid Gem outstanding liability.
- Cohort net LTV at D7/D30/D90 and predicted payback by acquisition channel. Report total gross IAP, platform fees, ad payout, refund reserve, backend cost and UA spend to measure contribution, not only gross revenue.
- Free/paying gap, fair-mode participation, season pass completion, purchase-related churn, self-reported unfairness, regret signals and support incidence.

No claim of 'maximize money' is meaningful without verified analytics. Build dashboards and cohort exports before paid scaling.

### 16.3 Experiment design and guardrails

Run randomized, remote-config-assigned experiments that vary **clear utility and value**: construction timer pacing, chest-versus-choice reward mix, merchant stock variety, relevant pack compositions, first-purchase pack value, pass track attractiveness, onboarding trigger placement, rewarded-ad availability, UI/tactile presentation. Avoid undisclosed changes to paid RNG odds or manipulative hidden price targeting. Pre-register metric, guardrails, minimal sample/decision rule and time horizon. Primary metric net D30/D90 contribution/LTV; secondary conversion; guardrails D1/D7 retention, refunds, free-player completion, frustration, crash-free session rate and support.

Failed experiments rollback by config revision. A/B assignment stable per user/account. Zero-user trials and automated bot play do not validate revenue. For extremely low traffic, focus qualitative playtests and iteration rather than misleading significance claims.

### 16.4 LiveOps/admin control panel

Authenticated restricted internal dashboard: publish/draft/preview configs; activate events/cohorts; set shop rotations; validate no invalid odds, impossible source chains or missing assets; preview all player states; pause a broken paid offer; revoke misconfigured event; examine ledger and support refunds; inspect content simulation alerts; manage localization; manual *audited* compensatory grants with operator credentials and approval rules. No debug admin economy bypass in public Unity build.

---

## 17. CONTENT DESIGN, ART BIBLE AND RELEASE ASSETS

### 17.1 Initial content minimum

- Existing 32 building recipes and 104 campaigns maintained; author 12 empire-operation templates and 20 repeatable constrained variants with solver coverage.
- 10–12 owned business progression definitions from existing art and 8–12 additional functional/visual upgrade states as budget permits; 12 original manager characters with distinct silhouettes/skill kits; 12+ equipment pieces and 3 sets; 30–45 research nodes across three trees; 15+ contract templates with tier variants; 8 original sets plus 2 fragment albums; 3 starting chest tiers + 4 specialty chests; 2 recruitment banner templates; 6-slot Daily Merchant; 5 specialty exchange shops; initial VIP 1–12; one 28-day season config; full event catalog from §7, with at least 4 active/validated templates on launch and remaining ready behind feature flags for measured deployment.
- 20+ real offer templates, shop variants and 4–6 gem/entry tiers; no fake SKU/price displayed. Contextual acquisition screens across all upgradeable resources.
- Full onboarding/returning player content, UI empty/error/locked states, iconography, new sound/VFX suite, localization string IDs, accessibility and 3 display aspect-ratio acceptance screenshots per major feature.

### 17.2 Content templates and localization

Every entity `id,displayNameKey,descriptionKey,assetKey,unlockConditions,dependencies,stats,resourceCosts,rarity,visualTier,sourceRoutes,featureFlag,contentRevision`. All localization strings in tables, not hard-coded C# screen logic; English default, architecture enables others. Right-to-left and long-string overflow tests at least on subset. Art assets with ownership/provenance, original authoring; no directly borrowed Century Games UI icons or characters. Maintain `Docs/ASSET_PROVENANCE.md`.

### 17.3 Cosmetic collections

Noncompetitive city themes (neon, cherry blossom, nighttime freight, bright commerce, luxury finance), building trims, trails, frames, emotes where social enabled, avatars and sound themes. Guaranteed contents for direct purchases. If decorative items carry stats in optional advanced features, they cease to be purely cosmetic and must be labeled plainly; keep launch cosmetics stat-neutral to preserve clarity.

---

## 18. QUALITY TESTS AND FAILURE INVARIANTS

### 18.1 Domain correctness

- Existing every 104 certified puzzle solution replay passes untouched after migration; existing tests and deterministic property suites pass (the repository previously reported 59 domain tests, but current exact count must be verified at baseline).
- 100k randomized legal boards/resolver runs or optimized equivalent stress test; conservation, no fractional/missing resource, no unbounded cycles, no duplicated one-time awards.
- Every config has valid IDs, no missing art/localization, no circular unlock dependency, no unearnable prerequisite, rarity weights sum exact, no source/sink arbitrage cycle, no duplicate SKUs/currency definitions, no negative or overflow prices, no invalid timer durations and no impossible free mode.
- Business/manager upgrade costs, equipment, research, VIP, temporary benefits, queue slots, capped passive accrual all have deterministic unit and integration tests. No over-100% timer discount; no unbounded production accumulation.
- `RoundCompleted` can advance eligible 6 subscribers without duplicate grants; repeated event delivery idempotent. Cross-reset and season rollover correctly archive progress and avoid duplicate claims.

### 18.2 Economy and commerce tests

- Source finder always points to at least one valid earnable source for required nonexclusive progress.
- Seeded chest distribution across 1 million simulated rolls statistically agrees with configured probabilities within tolerances; test guarantees exactly at pity threshold and after restore, batch equivalence, duplicate conversion and correct table revision.
- Merchant refresh: first free, paid cost ladder exact, stock persist, cap, expiry, concurrency, odds disclosure and transaction rollback.
- Buy choice pack then pick variant; guarantee exactly selected grant, never both. Paid item refund and entitlement reconciliation audit.
- IAP: price unavailable, cancel, pending/deferred, duplicate callbacks, process death between ledger commit and IAP confirmation, store restore, rejected receipt, product mismatch, account switch, consumable repeat, refund, partial network outage. Never charge twice nor grant twice; visible feedback factual.
- Negative Gem balance, manually edited local profile, changed wall clock, API replay, concurrent server calls, falsified event score, legacy invalid save, account merge and partial cloud sync fail safely.

### 18.3 UX and device matrix

- Actual device/emulator Android captures for first launch, every unlocked system, all currency shortage sheets, rare and normal chest, merchant before/after refresh, paid-product metadata unavailable, event ended, offline resume, purchased SKU owned, save migration, pending IAP.
- 9:16, tall, short, tablet, landscape desktop companion; safe area, Android Back gesture, portrait rotation and resume mid-animation. Hitbox and overflow scan, right-to-left test strings, reduced motion, color high contrast and low frame-rate testing.
- At least 10 real first-time participants for core onboarding usability iteration and additional paid/nonpaid testers for longer-term behavior. No self-reported fun assertion based solely on automated test success.
- Performance: crash-free sessions, p95 frame time under target device budgets, no memory leak after 60 chest opens/100 rounds, stress 30 minute play, network reconnection, background transition, thermal impact.

### 18.4 Security and incident drills

Simulate backend outage after paid confirmation, payment provider outage, DB failover, mid-event config mistake, mispriced real SKU, odds manifest mismatch, state corruption, chargeback surge, time change, duplicated RTDN/webhook event, excessive requests, image asset failure and telemetry failure. Feature flag kill switch must disable affected paid offers safely while leaving ordinary game playable; support refunds and accurate affected-user audit.

---
## 19. CODEX IMPLEMENTATION ORDER — COMPLETE DELIVERY PROGRAM

**Rule:** Every phase is a code + data + UI + actual testing vertical slice. Codex should implement sequentially, committing and updating progress report after each. A future phase may be feature-flagged but not silently skipped. Do not promise full game done after one generated pass. Perform all work that is technically available; stop for external billing/credentials/legal approval only when unavoidable.

| Phase | Required code and content deliverables | Exit gate |
|---|---|---|
| **P00 Audit and freeze** | Baseline branch, precise git SHA, original docs, UI captures, all tests, existing store ids, source map, technical debt | Existing game compiles; baseline screenshots/build checksum captured |
| **P01 Decision/spec migration** | Updated `AGENTS.md`, superseding decision docs, implementation tracker, architecture diagram, catalog JSON schemas, source/sink graph | No unrecorded design conflicts; all content schema validation tests |
| **P02 Economy foundation** | Ledger, wallet, inventory, item catalog, RewardTransaction, source finder interfaces, versioning, idempotent domain command handling, legacy-save migrator | Credit/entitlement migration fixtures; rollback; 0 duplicate grants |
| **P03 Empire vertical slice** | Owned businesses, initial franchises, buildings/plots, one construction queue, upgrades, storage, real capped production; responsive UI | Certified puzzles unchanged, first empire loop playable fully offline sandbox/server test |
| **P04 Contracts and research** | Renewable verified dispatch contracts, capacity, trusted timers, research tree, speedups, dispatch screens | Free path to all first research/dispatch goals; all timer/clock tests |
| **P05 Managers/equipment** | 12 original managers, roles, initial unlock, star fragments, XP, abilities, 3 gear slots, UI and actual effects | Clear business/manager benefit without breaking fixed puzzles |
| **P06 Rewards and collecting** | Chests, choice and merge, deterministic sample seeded tests, RNG service, album fragments, duplicates, animations and claim history | No missing reward, item loss or phantom outcomes; odds tested |
| **P07 Recruitment/gacha** | Intro ticket, banners, odds UI, guaranteed thresholds, pity persistence, batch draw, duplicate conversion, source finder | Exact effective odds, draw history, guarantee and refund-safe records |
| **P08 Shops/rotations** | Merchant stock, one free reroll, paid vouchers/Gems rerolls, specialty shops, price/currency distinction, expiry, stock, contextual acquisition | Stable server-side stock, concurrency and migration safe; no free-stock exploit |
| **P09 Offers/VIP/cards** | New Offer engine, product template library, dynamic eligibility, storefront redesign, VIP 1–12, weekly/monthly cards, growth fund, sequential packs, recharge rewards | Every SKU eligibility truthful; source finder integrates paid/free routes |
| **P10 LiveOps/pass** | Typed event subscriptions, event scoring, 28-day season, free/paid tracks, 14 event definitions, timed event shops, content generations, remote config | Event stacking idempotent; end/start/offline/late join cases tested |
| **P11 Backend/billing** | Deployed/staged account/economy/auth/config services, API, production-like datastore, server purchase verification, Play IAP integration, restore/refund flows, audited admin operations | Sandbox matrix and security QA complete; `BLOCKED_EXTERNAL` if Play account keys missing |
| **P12 Analytics/admin** | Typed event telemetry, dashboards, cohort metrics, config A/B testing, balance simulator, backoffice tools, consent handling | Metrics reconciled with server grant ledger; config rollback works |
| **P13 Social and competitive** | Verified skill leaderboard, Empire League, genuine asynchronous group projects and alliances/gifts, moderation/reporting | No fake users, duped scores or abusive purchases; solo player not disadvantaged from core progression |
| **P14 Polished product UX** | All 41 screen states, art/audio/VFX, first-session speed, haptics, reduced motion, content localization, accessibility, screen-density tuning | Captured real Unity/Android screenshots; gesture and frame-time gates |
| **P15 End-to-end content/balance** | Authored tier costs, 10–12 franchise lines, research, manager/gear progression, 12+ repeatable operations, events, all offer variants, art/QA | 10k–100k simulations and 1–180 day economy reports pass; playtester issues addressed |
| **P16 Release candidate** | Play Console product IDs, billing/license/config, privacy/Data Safety, store assets, Android AAB, external closed test where required, 3 device types, support docs | Real testing and policy audit passed; operator explicitly authorizes publishing |
| **P17 Post-launch instrumentation** | Gradual rollout, live economy monitoring, retention and monetization experiments, refund/support alerts, safe hotfix | No unresolved severe economy exploit; data-driven iteration, no invented results |

Subphases may overlap development for efficiency but cannot pass gate on unverified dependent components. Prefer a **small complete end-to-end slice** from gameplay -> reward -> inventory -> earnable resource -> upgrade -> offer -> authoritative fulfillment before proliferating 30 screens. After vertical slice is solid, generalize config and fill the required catalog.

### 19.1 Acceptance packages Codex must generate

For every Pxx: `Docs/CC2/PHASE_Pxx_REPORT.md` with git SHAs, exact files touched, all source/asset IDs, compile/test commands and numerical results, fresh Unity capture paths for before/after at 3 screen sizes, blocked external credentials, current feature flags, known issues severity P0/P1/P2, and next changes. Update `Docs/CC2/TRACEABILITY.csv` mapping every requirement of this spec to feature flag, code path, test ID, screenshot proof, and launch state. Missing traceability = not implemented. Maintain `Docs/CC2/DECISIONS.md`, `Docs/CC2/ECONOMY_TABLES.md`, `Docs/CC2/CONTENT_MANIFEST.md`, `Docs/CC2/BALANCE_REPORT.md`, `Docs/CC2/COMPLIANCE_CHECKLIST.md`, `Docs/CC2/RELEASE_STATUS.md`.

### 19.2 Build/test commands

Use original Unity test/build scripts (`Tools/test.sh`, `Tools/build.sh linux|android`, `Tools/playtest.py`, `Tools/campaign-playtest.py`, `Tools/android-playtest.py`) only after confirming current support. Add `Tools/cc2_content_validate.py`, `Tools/cc2_economy_sim.py`, `Tools/cc2_legacy_migrate_test.py`, `Tools/cc2_contract_test.py`, `Tools/cc2_liveops_replay.py`, `Tools/cc2_store_mock_test.py` (mock only inside editor/integration tests, never a fake provider in production player build). Pin Unity editor and packages, cap workers to avoid OOM, and never mark failures as success due to missing UI driver input.

---

## 20. FEATURE-TO-MONETIZATION NETWORK TRACEABILITY

This is the reference map of **mechanisms**, not just named packs. Every row is delivered by a canonical feature above or explicitly deferred by a truthful feature flag; no separate bespoke code for every row.

| Mechanism family | Mechanisms incorporated | Owned implementation |
|---|---|---|
| Free-to-paid entry | free useful unlock, low-cost first purchase, partial fragment unlock, free premium currency, introductory trial boost, initial free chest/draw, visible long upgrade track, alternative earned route | Tutorial gates, Starter Offer, Business/Manager Progression, SourceFinder |
| Time/capacity | construction/dispatch/research wait, speedups, general speedups, parallel queues, storage, production cap, auto-claims, contract speed and capacity | Empire, Contract, Research, VIP, Utility |
| Quality progression | levels, stars, rarities, gear, gear mastery, manager skills, ability unlock, research prerequisites, collections, late materials and content generations | Business, Manager, Equipment, Research, Generations |
| Chance | weighted chest, themed chest, featured banner, duplicate conversion, batch draw, pity and guarantees, milestone ladder, nested table optional, chest merge, choice chests, rarity reveals | RewardEngine, Recruitment, Collection |
| Random shops | timed stock, randomized shelves, one free reroll, voucher/Gem paid reroll, refresh cost escalation, disclosed probability, rarity guarantee where promised, limited stock, VIP shops, event shops, barter merchants | ShopEngine, OfferEngine, Inventory |
| Multiple currencies | Cash, Gems, activity credits, fragments, research docs, speedups, event tokens, VIP XP, trade exchange | ItemCatalog, Wallet, SourceSinkGraph |
| Offer packaging | starter, entry pack, targeted progression pack, generic resource pack, gem top-up, permanent unlock, choice pack, weekly/monthly card, season pass, growth fund, event bundle, cumulative recharge, sequential step ladder, bonus claim tiers, visual collection, location-bound acquisitions | OfferEngine, Billing, Cards, EventPurchaseTrack |
| Rotation and relevance | account unlock triggers, elapsed tenure, content cohort age, generation, VIP, previous purchase, owned item, resource shortage, scheduled event, regional price/availability, returner eligibility | EligibilityRules, RemoteConfig, TrustClock |
| Event stacking | quests, progress bars, milestone claims, free/premium track, purchase grants, event material usage, ranking, guild contributions, multi-event scoring for same action | EventBus, Passes, LiveOps, Social |
| Retention clocks | immediate satisfying interactions, result progression, minute/hour timers, daily mission/merchant, weekly events, 28-day season, content generation, persistent account goals, configurable legitimate push | UX, Timer, Daily, EventScheduler |
| Social/status | fair skill competition, progression-power league, alliance gifts for eligible purchases, shared projects, avatar frames, city themes, VIP prestige and collections | League, Social, Cosmetics, VIP |
| Friction and fairness | free core actions, free basic preview/undo, nonpaying progress, deterministic guarantee alongside RNG, legitimate free sources, purchases as convenience/acceleration, explicit paid power on noncertified modes | CoreModePolicy, ContentValidation, SourceFinder |
| Reward theater | snap/drag/rotate, chain activation, true profit particles, opening chests, rare rarity VFX, collection completion, multi-reveal, unlock animation, currency flight, success sounds/haptic | InteractionFeel, GameFeel, RewardFX |
| Data optimization | offer relevance, segment/cohort config, funnel analytics, player economy, price metadata, experiments, refund retention checks, audience/privacy restrictions | Analytics, Backend, Config, Backoffice |

### 20.1 Resource destination / action-to-progression matrix

| Action | Immediate result | Secondary progress | Tertiary commercial entry |
|---|---|---|---|
| Move/rotate piece | Real score forecast changes | Skill understanding and combos | Eligible utility boosters only in empire mode, never paid undo |
| Activate chain | Round score and actual earned reward | Daily mission, franchise XP, event, collection, season XP | Event packs/currency, growth fund relevance |
| Finish build | New productive capability | Construction event and empire level | Builder slot, speedups, materials |
| Claim dispatch | Cash/resources/chest/fragment | Contract event, manager XP, weekly goal | More dispatch slots, speedups |
| Unlock business | New permanent asset | Album and future star goal | Fragment choice boxes, targeted packs |
| Promote business | Stat/ability breakpoint | Event, VIP progression (if defined), collection | Scarce fragments, research, equipment |
| Recruit manager | New management option | Pity counter/milestones/event | More tickets, gear and skills |
| Open chest | Items, rarity reveal, duplicates | Album/promotion readiness, chest milestones | Earned source finder, optional related bundle |
| Reroll merchant | Changed eligible stock | Merchant task, new selection | Refresh voucher/Gems |
| Finish research | Efficiency and unlocks | Research event and new tiers | Documents and speedups |
| Collect production | Real cash/materials | Collection target/daily task | Storage/efficiency utilities |
| Complete collection | Set reward, badge, bonus | Higher-tier album target | Choice fragment boxes/cosmetics |
| Reach VIP tier | New rewards/benefits | Prestige progress | VIP special pack, eligible merchant |
| Claim pass mission | Free reward plus XP | Pass level and event goals | Premium track if chosen |
| Purchase eligible pack | Exact bought items | Eligible VIP XP / top-up track / alliance gift | New progression needs, not automatic upsell |

---

## 21. FULL DEFINITION OF DONE

A system is **DONE** only when its free loop, paid/earned acquisition routes, UI states, accessibility, save/backend authority, balancing data, telemetry, failure modes and real tests exist. A mock character list or UI panel alone does not satisfy this spec.

**Overall game qualifies as 'feature-complete RC' when:**

- Existing 104 challenges, deterministic solver and original entitlements are preserved and tested.
- Persistent business empire, managers, equipment, research, renewable contracts, production caps, fragment progression, source finder, chest/choice/merge/draw/pity, merchant rerolls, VIP, passes, cards, genuine offer rotation and content generations all work together on authenticated durable economy state.
- Offer catalog and odds are accurate; every active purchase product verified and restorable, genuine Play product metadata, no mock success and accurate refunds.
- Every major screen above works for unlocked/locked/empty/offline/pending/complete/error state; every tap/drag/rotation visually and mechanically responds, no decorative pseudo-controls.
- New events actually subscribe to verified domain actions; one event can advance multiple eligible reward tracks but never duplicate currency grants.
- Retention features and notifications are meaningful, optional and nonpunitive; free players have non-paywall progression and a gratifying reward cycle.
- Backend audits, RNG manifests, economy simulator, cohort telemetry, content publishing/rollback, security tests, Play policy and privacy review are complete.
- Actual Android physical-device and Play billing sandbox acceptance plus external tester feedback exist (not just a simulation), and release has explicit operator approval.

**Launch only after complete implementation is validated.** An initial release can gate expensive social and late-generation features behind flags but the code, config, tests and safe activation path for those promised features must be finished if this is to be handed off as an implementation of *everything*.

### 21.1 Explicit operator-blocked dependencies

Codex may require access/credentials for Play Console app/products and license config, production hosting and secret management, analytics/ad network IDs, privacy policy publisher, consent SDK setup, signed AAB/upload and physical device/human tester verification. Never invent them. Mark `BLOCKED_EXTERNAL` and ask only for the precise credentials/approval after the independent coding is complete. No publication until user explicitly authorizes it.

### 21.2 Work products to return

1. Git branch and commits, passing build/test log, screenshot/video gallery, Android test AAB/APK and backend infrastructure/deployment scripts.
2. Economy content tables, generated source/sink graph, item/catalog schema reference, event/offer catalog, odds disclosure manifests, backend/API documentation, admin operation manual and migration report.
3. Simulated 180-day cohort economy report with notes on nonpayer advancement and pricing hypotheses, telemetry dashboards and declared unknowns.
4. Full QA/security/payment matrix and compliance checklist; explicit remaining `BLOCKED_EXTERNAL` items, no false production-ready label.
5. One-page operator runbook describing how to rotate packs, launch an event, alter reward tables, roll back unsafe configuration, issue auditable compensation and review revenue/retention.

---

## 22. HANDOFF INSTRUCTION TO CODEX — COPY VERBATIM

> Implement **CASHFLOW CHAINS 2.0** from `CASHFLOW_CHAINS_2_MASTER_REVAMP_SPEC.md` against `https://github.com/zaynvirkk/Cashflow-Chains`. This specification supersedes conflicting old product/commercial direction while preserving existing deterministic puzzle outcomes, all existing earned progress, paid entitlements, UX accessibility and validated gameplay. Read all repository docs/sources, run baseline tests and create a safe feature branch first. Implement all 22 specification sections in ordered P00–P17 vertical slices, with actual Unity/C# code, data catalogs, original art/UI/audio, a secure authoritative backend, real compliant Play IAP integration, full economy and reward systems, contextual offers, rotating merchants, gacha/chests/pity, VIP, passes, events, social/competitive features, telemetry and simulator. Do not leave stub handlers, fake paid prices, decorative interfaces or skipped requirements. After every phase compile, test, run genuine gameplay, capture actual screenshots and report full test evidence and blockers. Do not claim publication, player retention or revenue without genuine evidence. Where external secrets, app-store configuration or human QA prevent acceptance, implement everything independent, mark the exact `BLOCKED_EXTERNAL` gate, and seek operator action. Do not publish to a store or change existing paid ownership without explicit authorization.

---

## 23. REFERENCES AND RESEARCH BOUNDARIES

Repository-specific grounding: `https://github.com/zaynvirkk/Cashflow-Chains`, current baseline sources and `Docs/` listed above. Cross-game inspirations are high-level commercial architecture patterns observed in Kingshot and Whiteout Survival: staged hero acquisition, upgrade shards, complementary stat trees, VIP, timed construction, active events, dual reward tracks, rotating merchant/reroll, reward chance, server-age generations, distinct acquisition currencies, purchase-linked social gifts. **This document does not assert possession of either company's private monetization code, account targeting formulas or proprietary balancing.** All concrete Cashflow figures are initial game-design hypotheses to test, not factual claims about those games or guaranteed commercial outcomes.

Official platform guidance (check current rules again immediately before release):

- Google Play payments and purchasable random item odds: https://support.google.com/googleplay/android-developer/answer/9858738?hl=en
- Apple App Review Guidelines (if iOS later): https://developer.apple.com/app-store/review/guidelines/
- Unity IAP v5 purchases and fulfillment: https://docs.unity.com/en-us/iap/purchases and https://docs.unity.com/en-us/iap/payment-providers/fulfill-purchases

**End of master specification.**
