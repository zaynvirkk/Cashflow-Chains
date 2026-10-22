> Governing update: CC2 master in Docs/CC2 and the22October supersession decision override incompatible product rules below. Historical specification is retained.

# CASHFLOW CHAINS
## FULL UNITY/CODEX GAME PRODUCTION SPECIFICATION
**Version 1.0 - 20 Oct 2026 - product design / ready for implementation planning, not commercially validated**

**Read first:** `00_START_HERE.md`, `03_SHARED_TECH_MONETIZATION_LAUNCH.md`, `04_CODEX_AGENTS_AND_WORKFLOW.md`, `05_RELEASE_AND_TEST_MATRIX.md`. No single milestone allows claiming the whole game is finished.

# C01. One-sentence pitch, differentiation and fantasy
**Arrange a miniature city to create magnificent chain reactions of money, goods and customers, then unlock more sophisticated businesses, cities and strategic possibilities.** Each short round is a transparent spatial economy puzzle; player intelligence and experimentation make a profit engine explode with visual feedback. The player isn't running an open-world simulator and isn't just tapping to upgrade +10% income. The city map is a colorful progression trophy, not an autonomous agent simulation.

**Three marketing promises (must be shown in real build):**
1. Place or rotate just one business and visibly transform an entire animated cashflow chain.
2. Turn a humble street into an ingenious compact money machine; score depends on layout, not tapping frequency.
3. Unlock new districts and business families that fundamentally change the combinations available.

Positioning: free-to-play, IAP-first optional, one-thumb-friendly, portrait, offline single-player, age rating determined by actual art/ads. Round length 1-4 min novice, 3-8 min experienced; unlimited free sandbox/challenge replay without premium refills.

# C02. Locked user experience constraints
- Flat **top-down orthographic** board, no 2.5D, isometric art, 3D shadows, perspective roof faces, 3D meshes or trompe l'oeil buildings. Camera does not tilt. Building icons may be rich flat illustrations drawn from directly overhead, strong silhouettes, crisp outlines, translucent 2D glows. Decorative skyline behind UI may be side-on flat illustration clearly separate from playable board.
- Complete first successful chain **within 45 seconds** of user control; player sees at least one improvement made by their own action within first 90 sec.
- No forced signup, paywall or interstitial during onboarding. Pause, relaunch and resume any round. No mandatory idle waits.
- Finite grid puzzling first, no true real estate, stock markets, employees or bank regulation. “Income” purely fictional numerical score/soft currency, never real money.
- Long-run replay should be skillful: each business piece has a small interpretable rule; modifier variety and piece drafting limit solved layouts.

# C03. Session state machine
`Boot -> Title/Continue -> ContextualTutorial or DistrictHub -> ChallengeSelect -> Preparation -> Arrangement -> Preview -> Activate -> Result -> RewardGrant -> Hub/Replay/NextChallenge`.

`Preparation`: choose unlocked piece roster and a limited budget. `Arrangement`: place/move/rotate allowed pieces, constrained by grid, tokens/paths, no cost for undo; icons show incoming/outgoing ports and rules; can tap `Preview` to see possible graph; no economy resolution before `Activate`.

`Activate`: disable placement; run **8 deterministic beats** at chosen readable speed (default 1x, option 2x), show bounded resource pulses and profit in readable steps, particles and screen feedback. `Result`: one conclusive score, target stars, newly earned resources, milestones, and `Retry`, `Next`, `Edit Last Layout`. Unlocked extra effects are cosmetic rather than mandatory for passing.

Allow `Reset layout`, `Undo`, and `Tap to inspect` any card, no traps or charges for correcting mistaken touch. Replays use identical seed unless the player deliberately chooses new modifier.

# C04. Complete round model - data and rules
**Production grid:** Tutorial 4x4, starter 5x5, commercial v1 main 6x6. Advanced scenarios 7x7 only if performance/UI test succeeds (feature gated). All rows/cols discrete integers; all cards occupy one tile, except explicit two-tile late-game landmark (out of v1). 4-direction orthogonal links only. Every piece has an orientation (N/E/S/W) and optional sockets; clicking Rotate always changes socket directions logically and visually.

