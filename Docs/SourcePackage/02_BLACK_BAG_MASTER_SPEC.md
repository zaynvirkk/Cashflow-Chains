# BLACK BAG
## FULL UNITY/CODEX GAME PRODUCTION SPECIFICATION
**Version 1.0 - 20 Oct 2026 - designed for implementation, subject to commercial and human playtesting**

**Read first:** `00_START_HERE.md`, `03_SHARED_TECH_MONETIZATION_LAUNCH.md`, `04_CODEX_AGENTS_AND_WORKFLOW.md`, `05_RELEASE_AND_TEST_MATRIX.md`.

# B01. One-sentence promise, core differentiation, fantasy
**Steal the most valuable haul you can fit inside a small bag, decide when the risk is worth it, escape with the treasure, and build a secret collection of extraordinary relics.** This is a tactile **polyomino packing + push-your-luck** puzzle in a seductive 2D museum-heist fantasy. Core fun = rearrangement and satisfying fits + greed versus security; persistent fantasy = collector of rare objects with smarter loadouts and harder vaults.

**Show, don't tell (real ad/first session):** rotate and slot a big art treasure into the last available cells; discover a $100,000 diamond but taking it raises the alarm; escape with a spectacular haul, display a rare relic in a flat 2D hideout gallery. Most objects are **one permanent set of original sprites reused** under different arrangements; no detailed stealth platformer, open-world, realistic criminal simulation, 3D museum or human combat.

Genre: portrait mobile casual spatial puzzle, free-to-play with IAP-first optional products and earnable gadget supply. Single-player complete offline heists, internet only for store/optional services. Short rounds 1-4 minutes, expert missions 5-9 minutes; default security advances by **actions**, never by real-world time. A play/pause-friendly commute mode is essential.

# B02. Pure 2D artistic rules
- Render case grid, loot silhouettes and UI **front-on, flat, orthographic**. No raised-edge 3D geometry, rotated perspective bag, photorealistic simulated depth, 3D props, character 3D animation, or isometric scene. Richness comes from original flat illustrated profiles, precise brass filigree, dark wine velvet texture, parchment typography, animated ink reveals and subtle glow. Everything remains a flat layered 2D sprite or vector drawing.
- Both UI identity and material identity are visible: velvet crimson #2B0610, ruby #8E1C32, antique gold #DAB66D, pitch black #09090B, cream #F2E5CD, blue gem #67B5E8, safe teal #4FD9C6, alert #F05B66.
- Atmospheric event locations can show **flat editorial background illustrations** behind the grid (museum walls and silhouettes), but not imply stealth movement or promise a level that isn't playable.
- The carrying case is a clean rectangular grid, not an image of mixed 3D objects. Every loot art bounding box exactly matches a legal polyomino mask. No image may overlap blocked cells while engine believes they are free.

# B03. Core flow and game states
`Splash -> Intro Heist or Hideout -> ContractBoard -> ChooseContract -> ScoutingBrief -> Loadout -> Vault -> InspectLoot -> Drag/Rotate/Rearrange -> Take/Leave/Unlock -> Extraction -> Result -> RewardGrant -> Hideout/Replay/NextContract`.

Gameplay consists of repeating **intel -> pack -> decide -> escape**. Vault exposes a set of items, a bag grid and a discrete security track. Items have visible shapes, values, rarity, set tags, fragility, mass and risk cost. Player may pack items in any order and rotate/move them freely within a revealed loot selection; **mere repositioning costs no alarm**. Alarm increases only on committed `TakeItem`, `OpenSection`, `TriggerExtraAttempt`, or clearly labeled special actions. This distinction preserves tactile experimentation without punishing phone interruptions. Choosing `Escape Now` resolves with the secured haul and no hidden roll unless a clear risk check was incurred by an earlier action.

No gameplay clock ticking while the app is backgrounded or paused. An alert can have dramatic blinking animation, but the numeric state advances only on committed actions. If a particular special timed challenge is introduced later, it must be optional, named and clearly disclosed before start.

# B04. Inventory packing rules - authoritative model
Grid sizes: tutorial 4 columns x 5 rows, starter 5x6, standard **7x8**, advanced special 8x8 **only if fit/legibility gate passes**. Cells indexed x in [0,W-1], y in [0,H-1]. Grid holds 0 or one item ID per cell; fixed blocked cells are `-1` and never editable. Coordinate origin upper left (consistent drawing and gestures). Items are connected orthogonal **polyominoes** defined as an ordered set of unit offsets `(dx,dy)`, e.g. 1x1, 1x2, L-tromino, 2x2, T-tetromino, 1x3, S-tetromino, 2x3 rectangle, 4-cell hook.

