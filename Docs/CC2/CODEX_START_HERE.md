# Codex: Implement Cashflow Chains 2.0

**Repository:** https://github.com/zaynvirkk/Cashflow-Chains  
**Canonical directive:** `CASHFLOW_CHAINS_2_MASTER_REVAMP_SPEC.md` in this handoff package.  
**Traceability:** `CC2_IMPLEMENTATION_TRACKER.csv`.  
**Intended result:** A genuinely working, thoroughly tested Unity Android game whose puzzle loop supports persistent empire progression, managers, building upgrades, fragments, equipment, chests, gacha with guarantees, merchants with paid/earned rerolls, VIP, cards, passes, rotating offers, events, authentic commerce, social systems, analytics and continuously satisfying tactile feedback.

## Your execution directive

1. Import this complete handoff into the repository under `Docs/CC2/` **before editing source code**, or otherwise ensure Codex can access all file contents. Read the entire master spec, repository `AGENTS.md`, relevant historic master/commercial requirements and the actual current code; reconcile changes and add an explicit superseding decision log. Treat the accompanying master as the *new product requirement*, not evidence that it has been implemented.
2. Create a safe feature branch. Compile, test and capture screenshots of the existing game before changing it. Preserve the 8-beat deterministic puzzle engine, 104 campaign levels, original purchased entitlements and local save recovery. Verify actual branch/commit. Never pretend an unverified feature exists.
3. Execute phases **P00 through P17**, respecting each exit gate. Implement real content, backend, store integration, UI, art, sound, VFX, probabilities, validation and behavior; don't implement a mock screen and call the feature complete. Each screen's locked/ready/error/offline states and every CTA must work.
4. Maintain and update `CC2_IMPLEMENTATION_TRACKER.csv`: every row must have a code path, test ID, state (`NOT_STARTED`, `IN_PROGRESS`, `BLOCKED_EXTERNAL`, `DONE_VERIFIED`), source evidence and fresh actual Unity/Android screenshot where applicable. Add child rows for missed requirements rather than silently drop them.
5. Build an economy simulator and validation before enabling revenue-driving catalogs. All premium balances, purchased RNG and paid shop refreshes server-authoritative. Real prices from Google Play metadata. Random paid item odds disclosed accurately in the relevant UI. Keep earned progression meaningful; certified puzzles retain original equal-rule scoring.
6. Run automated tests and device playback **after each slice**. Report numerical results and fix all P0/P1 issues before moving on. Backend outages and IAP pending/deferred/refund paths are first-class test states, not postlaunch tasks.
7. If real Play Console credentials, live backend secrets, age/privacy approval, legitimate Play sandbox purchases or physical-human QA is blocked, complete independent code, tests, deployment artifacts and written setup instructions; mark exact `BLOCKED_EXTERNAL` and request the specific external action. **Do not** insert dummy transactions or claim launch success.
8. Do not publish or change user billing/accounts without explicit authorization. Finished deliverables must include the branch, commits, test/build reports, config catalogs, deployment manifests, app builds, screenshots, balance reports and operating runbook.

## Pasteable kickoff instruction

> Implement the entire Cashflow Chains 2.0 handoff from `Docs/CC2/CASHFLOW_CHAINS_2_MASTER_REVAMP_SPEC.md` and track all requirements in `Docs/CC2/CC2_IMPLEMENTATION_TRACKER.csv`. This new specification supersedes older incompatible commercial requirements, but keep the existing 104 puzzle levels, deterministic solver, legitimate saves, purchased entitlements and original visual identity. Begin with P00 baseline and progress sequentially through P17 with fully verified vertical slices. The result must be a real, satisfying mobile game whose persistent empire, rewards, randomized acquisition, dynamic offers, merchants, VIP, events, purchase systems and telemetry all share one authoritative economy. All advertised buttons, upgrades, timers, rolls, claims, passes and transaction paths must work. Make no fake prices, fake countdowns, simulated store grants, placeholders, or claims of guaranteed revenue. After each phase provide test evidence, genuine screenshots and `BLOCKED_EXTERNAL` items. Continue independent work even when external platform configuration is missing. Do not publish without explicit consent.

## External dependencies likely to require operator action

Actual Google Play Console product setup, licensing/verification credentials, backend hosting and secrets, privacy policy publishing and actual Data Safety forms, optional ad network credentials, physical-device testers, store submission and publishing authority. No such credentials are assumed to exist from the repository alone.