**Resources are distinct:** `people`, `goods`, `credits`, `energy`. Keep them **integer units**, all money as signed 64-bit game credits. Money is a **fictional** scoring unit. Board economy uses `people/goods/energy` as throughput and `credits` as final outputs. A `CashflowNode` has `inputKinds`, `outputKinds`, `maxInputPerBeat`, `maxOutputPerBeat`, `conversionRecipe`, `bonusTag`, `incomingPorts`, `outgoingPorts`, `activationPriority`. Each item/recipe is explicit data, never inferred from its sprite or name.

**Sources** add resources: housing produces people, power station energy, warehouse may provide goods only if connected to factory/supplier source in that scenario. Converters consume inputs to produce output and credits (café consumes people+goods, factory consumes energy+materials to create goods, bank applies non-stacking final credit multiplier if eligible). No converter manufactures inputs it did not receive. Some passive buildings produce one resource from scenario-defined external supply at finite rates. Every rule must account for source endowments and action budget.

**Eight-beat tick resolution**:
1. At challenge start, zero all temporary inventories, set scenario allocations and deterministic seed; record each piece's starting stock if provided.
2. For beat 1..8, add capped source tokens; move tokens through orthogonally connected ports into next-beat inboxes with edge throughput capacity. A directed path exists only if ports connect appropriately.
3. Within each beat, each node processes at most its throughput limit, consumes only actual available stock, produces output tokens and profit according to integer recipe; outputs become eligible for transport next beat. No node may process the same token more than once in a beat. Use nonnegative bounded token counters.
4. Capacity, storage and spill/unsold goods recorded as bottlenecks; overflow cannot turn into cash. Deferred shipments are handled in deterministic ascending node ID + explicit tie priority, never Unity transform order.
5. Buffs compute from tags and positional relationships, but multiplicative bonuses are **capped**. Example: 1.2x adjacency synergy plus bank 1.5x, capped at combined max 3.0x per cash-producing action for v1. Round multiplier separate final objective score capped 8x, not accumulated through loops. Use rational fixed-point arithmetic (basis points, 10,000 = 1x), round with documented fixed mode, avoid floats in authoritative economy.
6. Output each `PulseEvent{source,target,resource,count,beat}` and `ProfitEvent{building,credits,reason,beat}`. Presentation animates event log at controlled speed; event log is truth, particles are decoration. Sum source-to-sink credit value and check conservation.
7. A completed contract is tested against total profit, constrained objectives, piece/budget constraints and optional combo diversity. No purchases affect eligibility on skill challenge leaderboards.
8. Persist `RoundResult` with challenge seed, board hash, score, result UUID, earned stars, achievements; grant rewards **once** via idempotent ledger.

**Graph loops:** loops are permitted as visual networks but never duplicate tokens. Token quantity is conserved and sinks consume resources; every token has beat TTL=8 and card throughput cap. Graph recursion forbidden. No explosive unbounded combinatorial multiplicative loops. Simulate graph with bounded arrays. A static solver enumerates safe layouts for tiny tutorial boards; generated challenges validated by randomized and heuristic search.

# C05. First piece catalogue - v1 mechanics
Every piece has `id, price, tier, input, output, cap, socket topology, synergy tags, unlock, iconArtKey`. Starter examples below are initial tuning hypotheses, not economy commitments:

| Piece ID | Role | Starter rule | Player insight |
|---|---|---|---|
| HOME | Source | Emit 3 `people`/beat, one east-facing outgoing port | Customers are fuel |
| CAFE | Converter | Consume 1 people + 1 goods -> 80 credits | Needs customers AND supplies |
| GROCER | Converter | Consume 2 people + 1 goods -> 110 credits | Faster payoff with volume |
| WAREHOUSE | Router | Receive up to 6 goods/beat; send max 6 over two outgoing ports | Prevent shortages |
| FARM | Source | Emit 2 goods/beat, 1 energy stock budget | Cheap supplier |
| POWER | Source | Emit 4 energy/beat, consumes no people | Machines must be powered |
| FACTORY | Converter | 1 energy + 1 goods -> 3 goods, max 2 crafts/beat | Scales downstream |
| MARKET | Converter | 2 people + 2 goods -> 220 credits | Better connections |
| OFFICE | Source/booster | Adds 1 people/beat and +10% adjacent commercial sales (cap applies) | Density matters |
| BANK | Amplifier | +25% profit for one directly connected commercial outlet, once per card | Placement matters |
| LOGISTICS | Router | Adds extra edge range (2 tiles), max 5 goods/beat | Bridge a gap |
| APARTMENT | Source | 6 people/beat, 2x cost/footprint opportunity | Scale vs expense |
| BOUTIQUE | Converter | Consume 1 people + 2 goods -> 240 credits | High-value specialization |
| PARK | Bonus | +15% one adjacent HOME and +5% café, additive bounded | Public appeal |
| PORT | Source/contract | Import 4 goods for 2 credits per unit, conditional scenario | Specialized supply |
| LANDMARK | Objective | Final score +25% if 3 different businesses connected, once | Reward diversity |