### Shape transform and placement invariants
- Default pivot is top-left of unrotated canonical normalized bounding box. Rotation 0/90/180/270 uses integer coordinates and **normalizes minX/minY to zero** after transform. Rotate matrix for clockwise: `(x,y)->(-y,x)` then shift to nonnegative. Preview mask must match engine mask exactly.
- Legal placement: for all rotated offsets `x=origin.x+dx`, `y=origin.y+dy`, inside rectangle, not blocked, not occupied by **other** item. If moving item already on board, temporarily treat its original cells empty during validation; commit only if new layout valid, otherwise snap back with one readable warning.
- One item identity can be either available, selected/previewed, secured-in-bag, stashed/escaped, sold, or archived in collection; it cannot be both loose and secured. Position/orientation stored alongside unique `instanceId` and `definitionId`.
- No pixel-perfect physics or free angle rotation. Snap on release to nearest grid cell; preview uses correct `RectTransform` pivot and safe-area transform. Display precise cells/highlight independent of art silhouette. Provide one-handed `Tap item -> Rotate button -> Tap destination` alternative to drag.
- Items don't stack, overlap, clip outside grid, or secretly use fewer cells than visible art. Drop outside board cancels action, does not destroy loot. Never grant rewards based only on sprite overlap.

### Packing/weight/fragility rules
Base bag has 5x6 cells + weight 14 units; standard bag unlocks 7x8 cells + weight 24. Upgrade capacity gradual and earned; **avoid a giant purchasable bag that makes every puzzle trivial**. Each loot item has 1..8 cells, weight 1..10, value 100..100000 depending on tier, optional tags `fragile`, `metal`, `uniqueArtifact`, `collectionSet`.

Bag constraints evaluated together: `occupiedCells ≤ usableCells` AND `totalWeight ≤ bagWeightCap` AND `toolSlots ≤ ownedSlots`. Some bag variants change shape/blockers rather than universally increasing both limits: `Flat Folio` longer but narrow, `Reinforced Case` safe for fragile, `Traveler` standard shape but extra tool slot, `Diplomat` larger interior with reduced weight limit. Every bag class is earnable through play; cosmetic versions may be sold. Advanced upgrades are optional convenience.

Fragility: Optional `Fragile` items require `protection` tag for maximum payout; without protection sell price = 70% base (clearly shown **before** player commits). Never destroy rare permanent artifact outright due to a hidden roll. Keep rules simple and disclosed. Set bonuses and sale value separate, no mysterious multipliers.

# B05. Vault and security rules
Security track default 0..6 with initial 1 or 2 based on contract. Every visible action has deterministic `alertCost` 0..2. `TakeItem`: normally +1, rare secured case +2. `OpenSection`: +1 or +2 and reveals 1–3 treasure choices; `UseDistraction`: reduce alert by 1 once/run at tool cost; `Inspect`: free default for basic info, optional scanner reveals hidden tags without increasing alert. Rearranging, hovering or rotating never adds alert.

**Alert resolution:** At each committed action, if `newAlert <= contractAlertThreshold` item/section acquired and session continues. If `newAlert > threshold`, security triggers and the player may lose the **unextracted session reward only**, by a clearly stated percentage/penalty such as 50–100%, with partial extraction insurance granted by earned skill/tool. In standard difficulty prefer an explicit last warning (`one action remains`) before catastrophic failure. No premium purchase popup after catching player at failure; Retry available free.

Use discrete alert hazards (`guards`, `camera`, `pressureAlarm`, `extraLock`) as thematic modifiers with transparent costs: guards means `OpenSection` +1 alert, cameras mean every third Take +1, pressure alarm makes large item +1, extra lock costs one `gadget` or +2 actions. No simulated patrol physics or real burglary procedures; all are abstract fictional puzzle rules. A `HazardInfoPanel` must explain it before commitment.

**Extraction:** Tap `Escape Now`, game checks what is physically in the bag, applies declared fragile modifiers, contract bonuses, rarity/collection rewards, grants once via ledger, publishes score and displays gallery items selected for preservation. Player can escape with zero loot but gets no or small consolation reward. Escape cannot happen automatically due to inactivity.

# B06. Commit / rearrange interaction design (critical)
The game must separate **preview**, **secured inventory**, and **commit cost**, resolving a potential ambiguity in initial sketches.

