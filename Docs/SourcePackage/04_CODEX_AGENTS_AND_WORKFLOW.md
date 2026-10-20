# CODEX AGENTS.MD TEMPLATE AND EXECUTION WORKFLOW
Version 1.0, applies to both projects. Copy the section **BEGIN AGENTS.md** below into each Unity repository root `AGENTS.md` and replace `<GAME_SPEC>` with the corresponding master file.

## BEGIN AGENTS.md
# Codex Instructions for This Unity Game

## Sources of truth
Read these documents before editing gameplay: `Docs/<GAME_SPEC>`, `Docs/SHARED_TECH_MONETIZATION_LAUNCH.md`, `Docs/RELEASE_AND_TEST_MATRIX.md`. The user-approved game master specification determines mechanics. If a proposal conflicts with it, record a blocking issue and request review rather than silently change the game's identity.

## Constraints
1. This is a **pure 2D** mobile game: orthographic camera only, no 3D models, isometric buildings, tilted tile perspective or prefab purchased art. Sprites must be original and consistent. Unity 2D URP, pinned Unity 6.x compatible packages.
2. Keep the core gameplay playable **offline**. No dependence on generative-model inference at runtime, real-time multiplayer, account registration or external servers for ordinary rounds.
3. Implement in testable stages. No broad rewrites, no "done" status without compile/test/device screenshot proof. All public APIs must be strongly typed and covered by tests where meaningful.
4. Core game outcomes authoritative in pure C# domain layer. No direct changes to economy from animation scripts, UI button handlers, purchase callbacks or debug console.
5. Save every important state atomically. All grants, IAP deliveries, contract rewards and first-clear rewards idempotent. Use unique transaction IDs and schema migrations.
6. No fake IAP prices, simulated success or mock SKUs in release builds. Unity IAP **v5** APIs must be checked against pinned version. Paid product entitlements only after verified success; restore nonconsumables and address consumable storage limitations.
7. No fake countdowns, pressure messaging, misleading "sale" badges, forced rewarded advertisements, simulated real money redemption, paid random loot, or deliberately unwinnable free levels.
8. Art generator must never copy assets of named existing games; generated images are concept references until cropped/sliced/imported and inspected in Unity. Keep SVG/vector source and attribution/licensing records.
9. Accessibility and localization are always part of Done; text must be native UI not baked into assets. Screens work at 9:16 and taller/shorter devices with safe areas.
10. Name every new package, SDK or network permission in dependency log with reason and privacy impact. No external network calls by gameplay unless specified by privacy and service design.

## Repository shape
`Assets/_Game/{Art,Audio,Data,Editor,Fonts,Materials,Prefabs,Scenes,Scripts,Tests,UI}`; `Scripts/{Domain,Application,Presentation,Infrastructure,Monetization,Analytics}`; `Docs/`; `Tools/`; `ProjectSettings/`; `Packages/`. Use editor scripting to construct prefabs/assemblies/scenes so code can reproduce them. Do not patch binary scene files manually if scriptable editor code can create equivalent objects reliably.

## Standard cycle for every milestone
A. Read source of truth and list requirements with IDs.
B. Write acceptance tests first for critical domain behavior; implement the minimal working system.
C. Compile and run edit-mode/play-mode tests; record output with Unity version and exact command.
D. Execute a realistic 1-3 minute gameplay pass in the editor; collect **actual** screen captures from Unity with safe-area variants.
E. Compare to art and interaction rules; fix visible and mechanical issues.
F. Run deterministic 10k-case simulator on content rules; generate any needed proof report.
G. Update `Docs/IMPLEMENTATION_STATUS.md`, migration/version notes, test log and known issue list.
H. Only then advance to the next package. Stop and flag `BLOCKED_EXTERNAL` for missing credentials, account/store actions, privacy approval or real tester feedback.

## Output report after each package
- Scope changed, files created/modified, exact Unity/editor versions, build target
- Tests run, numerical pass/fail, exceptions and logs
- Actual screenshots/video and observed differences from specification
- Open issues: P0 blocker, P1 major, P2 polish
- Milestone criterion pass/fail, next package requested

## Make no unstated product changes
Never add a new gameplay currency, AI companion, ads, server, friend invite, subscription, daily login punishment, paywall or major gameplay system without explicit approval in versioned docs. Good-looking mockups are not proof of feasible 2D asset integration.

## END AGENTS.md

---

## 1. Codex task phrasing that produces verifiable outcomes
**Bad prompt:** 'Make the complete polished game with all features and upload it.'
**Good prompt:** 'Implement CC-P02 only. Exact scope: deterministic 8-beat resource propagation, throughput and caps; implement PlayMode visualization stub, unit tests for conservation, cycles, negative profit and reproducible seeds. Compile on pinned Unity, run tests and provide screenshots. Don't touch IAP or navigation.'