**Important**: “Café consumes goods” means the tutorial must either pre-stock that node or provide a visible FARM. Do not stage tutorial whose visual cash path omits the required supply. Art icons cannot display profit values that contradict actual rule output.

Target **24 functional piece definitions** for release across 6 families (residential, supply, retail, industrial, services, special), **12 scenario modifiers**, **6 districts × 12 curated/procedurally-assisted levels** = 72 core challenges. Optional verified seeded daily challenge, 15 rotating contracts, 30 permanent achievements and 6 cosmetic skin sets. Further expansion is post-launch contingent.

# C06. Combos and chain-reaction presentation
Combo means a named verified condition satisfied by actual graph + resource flow, not arbitrary near-neighbor sparkle. Starter named combos:
- `Morning Rush`: CAFE consumed at least 6 people routed from HOME/ APARTMENT.
- `Supply Master`: same WAREHOUSE feeds 3 profitable shops with actual goods.
- `Factory Town`: FACTORY output reaches at least 2 distinct retail sinks.
- `Financial District`: BANK successfully amplifies 3 different cash events from adjacent commercial nodes (within single eligible adjacency; bounded).
- `Local Loop`: ≥3 distinct business families contribute to a path that creates revenue.
- `Luxury Row`: BOUTIQUE near PARK and BANK, credits generated with capped modifiers.

Every combo triggers distinct flat-2D animation: cyan pulse on edges, gold path fan-out, mint currency particles, pulse-count ribbon, sound chord; animations **must be skippable**. `ComboDetails` modal explains underlying requirements and exact earned profit. Avoid colored blobs hiding puzzle comprehension.

# C07. Challenges and difficulty progression
Each challenge spec includes: `seed, size, budget, startingPieces[], draftChoices[], lockedTiles[], targetScore, secondaryObjectives[], tags, modifiers[], allowedPieces[], rewards, expectedSolutionBands`.

Three-star score model: 1 star reach main target; 2 stars meet main target and finish under soft budget; 3 stars also achieve optional specific combo or efficiency objective. Never block progression on three stars; first star unlocks next challenge. After first clear, player can optimize freely and compare against personal best.

Introduce complexity gradually (measured by first three unique concepts):
- Tutorial T0: housing + cafe with pre-stocked goods, connect and activate.
- T1: choose orientation so resource reaches shop.
- T2: optional warehouse to split goods; first noticeable >2x uplift.
- District 1 levels 1–12: homes, cafés, farms, markets, warehouses; budget/limited slots.
- District 2: office and bank, multiplier caps and synergies.
- District 3: factory and energy paths.
- District 4: retail festival and demand modifiers.
- District 5: logistics range, port imports, bottlenecks.
- District 6: mixed families, conditional landmarks, advanced contracts.

Difficulty must come from new choices, not inflation-only targets. `ConstraintGenerator` recombines validated budgets, tile blockers, drafts, modifiers; solver checks reachability and nontriviality using bounded search; curate first 12 levels by hand. Use static scenario IDs for mandatory levels; procedurally generated daily is secondary. Tests ensure no forced payment to win and no one dominant layout >90% of solved seeds.

# C08. Controls and moment-to-moment feedback
- Tap piece tray to select, tap empty legal tile to place, drag existing tile to new slot, tap selected piece to inspect, dedicated Rotate and Undo accessible by thumb, optional long-press information; explicit tooltips for unavailable placements.
- Drag preview ghost shows legal/illegal tile outlines; deterministic highlight of connected inputs/outputs, and small predicted resource count / score estimate. Explain that estimates can differ due to queue and capacities; use exact deterministic preview when affordable.
- Activate button high salience, blocked while invalid only with reason. No double activation grants. During animation, `Skip` still computes identical results.
- Haptics/audio satisfying but nonessential: one placement tick, one connection success, rising arpeggio for cascades, low note for bottleneck, celebratory chord for beating target. Mute options independent.
- Camera static top-down; game board itself can gently pan within UI container if >6x6; do not zoom so small that nodes become unreadable. HUD safe-area aware.