Proposed polished UX for v1:
1. At scene entry 3–6 objects are visible in `Vault Shelf` outside bag. Bag starts empty or prefilled in tutorial.
2. Drag an object onto grid freely to preview a packing plan; while hovering, it is **not owned**. Tap `Take` or `Confirm Load` to commit that object into bag; game increments alarm according to visible cost. This avoids penalizing the puzzle-solving process with alarm on every reposition.
3. Once committed, moving or rotating a secured object inside bag is free. Returning a committed object to shelf is **not** a free refund of alert cost; it becomes `Dropped` and can be retaken only by paying new visible cost. Tutorial explains this before allowing the action.
4. A single `Take All Planned` option can commit a legal set together but shows summed risk before confirmation and processes sequentially according to item IDs. The basic flow remains readable for a one-minute round.
5. A `Rearrange` mode highlights safe legal moves; empty case preview allows quickly rotate any item.
6. Security cannot rise merely from touching screens; fast repeated taps debounce, transaction reward idempotent.

**Design alternative subject to playtesting:** choose “commit on successful drop” instead of separate Take confirmation to reduce friction. If implemented, displayed red alarm cost preview precedes release; prototype should A/B usability but keep deterministic state. Launch one canonical model and write tests accordingly.

# B07. Core valuation and mastery
Raw haul = sum of secured item values with declared wear/protection adjustments, added single-mission contract bonus, and first-time collection discovery bonus. `Score` = collected haul points + explicit risk-efficiency rating. Do not multiply values for each action repeatedly. Rare item value can be very high, but the bag's size and weight impose opportunity cost.

Star ratings:
- 1 star: safely extract with contract minimum value or specified object.
- 2 stars: meet value and keep alert below threshold or fulfill optional shape challenge.
- 3 stars: additional objective, e.g. 2 set items, no tools, or >=80% weight efficiency; no purchased tools may be required for 1 star.

Players improve by correctly arranging shapes, choosing sets rather than only highest displayed price, mastering security budget, and building compatible loadouts. A replay using same seed has the same loot arrangement and rule outcomes. Optional random contracts use deterministic seeded catalog plus hazard templates, certified solvable by automated placer for minimum objective.

# B08. Loot catalogue and generation
Launch inventory scope: **48 unique definition sprites** spanning 6 collectible categories: jewelry (8), historical objects (8), art/scrolls (8), gadgets/tech (8), gold/coins (8), rare artifacts (8). Each category supports ~4 shape templates reused across distinct art. A valuable **collection piece has a set ID, not loot-box rarity driven by a purchase**. Begin with curated tutorial loot and optional seeded vault tables.

Example definitions (illustrative and tunable):
| Loot ID | Shape offsets | Weight | Base value | Properties |
|---|---|---|---|---|
| `cash_roll` | [(0,0),(1,0)] | 1 | 500 | common |
| `gold_bar` | [(0,0),(1,0),(2,0)] | 5 | 3000 | metal |
| `pocket_watch` | [(0,0)] | 1 | 1200 | masquerade set |
| `ruby_ring` | [(0,0)] | 1 | 4200 | rare jewelry |
| `paint_frame` | [(0,0),(1,0),(2,0),(0,1),(1,1),(2,1)] | 4 | 9000 | fragile, art |
| `ceramic_vase` | [(0,0),(0,1),(1,1)] | 3 | 6600 | fragile |
| `royal_crown` | [(0,0),(1,0),(0,1),(1,1)] | 5 | 16000 | imperial set |
| `imperial_egg` | [(0,0),(1,0),(0,1),(1,1),(0,2),(1,2)] | 6 | 26000 | imperial set, unique |
| `gem_case` | [(0,0),(1,0),(2,0),(1,1)] | 4 | 12000 | jewelry, rare |
| `ancient_tablet` | [(0,0),(1,0),(2,0),(1,1)] | 4 | 7000 | archaeological |
| `ivory_chessbox` | [(0,0),(1,0),(1,1)] | 3 | 4800 | collectible |
| `obsidian_mask` | [(0,0),(0,1),(1,1),(2,1)] | 2 | 8500 | forbidden set |

For culturally sensitive antiques, use completely fictional stylized lore, do not falsely market real protected heritage items or replicate trademarks.

Generation rules: tier budgets control available item count, value distribution and shape mix; seed plus `vaultTemplateId`. Generate guaranteed primary target if contract is curated. Independent packing solver verifies reachable minimum score at standard earnable bag capacity, with visible alert budget. For any generated mission, don't offer an objective impossible with no paid upgrades. Randomness used only for free generated loot; purchases offer defined goods, no paid randomized chests.