Keep work packages small enough that all changed files can be reviewed. Avoid parallel agents modifying the same Unity scenes, prefabs or serialized assets. Parallelize only isolated work such as independent SVG generation, documentation, unit-test fixtures and pure-domain classes with stable interfaces.

## 2. 10-step phased development map (for each game)
| Stage | Scope | Exit proof |
|---|---|---|
| 0 Repository/setup | Unity version, input/UI, CI, package locks | Headless compile and blank scene screenshot |
| 1 Pure rules | Core mechanics without final art | Deterministic test suite and seeded replay |
| 2 Interaction | Tap/drag/rotate/cancel and result | First round completable on touch device |
| 3 Original art gate | Representative full-quality asset set | 3 genuine Unity screenshots approved at 2 resolutions |
| 4 Onboarding | First 3 minutes and restart | Novice testers succeed without coaching |
| 5 Core progression | Levels, challenges, rewards, unlocks | 30min–3hr prototype with replay + save |
| 6 Content pipeline | Authoring tools and full validated v1 catalog | All challenges valid/solvable, no blockers |
| 7 Commercial services | Billing, entitlements, analytics, optional ads | Sandbox pending/restore/refund test matrix |
| 8 Polish/compliance | Audio, a11y, localization, privacy, listing | Device QA and store policy checklist |
| 9 Soft launch | Distribution cohorts, crash analytics, support | Measured retention and real purchase evidence |
| 10 Production | Signed release artifacts, staged publishing | Human-approved store releases, telemetry postlaunch |

Stage 9 release numbers cannot be invented by Codex; it can instrument and analyze, but a human operator supplies developer accounts, cohorts and approvals.

## 3. Deliverables Codex must place into the repository
- Source code and determinism/progression tests
- Unity scenes and prefabs (constructed reproducibly)
- Original art source, exported sprites, atlas config, fonts with license record
- Original audio sources, exported loops/cues, music/SFX controls
- JSON content data, schema validators, content import scripts, a content-ID report
- First-launch, in-progress, results, progression, catalog/shop and settings flows
- Screenshots captured from actual supported Unity build; comparison against reference screens
- Local save/backup/migration with corruption recovery and test fixtures
- Platform billing adapter with mock and sandbox test proof
- Analytics typed event catalog and runtime validation
- Privacy inventory, in-app legal/support panels and external publishing checklist
- CI build scripts, release signing procedure without secret keys in repo, release notes and rollback protocol

## 4. Critical visual rules for Codex
Reference images are artistic targets not a list of screenshots to slice. The game may look *less photorealistic* but should look more coherent, playful and readable in actual 2D. Recreate visual identity with original vector paths, consistent palettes, 9-slice decorated UI, hand-authored flat pattern masks, procedural particles/glows and original audio. Strong silhouette at one phone-cell size matters more than tiny UI engraving.

Never place illustrative 3D-looking house roof artwork on a claimed top-down flat tile. Never paint world labels/profit text into PNG sprites. Never ship an item whose drawn shape differs from its logical footprint. Limit motion in backgrounds if it obscures puzzles.

## 5. External approvals and credentials
The following require user or authorized human action, not autonomous claim of completion: Play/App Store developer account identity verification, tax/merchant payout, bank information, certificates/signing, spending advertising budget, publishing live, privacy/legal signoff, real device QA, independent licensing/trademark checks, production tester enrollment, actual player studies, and store submissions that require human approval. Codex can prepare forms/text/metadata, run local tests, or execute authorized actions through connected systems, but must not fabricate completed approvals.

## 6. Acceptance-traceability file example
```yaml
requirement_id: CC-C04-GRAPH
source: 01_CASHFLOW_CHAINS_MASTER_SPEC.md#C04
owner: gameplay
files:
  - Assets/_Game/Scripts/Domain/FlowResolver.cs
tests:
  - FlowResolverTests.LoopTokensDoNotMultiply
  - FlowResolverTests.ReplaySeedStable
build: android-dev-2026-10-20
status: PASS
proof:
  - Artifacts/tests/flow-tests.xml
  - Artifacts/captures/board_1080x1920.png
```
For Black Bag use `BB-B04-PLACEMENT` and its own tests. A traceability table maps every shipped screen, content set and SKU to test evidence.

## 7. Work isolation and versioning
Git features by short branches; commit reproducible Unity project and content seed JSON; no binary build or secret certificates. CI runs Unity test runner via installed Unity license/environment and collects logs. Use platform snapshots for test savestates; migration tests load one save created at each prior public schema version. Every IAP SKU is stable after submission; remove from storefront by disabling eligibility but preserve entitlement mapping and restoration.

## 8. Prompt to finish a milestone
> Read current `Docs/IMPLEMENTATION_STATUS.md`. Propose the next narrow milestone from the approved spec, implement it, and run all existing tests plus its own acceptance tests. Capture screenshots from Unity, report first failing test rather than ignore it, update changelog and traceability. Do not build unrelated features. Return a clear PASS/BLOCKED/FAIL checklist and code references.