# C09. Screen-by-screen production specifications
**SC01 Splash/Title**: logo, flat animated skyline silhouette, Continue/New Play, no forced account. First opening goes directly into animated board in ≤5s on reasonable device.
**SC02 Guided first board**: 4x4 top-down, 2 active pieces and highlighted intended tile; progress 1/3; contextual drag + rotate and Activate. Remove tutorial steps after success, re-open Help anytime.
**SC03 Main board**: objectives, round timer if mode uses action deadline (no clock for default puzzle), money + optional premium currency, grid, connected edges, tray, rotate, preview, Activate, pause, undo, combo ribbon. Largest screen; no permanent huge advertisements.
**SC04 Results**: score, profit sources breakdown, first/second/third stars, one helpful tip, earned soft currency, challenge complete, Next/Retry/Edit; no paywall after failure.
**SC05 District Map**: attractive flat 2D overhead progression islands/blocks (not 2.5D), 6 districts, percent completion, next contract icon and unlock requirements; visual changes earned via completion.
**SC06 Challenge Select**: 12 chapters cards per district, objectives, seeded modifiers, earned stars, rewards, preview, start.
**SC07 Piece Collection**: grouped catalog, 24 pieces, unlock progress, mechanic rules, synergy tags, pin favorite, change nonfunctional skin.
**SC08 Combo Encyclopedia**: documented combos, discovered badges, illustrated before/after diagram and replay.
**SC09 Blueprint Management**: save/load owned layouts with name, district compatibility, cost, piece legality, thumbnail. 2 free slots, expandable earned slots; optional cosmetic/utility sold only if meaningful.
**SC10 Contract Board**: three active contracts, estimated difficulty, seed and duration, auto-resolve only limited verified saved-layout contract; client cannot claim repeating reward through app restart.
**SC11 Daily Challenge**: seeded identical logical puzzle, no global online leaderboard in v1, personal streak tracked without punishment, replay allowed, reward earned once.
**SC12 Empire Hub**: progression trophy map; building completion animations are lightweight, no free-floating 3D buildings. Display cashflow score history and next district. Active round remains main fun.
**SC13 Event Board**: opt-in curated multi-level event with published reward map; no surprise paid gate, no fake deadlines. Remote-disabled if content unready.
**SC14 Shop**: platform-local currency prices, clear content, disable unavailable, restore purchases, “no purchase needed” acknowledgment, not mixed with in-game score currency.
**SC15 Offer Details**: SKU preview with exact owned items, direct benefits, confirm via platform sheet, decline obvious and unpenalized.
**SC16 Inbox/Return Report**: offline contracts, recent unlocks, saved layout results and new objectives; no giant intrusive daily modal.
**SC17 Settings/Help**: sound, music, vibration, reduced-motion, subtitles/captions, privacy, support, restore purchases, version, tutorial replay, save management.
**SC18 Purchase/Error**: loading/cancel/deferred/unavailable/success/refund/restoration, never invent confirmation. **SC19 Legal**: privacy, terms, licenses, support. **SC20 Debug/admin**: developer only, simulator inspector, grant test currency only outside release build.

For every screen Codex must produce a screen-state matrix: empty, loading, success, error, offline, long localized strings, small-screen, owned/locked. Never bake final UI text into raster backgrounds.

# C10. Installation-to-year player journey
| Time / tenure | What the player does | Reward and new capability | Future reason |
|---|---|---|---|
| Opening 0–15s | See 2 connected pieces | Animated pulse and score | Change layout |
| 1 min | Solve first 4x4 | Free basic piece | Beat next target |
| 3 min | Solve 3 mini boards | First combo and starter neighborhood | Try higher multiplier |
| 10 min | Learn warehouse routing | New business family | Fix bottleneck |
| 15 min | Choose first draft | First meaningful strategic tradeoff | Try alternative build |
| 30 min | Finish district starter circuit | First district visual transformation | Next district rules |
| 1 hour | Two districts, start collection | Save first custom blueprint | Improve personal best |
| 3 hours | Multiple market modifiers | 2-3 viable build archetypes | Advanced contract |
| Next day | Return contract resolves | Report + new modifier | Improve design |
| Day 2 | Reach commercial family | Introduce BANK/office constraints | New combo |
| Day 3 | Second collection set | New schematic/visual collection | Finish set |
| Week 1 | Industrial family | Factory and power constraints | Scale chains |
| Week 2 | Logistics contracts | More distinct strategies | Mix production lines |
| Week 3 | Luxury/retail events | New modifiers + cosmetic goals | Beat themed challenge |
| Month 1 | Several districts completed | Expert scenarios and completion trophies | Improve mastery |
| Month 2–3 | Daily seeded/advanced contracts | New challenge sets by versioned content | More varied experiments |
| Beyond | Optional paid/free content expansion | New board conditions and family mechanics | New strategic possibilities |