# B09. Bag and tool catalogue
**Earnable loadouts:** Default Satchel, Flat Folio, Reinforced Case, Collector Trunk (high cells but lower carry weight), Courier Bag (lighter, tool flexibility). Player selects exactly one per mission, with strategic tradeoffs (not monotonic infinite capacity).

**Earnable tools:**
- `Foam Case`: protect one fragile item in active vault.
- `Scanner`: reveal exact shape/value/tags of one concealed item, no instant victory.
- `Diversion Token`: reduce alarm 1 once per vault, clear use count.
- `Pivot Kit`: legal rotate/rearrange convenience, not essential (rotation free without it; kit may permit 1 two-item swap in special blocked-grid mode).
- `Ledger`: indicates set progress before deciding to take a collectible.
- `Secure Seal`: raises insured payout floor from 0 to e.g. 30% if alerted, once/run; must not encourage paid bailout prompts after losing.

Loadout = bag choice + max 2 earned tool slots in starter, 3 later, tool consumables limited **per mission** and earnable at normal cadence. Do not include guns, suppression devices, real physical bypass instructions or realistic crime tactics from previous glossy mockups; v1 is a fictional packing strategy game.

# B10. Contract types and mission variety
- `Value Run`: reach required haul.
- `Set Retrieval`: find at least 2 pieces from named set.
- `Fragile Freight`: value fragile items with protective strategy.
- `Heavy Cargo`: scarce weight, generous space.
- `Tight Suitcase`: several blocked cells, rotation mastery.
- `Low Alarm`: few committed Takes, favors precise selection.
- `Curator's Choice`: preserve one historically valuable fictional relic; direct target visible.
- `Split Objectives`: choose whether to take a high-value item or complete multi-piece set.
- `Luxury Gala`: rare items but extra alert for protected treasures.
- `Mixed Vault`: varied loot and visibility with two possible routes.

V1 **6 fictional locations x 12 contracts each = 72**, like different flat 2D backgrounds + icon treatment, but rules driven by constraints not expensive 3D environments. Locations: Private Study (tutorial), Antique Auction House, Art Gallery, Imperial Museum, Grand Hotel Vault, Collector's Archive. Each has 12 missions with 8 curated first-tier, 4 certified seed variants. Same grid assets reused across all venues.

Later expansions may introduce one new mechanic such as dual-compartment case or sealed items; **not** a fully controllable stealth game.

# B11. Scene UX: exact screen and states
**BB01 Boot/Intro**: 2D black/red/gold silhouette background, one tap `Play`, automatic anonymous local profile and speed-to-first-item, no signup or videos.
**BB02 Tutorial Vault**: 4x5 case, 3 valuables and two empty slots, tutorial tooltip `drag`, `tap rotate`, `escape`; first successful legal pack ≤35sec; use simple 1x2 and L items first; no tricky security yet.
**BB03 Standard Vault**: contract header top, grid center-left or full center depending device, shelf scroll, current haul, weight/cell count, alert pips, hazards, contextual selected item panel, easy Rotate, Take, Drop, Inspect, Escape. Visual priority = available remaining cells and risk.
**BB04 Item Details**: flat object sprite, exact footprint, weight, value, set, fragile cost, alert cost, price after protection. Modal non-blocking.
**BB05 Loadout Select**: earned bag types with precise capacity diagram, weight limit, tool slots, tradeoffs, tool quantity and Equip.
**BB06 Mission Preview**: objectives, set goal, default alert max, vault modifier, three clear stars, earned/free bag compatibility, Start.
**BB07 Heist Results**: secure/failed, actual loot list, deductions explained, preserved unique discoveries, earned currency, set progress, Replay/Next/Hideout; no “pay to restore loot” popup.
**BB08 Contract Map**: 6 illustrated flat-2D venues, progress, contract count, fog-of-war on unrevealed only (not loot purchase uncertainty), next venue.
**BB09 Hideout**: flat 2D gallery illustration, collected artifacts visible as flat card silhouettes and display slots, upgrade decor earned. Not a walkable 3D room.
**BB10 Collection Book**: 10 named sets in v1, completion and missing placeholders, collection rewards, inspect full-screen original object art.
**BB11 Equipment**: owned bags/gadgets, level/enhancement only where real distinct utility, tool earn routes, save 2 free loadouts.
**BB12 Scouting/Return Report**: optional offline 1–2 investigations to reveal next **existing curated** contract modifier/loot category. Rewards once, cap elapsed time, no irreversible fail while offline.
**BB13 Gallery Improvements**: choose flat wallpaper, case trim, shelf layout, title plaques, cosmetic selection; all original art, no intrusive construction timers.
**BB14 Missions/Challenges**: 1-star through 3-star, no-gadget challenge, shape puzzle and limited alert challenge, optional personal best.
**BB15 Daily Vault**: certified deterministic seed, clear conditions; may skip with zero penalty. No global competitive advantage from purchased tools.
**BB16 Event Contract**: themed vault set with original object art and explicit rewards; feature gated until content ready.
**BB17 Shop**: SKU cards with platform prices and previews, small/medium packs, permanent loadout slots, cosmetic galleries, expansions, purchase/restore.
**BB18 Offer/Product Detail**: exact contents, duration, eligibility, obvious close, no fake countdowns.
**BB19 Settings**: controls, music, haptics, accessibility, privacy, support, restore purchases, credits/version, tutorial replay.
**BB20 Loading/Error**: offline, interrupted purchase, deferred family approval, save conflict, malformed content, no receipt duplicate.
**BB21 Developer Editor**: generator seed/shape visualizer, bag solver, mission validator, grant simulated items in dev-only build.
**BB22 Legal/Support**: ratings, privacy, consent, terms, store restoration, help.

