# Codex Instructions for This Unity Game

## Governing CC2 direction
The operator accepted `Docs/CC2/CASHFLOW_CHAINS_2_MASTER_REVAMP_SPEC.md` on22October2026. Follow its P00–P17 gated program and `Docs/CC2/CC2_IMPLEMENTATION_TRACKER.csv`. Conflicting older product restrictions are superseded by `Docs/Decisions/2026-10-21-CC2-Commercial-Rebuild.md`; retain puzzle invariants, legitimate ownership, accessibility and transaction security. Preserve baseline branch and do not publish/spend/create accounts without authorization. The original constraints below remain historical where explicitly superseded.

## Sources of truth
Read these documents before editing gameplay: `Docs/01_CASHFLOW_CHAINS_MASTER_SPEC.md`, `Docs/SHARED_TECH_MONETIZATION_LAUNCH.md`, `Docs/RELEASE_AND_TEST_MATRIX.md`. The user-approved game master specification determines mechanics. If a proposal conflicts with it, record a blocking issue and request review rather than silently change the game's identity.

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