Cadence is illustrative for engaged players. Never silently gate gameplay by calendar; a highly engaged player may progress faster.

# C11. Persistent economy and progression
Currencies: `cityCredits` (earned, non-purchasable base score/progression money unless purchase explicit), `sparkGems` (optional premium credits), `blueprintTickets` (earned progress or sold in clear packs only if no random prize). Avoid three separate confusing economies at v1 unless each is justified. Recommended actual launch: **cityCredits + one optional premium currency**, and blueprint slots as entitlement counts, not third currency.

Rewards use a monotonic `DistrictXP` based on challenge completion and objective diversity, not raw credits produced indefinitely, to avoid optimal farming exploit. Replay rewards decay for same challenge after first completion but never block replay; objective diversity bonus encourages novel play. First-clear grants are deterministic and idempotent.

Starting balances (tunable): 350 cityCredits, 0 premium gems; first 3 challenges free pre-selected pieces. A new piece unlock costs 1–4k earned cityCredits, gained through first-clear rewards, not real-time hourly idle wait. District unlock earned through clear count + one named combo, not purchases.

Free blueprint slots 2, more slots can be earned from district completion; optional purchase expands convenience but never removes free access to saved layouts. Cosmetics affect color/particle effects only. Content packs add *substantial* extra optional challenges; don't gate the sole next progression chapter.

# C12. Cashflow store: product inventory and exact boundaries
All USD below are **test-market hypotheses**, display localized store prices from platform catalog.
| Product ID | Type | Suggested tier | Fulfillment | Value and constraint |
|---|---|---|---|---|
| `cc.start.01` | nonconsumable starter one-time | $1.99 | 500 city credits + two unique flat skins + permanent founder badge | no special power; if credit bundled, no refund exploit |
| `cc.gems.small` | consumable | $0.99 | 90 gems | repeatable; transparent |
| `cc.gems.medium` | consumable | $4.99 | 520 gems | no fictitious discount |
| `cc.gems.large` | consumable | $9.99 | 1,150 gems | test with spending caps/support |
| `cc.reroll.5` | consumable | $1.99 | five optional rerolls in drafting mode | never required for mandatory first-clear |
| `cc.blueprint.4` | nonconsumable | $3.99 | four extra save slots | check ownership before sale |
| `cc.skin.neon` | nonconsumable | $4.99 | 8 coordinated flat neon building skins, profile badges | no stats |
| `cc.scenario.harbor` | nonconsumable | $6.99 | 12 new authored challenges, 2 modifiers, extra art set | launch only after content tested |
| `cc.scenario.finance` | nonconsumable | $9.99 | 20 expert challenges & one new optional business family | no first month requirement |
| `cc.pass.s1` | optional later | $6.99 | published exact free/premium tiers | **disabled for v1**, only if ongoing production proves feasible |

Shop eligibility: after first clear (store icon may appear), purchase suggestion after ~10–15 min *and* player discovers desired utility (cannot hard block). Extra blueprint offer only after both free slots used. Cosmetic collection offer after first district finished. Repeatable rerolls only in drafting levels, capped per run, not while confused beginner. Content expansions appear after relevant base content milestones. No fake limited timers; if an actual promotional price ends, use genuine server/app-store terms and disclose transparently.

All monetized products have analytics for impression -> product detail -> platform sheet -> success -> usage -> D7 retention. Monthly paid pass only after 6 weeks of reliable cadence and observed interest, not merely because it appears in mockups.