Each screen requires localization-safe layout, tablet/safe-area QA, accessibility, offline handling and state screenshots. Because images from concept renders contain illegible microtext, recreate typesetting natively, not raster screenshot slicing.

# B12. Installation-to-months player journey
| Milestone | Concrete activity | Personal reward | Next curiosity |
|---|---|---|---|
| Open 0–15s | Case opens, tutorial shows cash and watch | First successful drag | Where else does it fit? |
| 1 minute | Pack 2–3 objects and escape | First haul, simple gold flash | Second vault with L-shape |
| 3 minutes | Choose extra jewel or leave | Risk outcome under own control | Try better extraction |
| 10 minutes | New bag shape and first fragile piece | Understand equipment strategy | Protect painting |
| 15 minutes | 3 vaults + first set item | Collection panel reveals 1/3 set | Complete set |
| 30 minutes | 6–8 vaults and next venue preview | First gallery display | New hazards |
| 1 hour | First alternate bag and real tradeoff | New loadout route | High value vault |
| 3 hours | Special contracts, 2–3 sets | Hideout visibly filled | Expert challenges |
| Next day | Scout return report | New unlocked contract intel | Attempt target vault |
| Day 2 | Antique Auction House | New item set and risk | Complete auction series |
| Day 3 | Special bag earned | Alternative packing strategy | Retry old high-score |
| Week 1 | Art Gallery contracts | Bigger but awkward items | Skill vaults |
| Week 2 | Imperial Museum | Intricate polyomino objectives | Legendary collection piece |
| Week 3 | Grand Hotel Vault | Mixed shape/weight and pressure | Different bag loadout |
| Month 1 | Collector's Archive unlocked | Gallery collection completion | Mastery challenge |
| Beyond | Optional authored expansion | New relic mechanics/venues | More collections without 3D |

Timings approximate; no hard calendar locks and no promise that all players progress at same pace. An engaged user can play uninterrupted for hours; if all reward-bearing quests finish early, certified endless challenge mode remains available.

# B13. Return systems and long-term attachment
The player's hideout displays **earned** heirloom collections. At most 10 sets with 3–5 objects each at launch; each set tells a one-paragraph fictional provenance story, gives cosmetic showcase decoration and one free meaningful game item upon completion. Collections are not paid chance boxes; collection item availability tied to earnable challenge progression, not randomized paid pulls.

`Scouting`: player assigns scout to a location for a 2–6 hour elapsed-time reveal; results reveal a future contract modifier and one item family. If user doesn't return, intel stays available. Scouting is informational/bonus, not needed to unlock normal missions. No adversarial raids on player's hideout while away.

Ranked mastery is **offline personal**: best haul-per-alert, highest % packing efficiency, best no-tool runs. No need multiplayer matchmaking/anti-cheat rankings. A progression pass is a future option only if mission content cadence exists.

