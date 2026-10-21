# Cashflow production expansion - 21 October 2026

The operator's latest instruction supersedes the earlier free-only release direction. Follow C12 and S08-S09 in the supplied specifications; do not ask the operator to choose a different monetization model. Android / Google Play remains first; Black Bag remains on hold. The five supplied images guide city density, readable value plates, material contrast and progression. Promotional prices, fake timers, a pass and premium-currency balances in references are not implementation authority.

## Gaps confirmed in current source

- Traffic is a tiny unrotated rectangle, so horizontal cars appear wrong. Improve its silhouette, lane direction and intersection behavior, and remove distracting background traffic artefacts where practical.
- City decoration is largely baked into scenery. Add bounded traffic, pedestrians, window/fountain/smoke motion tied to active buildings, with reduced-motion support and measured frame cost.
- Typography/contrast is uneven. Keep Bungee for short calls to action; improve body/amount hierarchy, mission labels, building value plates and compact-screen sizes.
- There are already 24 definitions and 72 base levels, but gameplay does not expose persistent building collections or a combo encyclopedia. Some combo checks currently test mere presence rather than demonstrated contribution. Fix those before expanding counts.
- Commerce was removed at the operator's earlier free-only instruction. Reintroduce Unity IAP v5 with the documented SKU catalog, not fake prices or a functioning local purchase mock in release.
- Current native first-session interaction is intermittent under the automation driver. Diagnose actual gesture/focus behavior; never mark failed input as passing or weaken state assertions.

## Work packages and verification

1. Polish traffic/living city/text and finish first-session gesture checks. Inspect actual native and Android captures at compact/tall/desktop sizes; audit overflow, meaningful delta feedback, payout interruption, audio and frame time.
2. Persistent building collections and combo discovery/encyclopedia, with actual-flow requirements and idempotent collection rewards. Test false-positive combos, repeat clears, resets, legacy saves and reachable sets.
3. Deliver the documented optional expansion content: 12 Harbor scenarios and 20 Finance expert scenarios, two additional districts and eight distinct advanced piece definitions/art. Preserve IDs/rules/rewards for 72 base puzzles. Validate all 104 references, alternate layouts, budgets and conservation. Paid access is entitlement-gated; no base progression paywall.
4. Implement the C12 catalog: starter, gems 90/520/1150, five draft rerolls, four additional blueprint slots, eight actual neon skins, Harbor and Finance packs. Season pass disabled. Draft rerolls apply only to optional drafting; no selling nonexistent benefits. Keep raw USD hypotheses out of runtime purchase UI. Use local-only consumable disclosures as S08 allows; no cross-device consumable restore claim.
5. Unity IAP adapter: fetch real metadata, validate Google receipts, save before confirmation, deduplicate receipts, handle pending/deferred/cancel/unavailable, restore nonconsumables and reconcile entitlements only after successful history retrieval. Test the domain and an editor/test-only fake provider separately from actual platform acceptance.
6. Update builds/signing/permission/privacy/store documents, source-bound replay and actual screenshot gallery, then commit/push to the requested private repository. No store upload or public publishing without explicit operator instruction.

No claim of complete commercial acceptance until the purchase sandbox matrix, physical devices, genuine human testing and applicable Play Console steps have actual evidence. Local upload key exists and remains ignored. Public listing identity: Zayn Virk, zaynvirkk@gmail.com.