# C13. Difficulty, soft currency and balancing formulas
- Base profit per building: `realizedOperations * recipeCredits`, with integer rounding. Multipliers applied once at sink, basis-point math, max `3.0x` per event, `8.0x` round bonus cap. Never compound bank multipliers on same event.
- **Challenge target** derive from simulated reference solution `scoreBenchmark` × target factor 0.70–0.85 (intro), 0.85–0.95 (mid), 0.95–1.05 (expert with multiple viable strategies). Generator must prove at least one legitimate solution; `scoreBenchmark` discovered via bounded solver/heuristics and verified human-playable.
- Reward = `firstClearCredits + stars*smallBonus + diversityBonus`; soft currency rewarded for successful challenge, not endlessly proportional to raw score; replay offers capped smaller bonus per unique seed/day. No arbitrary 24-hour wait to continue.
- Scoring leaderboard is **personal best only** initially. If online ranking introduced, require server-authoritative verification and separate paid consumables challenge rules.
- Daily challenge chooses from certified seed pool keyed by UTC date; no payout variance by location or paid status. Offline day rollover using conservative trusted fallback; no exploit from clock manipulation.

# C14. Content creation tools Codex must build
1. `PieceEditor` with rule validation and diagram preview, shows throughput and ports; editable via inspector or JSON.
2. `ScenarioEditor` that locks forbidden cells, sets draft pools and modifiers, validates budget/reachability.
3. `ChainInspector` visual debug trace showing which resource created each profit pulse.
4. `EconomyBatchHarness`: 10k seeded boards/challenges, flags infinite loop, overflow, negative profit, impossible objective, trivial solution, one dominant piece.
5. `ScreenshotBot`: capture all essential screens, startup to advanced, at target portrait aspect sizes, output QA atlas.
6. `OfferSandbox`: fake store SDK implementing pending/failure/restore with actual UI.
7. `LocalizationValidator`: detect truncation, missing keys, plural formatting and prices not matching store-returned strings.

# C15. Game-specific events for telemetry
`board_open`, `piece_selected`, `piece_placed`, `piece_rotated`, `layout_undo`, `chain_preview`, `activation_started`, `activation_completed`, `combo_discovered`, `bottleneck_viewed`, `challenge_failed`, `challenge_passed`, `stars_earned`, `blueprint_saved`, `blueprint_loaded`, `district_unlocked`, `contract_sent`, `contract_returned`, `market_modifier_seen`, `piece_collection_viewed`, `first_paid_reroll_used`. Measure % who trigger >1 unique combo within first 15min, average voluntary retries, ratio of distinct layouts to attempts, number of purchases followed by continued play.

# C16. Art bible and asset manifest
**Style name:** `Electric Ledger`: overhead 2D neon-circuit city toyboard with polished minimal vector building tokens, hand-painted-flat city textures and luminous routing. Crisp silhouette and icon at 40–80 px; elegant noir-navy backgrounds; mint/yellow cash paths, copper/gold success flare, purple event modifiers. Distinct from realistic finance dashboards and from isometric city builders.

Suggested palette: navy #071B2A; blue #143D59; mint #27EDBD; lime #B8F268; amber #FFBE54; magenta #E657C3; cream #F6F0DA; danger coral #FF706A. Text UI theme uses approved readable licensed font with 2 weights, numeral variant; document source/licensing in attribution.

Ship assets v1: 24 flat building icon sprites each 128x128 original source + 2 optional recolor variants, 8 pipe/connection shapes + animated glow mask, 6 source token icons, 6 district backgrounds (flat abstract silhouettes), 12 challenge cards, 28 UI icons, 8 buttons/frames (9-slice), 10 particle prefabs/shader effects, 4 congratulation banners, 2 scalable tutorial finger animations, 6 music loops/stems (original) and ~22 original procedural sound cues. Use atlas separation UI core / buildings / district decorative. Limit atlas draw calls and contrast. UI text never baked into game-art assets.

**Visual acceptance example:** Compare screenshot on small Android phone to approved source: 6x6 board fully visible without zoom-caused unreadable numbers, selected tile clear, connection arrows not mistaken for streets, input/output colors differentiate, result clearly corresponds to actual score, no perspective. Use a reduced-motion screenshot as well.

# C17. Audio and feel spec
Short spatial soft chime on place, clean click on rotate, two-tone success on valid connection, ascending 5-note motif on greater chain profit, rounded thud on blockage, gentle gold shimmer on district completion, optional 8-beat musical evolution synced to resolution. Background is mellow electronic jazz/neo-soul authored via sequence/synthesis, not samples taken from commercial recordings. Duck music on important feedback; safe levels, accessibility control and no nonstop reward-noise.