# B14. Store catalog and business model
Starting price levels are **hypotheses**; actual prices localized via store metadata, not hard-coded.
| Product ID | Type | Proposed tier | What players get | Availability |
|---|---|---|---|---|
| `bb.start.01` | one-time | $1.99 | one permanent velvet case style, 2 earned-compatible basic scanners, small coins | available after first full heist |
| `bb.coins.small` | consumable | $0.99 | fixed amount clear in UI | repeatable, no necessity |
| `bb.tools.3` | consumable | $1.99 | 3 declared non-random scans/diversions | after gadget tutorial |
| `bb.tools.10` | consumable | $4.99 | 10 declared non-random charges | optional |
| `bb.loadouts.3` | nonconsumable | $3.99 | +3 named saved loadouts | after free 2 used |
| `bb.cosmetic.noir` | nonconsumable | $4.99 | 4 flat case skins + brass trim + 3 backgrounds | cosmetic only |
| `bb.gallery.gilded` | nonconsumable | $5.99 | 3 flat illustrated hideout themes; no stat buff | after gallery unlocked |
| `bb.campaign.auction` | nonconsumable | $7.99 | 12 new authored missions + 6 new fixed loot definitions, 1 collection | after early base content |
| `bb.campaign.vaults` | nonconsumable | $9.99 | 20 expert missions + 1 new certified hazard mechanic + 1 cosmetic set | future content |
| `bb.pass.s1` | future disabled | $6.99 | fully enumerated pass tiers with progress and no retroactive removal | after real liveops validation |

**No paid bailouts after failure**, no selling guaranteed legendary rewards, no hidden odds, no purchase that makes every bag unlimited. Any bag with mechanical difference can be acquired from normal gameplay; cosmetic case packs may be premium. Tool refills should never become necessary to complete standard free missions. Limit store surfaces to voluntary store + one starter prompt after first enjoyable heist + relevant contextual utility offer only after usage.

Product economics: track payer rate, direct tool pack attach, average repeat purchase among tool buyers, gallery cosmetic interest, real content pack uptake and attrition after consumption. If tools are not purchased, don't introduce frustrating scenarios to force demand; consider hybrid ads/premium pricing instead.

# B15. Free economy and progression (initial defaults)
Currencies: `lootCredits` earned through **saleable** objects, `redSeals` optional real-money premium currency or direct purchase only; do not give both purchase-only and earned versions of the same essential tool without clear difference. Recommended v1: `lootCredits` + directly priced IAP SKUs; skip second currency unless analytics validates its value.

After mission, player chooses `Sell` ordinary items for credits or `Preserve` unique set items at no sale value for gallery progression. **Unique collected relics cannot be permanently lost by browsing UI or failing the next mission.** Duplicate collectibles can be sold. New bag/skill upgrades earned through contract stars and coin costs. No exponential hourly production; active heists earn primary value.

Example rewards: tutorial 500, second mission 750, subsequent starts 1.0k–3.0k escalating by difficulty and bag constraints; bag unlock level targets calibrated so first alternative bag within ~45–90 min, not 3-day wait. Adjust based on session and onboarding telemetry. Progress by stamps and unique objective completions, not merely wealth; limit profitable repeating identical trivial mission by diminishing *progression XP*, not removal of fun.

# B16. Good and bad vault economics
Good challenge: 7x8 bag, enough available items to pack 70%–95% of capacity, at least two possible fitting strategies, a transparent item worth more per occupied cell but worse for weight/alert, one target set item that can be preserved, and a reachable 1-star free solution. Outcome depends on choice, not a hidden random capture.

Bad challenge: 60 indistinguishable jewels, only one exact solution, paid gadget mandatory, arbitrary loss after successful Escape, security rising on every drag micro-adjustment, a rare paid-only bag solving all constraints, or required item bigger than any earnable available bag.

**Generation algorithm:** choose 6–14 loot definition IDs from permitted tier with seed; guarantee contract target(s), packable candidate set, total visible aggregate cell demand > bag area for meaningful selection. Compute maximum value under area/weight/alert via bounded knapsack + polyomino placement search (backtracking using bitmask occupancy for 7x8; memoized state `(occupancyMask, weight, usedItemMask, alert)`; cut off with upper bounds); reject seeds with no solution, trivial one-item win or no meaningful second-best. Keep advanced solver in editor only; runtime receives curated seed and static loot list.

## B17. Fair security algorithm and clear choices
`alert` integer, target limit per vault, each item has `takeAlert`, each special action has `cost`. Avoid unseen chance dice; if hazard has probabilistic risk, declare probability and use free skill challenge only, not paid stochastic rewards. For standard v1 use deterministic outcome: alert over 6 triggers detection and 50% unprotected haul loss, rare collection artifact remains until successfully extracted. A `Secure Seal` could limit loss to 25% if applied before risk (earnable). Risk costs shown beside Take button and yellow/red after result. Show next action safety prediction. Default objective 1-star completable before reaching 6. When player is about to cross, modal `This would trigger the alarm. Continue?` with clear risk and a safe Escape button. Do not lock Escape.

# B18. Art and animation plan
**Style name:** `Velvet Ledger`: 2D editorial object engraving, black/maroon/gold geometric ornament with original glass-jewel spot colors. The goal is a strong **game interface** reminiscent of collectible illustrations, **not** a 3D item photograph. The board occupies 60–72% vertical screen; object silhouettes readable at grid-cell scale; rarity color labels also convey textual symbols. Delicate gold borders at display-size but must not impair taps or contrast.

Assets for launch: 48 flat SVG/vector-produced loot silhouettes with 7 size templates + overlays, 5 bag backdrops with different blocked pattern masks, 6 location silhouette backdrops, 10 set card headers, 12 original UI chrome components (9-slice), 24 icons, 5 alert textures/masks, 10 2D particle types, 6 illustrated gallery panels, 5 bag cosmetic styles, 24 SFX and 4 original minimal music loops. Generate 2D ornamental textures (no heavy 3D shader), clean alpha cutouts, separate shadow masks and thin line art. Save source SVG if authored, PNG output for Unity, colors/shape semantics in data.

Audio: silk drag friction, secure snap click, soft tick on rotation, metallic set chime, uneasy harmonic motif for high alert, resolved warm bell on safe extraction, slower minor-key motif on fail (never violent). Smooth 120–200ms input responses; avoid long mandatory animations. Optional reduced-motion mode retains placement clarity and labels.

**Critical style integrity test:** Every item must appear wholly inside its precise legal grid footprint; source illustration may fill occupied-cell union, but does not visually invade other valid placement cells. No in-game firearms/violence implied by visual tools if not functional. Original museums and names only. Art screenshots must reflect real Unity UI, not atmospheric invented features.

# B19. Human factors and mobile design
Tap/drag threshold avoids accidental take. Zoom item tooltip optional for intricate items; keep 7x8 on 1080 screen with each touch cell generously sized (>44 logical px where possible) and large Rotate, Take and Escape controls. Shelf scroll vertical or bottom row without stealing bag grid room. On small screens use full-width vault board and collapsible details drawer. No critical action accessible exclusively via drag. Long press to inspect and optional tap-to-place works.

Auto-suggest legal cell option is **free hint** for first sessions; late-game optional gadgets add strategical info or reduce alarm, never the only way to use basic rotation and fit checks. Feedback to placement errors: gentle red crosshatched cells, descriptive `Needs two contiguous cells`, no negative punishment.

# B20. Analytics events specific to Black Bag
`vault_open`, `shelf_item_inspected`, `item_previewed`, `item_rotated`, `item_placed`, `place_invalid`, `take_confirmed`, `item_dropped`, `weight_limit_hit`, `alarm_near_limit`, `escape_confirmed`, `heist_success`, `heist_failed`, `heist_retry`, `no_gadget_clear`, `new_set_item_extracted`, `set_completed`, `hideout_viewed`, `bag_equipped`, `tool_used`, `scout_sent`, `scout_returned`, `first_mission`, `first_replay`, `collection_inspected`, `expansion_started`.

Important funnel metrics: novice can rotate item at all, time-to-first-fit, time-to-first-escape, % who voluntarily replay failed mission, % selecting extraction under high alert, mean valid moves per heist, session length, days returned, rare-artifact collection retention, paying conversion and repeat-tool purchases. Analyze frustration from invalid-drop counts and alarm failure, not just purchases.

# B21. Debugging, editor tools, automated QA
1. `PolyominoEditor`: draw occupied squares, validate connectedness, normalize rotations, render 4 orientations, check artwork union mask.
2. `MissionGenerator`: select inventory from certified shapes/value, choose security rule and bag eligibility, validate target solvability.
3. `PackingSolver`: exact search for curated missions under cell, weight, alarm and target; generate proof solutions and near-optimal alternatives. Must not ship solver in client if ranking matters.
4. `InputReplay`: capture pointer, rotate, Take, Escape events and rebuild identical board state via deterministic commands.
5. `CollectionValidator`: ensure all advertised sets have reachable free missions, no permanently impossible collection completions.
6. `ScreenCapture`: full portraits across 22 screens, empty/loading/success/offline/payment and account states.
7. `EconomyTool`: simulate 1k players over 3 months with explicit behavior assumptions, estimate time to first bag/venue and ensure free progression; **not predictive**.
8. `IAPMock`: deferred, fail, purchase repeated, restore, network disconnect, refund.