# C18. Deterministic testing and exploits (game-specific)
- Board identical seed + commands yields byte-for-byte identical domain event logs.
- Sum of consumed goods ≤ generated+initial goods; demand ≤ housing and other legal sources; no negative inventory.
- Rotating piece updates ports and edge graph on same frame, even after undo; no phantom flow.
- 4-way corners, blocked tile, boundary and empty input: deterministic neutral outcomes.
- Bank bonus applies **once** and respects cap; looped routes cannot create credits without resources.
- Negative buy/sell exploit, score overflow, 2^63 boundary, NaN visual value, multiple activation fast taps, resume mid-animation, skip animation must not duplicate money.
- Challenge 3-star objective should always be achievable with allowed non-premium pieces; test 1000 seeds for viable solutions and stage test 72 authored challenges.
- Player cannot purchase blueprint slots twice for extra hidden grants if nonconsumable; restore correct after reinstall.
- Large active event or 7x7 level memory remains within target device; 6x6 must be responsive and battery-friendly.

# C19. Launch-ready v1 exact ship inventory
**Ship:** 72 base challenges in six districts, 24 business pieces, 12 market modifiers, 15 contract templates, 30 achievements, 8 demoable combos, 2 free blueprint slots + earned capacity, 6 cosmetic packs, core result + progression screens, accurate offline contract catch-up, optional one-time/consumable non-random IAP catalog subset (no subscription), analytics, backup/restore, privacy/quality/regional metadata and actual screenshots. **Later only:** global ladders, PvP, procedural every-day unlimited events, player-designed public levels, guilds, huge realistic economy, deep characters, endless prestige chains, seasons requiring constant content teams.

Marketing evidence: real Unity screen video shows first placement -> cyan routed goods -> profits cascading -> one changed piece doubles score; then progression district unlock. If actual Unity can't communicate this within 8 seconds, revisit art/animation rather than create deceptive video.

# C20. Milestones and work packages for Codex
`CC-P00` repo creation, Unity 2D settings, pinned packages, CI smoke build, deterministic seed + save shell.
`CC-P01` flat board/grid, touch UX, spawn piece, rotate, input, undo; unit tests.
`CC-P02` token flow graph, eight-beat deterministic engine, queue and score oracle, 10k property tests.
`CC-P03` on-screen chain timeline and original audio/FX, screen capture and readability gate.
`CC-P04` 4x4 tutorial, first 15 minutes with 10 pieces, 12 curated intro levels; human feasibility test.
`CC-P05` progression district hub, unlock tree, catalog, blueprints, contracts, saves.
`CC-P06` build full 72 challenges, validated 12 modifiers, generator and debugging editor.
`CC-P07` QA original art/audio polish, responsive UI, all 20 screens and accessibility.
`CC-P08` Unity IAP v5 adapter, verified entitlements, store catalog, telemetry, optional reward ad feature gated.
`CC-P09` localization, balance simulation, device testing, full purchase matrix, Play closed test, privacy and store assets.
`CC-P10` produce signed AAB build candidate, release notes, staged rollout and operational playbook; **external approval required**.

**Stop/go gates** after P02: graph invariant and no infinite score; after P04: testers voluntarily replay, understand adjacency; after P06: no dominant single strategy and levels all solvable; after P08: purchase and refund matrix; after P09: Android build + policy acceptance.

# C21. Definition of done and first Codex prompt
No game is "100% complete" merely because it compiles. Done = all 72 accessible and solvable, 20 screens operational with states, functional IAP, all licensed assets original, responsive accessibility, no save corruption or currency duplication, localization functional, min supported device tests, privacy labels accurate, a verified store build, account reviews and tester gates passed, no P0/P1 defects, every requirement mapped to a test.

**Use this kickoff prompt:**
> Build CASHFLOW CHAINS according to `01_CASHFLOW_CHAINS_MASTER_SPEC.md` and shared technical/workflow/release documents. First create `Docs/IMPLEMENTATION_PLAN.md`, `Docs/RISKS.md` and traceability matrix keyed CC-P00 through CC-P10. Implement only P00-P02, with a compilable Unity project, fully deterministic 8-beat engine and automated conservation and anti-cycle tests. Create an actual flat 2D gameplay capture of the gray-box board. Do not build paid products or claim visual quality until I approve the play slice. Report exact tests and evidence at the end of each package.