### Required invariant tests
- 4 rotations restore canonical shape; no off-grid placing; same occupied cells as art placement; no two instances share grid cells; weight and inventory never negative; all heist grants idempotent.
- Taking item before confirmed legal fit cannot grant value or alert advantages. Alarm only advances on actual committed action. Unlimited rearrangements don't increase alarm or alter loot identity.
- `Escape` returns only actual secured objects; leaving a vault early cannot earn mission-target bonus without target; no duplicate gallery relic on rollback.
- Active mission save/load restores orientation, tile allocation, alarm, committed actions and available shelf exactly. Crash on `Take` before save cannot duplicate loot by retry.
- Running same seed with same commands yields same results. Invalid/tampered level yields graceful fallback and analytics, not crash.
- Free bag loadouts can solve all mandatory 1-star levels; human players can finish first mission in <3min reliably.
- No store product gives automatic default 3-star; restored purchased loadout slots count accurately.

# B22. Shipping inventory and explicit exclusions
**Ship v1**: 72 level contracts / 6 locations, 48 loot definitions across 6 families, 5 earnable mechanically different bags, 6 tools, 10 collectible sets with 3–5 items, flat hideout, optional limited scouting, 22 production screens/state types, original source art and sound, local saves, clear result and replay, store of specific products after eligibility, telemetry, localized basic English with localization-ready architecture, purchase refund/restore support, Play release gate.

**Deferred**: interactive guards moving in hallways, character combat, weapons and tactical stealth, AI-powered social networks, huge 3D hotel interiors, physically simulated physics bags, procedurally AI-generated loot at runtime, random paid gacha, public trade market, global leaderboard, PvP, subscription, mandatory events and extensive story dialogue.

# B23. Milestones for Codex
`BB-P00` create Unity 6.3 LTS Universal 2D project, domain/test assemblies, fixed grid, save schema and screens skeleton.
`BB-P01` exact polyomino rotation, preview, collision, snapping and 5x6 board; 10k property tests and mobile gestures.
`BB-P02` action-based alarm, Take versus preview, weight and value, Escape and loss outcomes; deterministic replay + save resume.
`BB-P03` 2D original luxury UI kit, representative 18 object asset set; actual Unity screenshots on phones and safe area.
`BB-P04` guided 3-minute tutorial, 8 curated missions, first collection, new bag; test with novice participants.
`BB-P05` 48 item definitions, 72 missions, generator/solver, difficulty validation, 10 collections and hideout.
`BB-P06` loadouts, gadgets, scouts, persistent upgrades, 22 screens and progression.
`BB-P07` responsive audio/polish, UX reduction, accessibility, low-memory/mobile perf and QA captures.
`BB-P08` analytics, Unity IAP v5 catalog, server-side purchase verification where needed, refund/restore/deferred and support.
`BB-P09` closed test, finance/pricing validation, privacy/data labels, store assets from real gameplay, device and compliance acceptance.
`BB-P10` signed AAB, release bundle, build notes and store submission packet; real review and account approvals require external action.

**Stage gates:** After P02, prove legal item placement and deterministic alert. After P04, show 10 real people complete first vault without assistance and >half want a second. After P05, all mandatory missions provably winnable for free, no monotonous shape dominance. After P08, all payments recover after crash. After P09, no artwork hiding collision and no P0 defects.

# B24. Definition of done and kickoff prompt
A launch candidate has all v1 content valid, first experience understandable, 100% legal placements, 22 screens function in normal/offline/error states, collector gallery contains items from actual heists, no security/touch exploits, durable save and restores, IAP offers and legal disclosures correct, non-random paid products, real Android builds/screenshots, final smoke tests and tested refund/purchase/reinstall behavior. Profitability is decided after cohort revenue data, not at compile time.

**Initial Codex instruction:**
> Read `02_BLACK_BAG_MASTER_SPEC.md` plus the shared tech/workflow/release documents. Create traceability matrix for BB-P00 through BB-P10. Implement P00-P02 only: pure C# bitmask polyomino packing, all four rotations, touch drag/tap controls in 2D, a discrete alert state machine, commit versus rearrange, Escape and deterministic results. Generate automated tests for every shape and 10k randomized moves. Render an actual on-device-style Unity portrait capture using simple original flat shapes. Stop with a test log and issues before adding art batches, content catalog, or IAP.
