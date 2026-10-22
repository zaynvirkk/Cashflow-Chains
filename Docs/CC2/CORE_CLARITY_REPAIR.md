# Core clarity and input repair — operator correction, 23 October 2026

The operator reports that the game looks glitchy and does not explain what a home, café or connection represents. This is an open P1 product failure. Passing solver tests does not resolve it. Further manager/reward/store expansion is paused until the opening is coherent and native input is dependable.

## Meaning the game must communicate

- The player plans a neighborhood's trading network. Moving a piece edits a plan; it does not purchase a real property or charge rent to the player.
- A Home represents residents. They become customers when a walkable route reaches a shop entrance. The arrow marks the route's direction; a receiving dot marks an entrance. Merely being diagonally nearby is insufficient. Most routes span one neighboring block; Logistics explicitly bridges an empty block.
- A Café serves customers using supplies. One customer visit plus one supply produces a sale. The opening café already has 14 supplies; the next lesson introduces delivery from a farm. Its stock must be visible before activation.
- Teal people, gold crates and blue power symbols distinguish visitors, supplies and electricity. Color alone is insufficient. A line must correspond to a genuine resolver shipment; no invented profitable connection.
- Activation runs eight trading steps of the plan. Sales value is the round objective, distinct from the saved Cash/material reward. A replay may improve a score without granting the same first-clear reward twice.
- The permanent empire contains separately acquired assets. A completed residential property there earns rental income; this is not a rent payment, ownership purchase or expense on the certified puzzle board. This distinction must appear in the home detail and help.

These are explanations of existing recipe behavior and the master specification's Piece/Business/BoardNode separation, not changes to the deterministic solver or reference scores. The numerical puzzle remains an abstraction of trade, not a physically simulated city or financial product.

## Scope and gates

Implement the master §10.1 direct-to-board first launch; remove the coach obscuring streets; explain the reason for the first move and the supplied stock; use actual customer/sales counts in feedback; preserve free undo, forecast, input alternatives and all 104 reference results. Improve route semantics and empty-plot hierarchy before adding decoration.

Investigate missed taps using raw pointer positions, uGUI raycast targets and native press/release traces. Do not fix a production defect by adding blind retries to the replay. Repeat ordinary navigation and test edge presses/drag cancellation, then replay genuine dispatch/research actions. Keep actual failures as evidence.

Acceptance is pending. Native screenshots and automation prove rendering/interaction behavior only. First-time human comprehension, enjoyment and retention require real participants; do not infer them from a successful script.


## Findings from current replay

The first three lessons pass with the widened font, revised semantics and reconciled wallet. The longer P04 run completes actual dispatch/research/earned-speedup actions, but its original final-only overflow report is not sufficient acceptance: cold restart overwrote earlier dense-board findings. Exact observed labels were `3 VISITS / STEP`, `2 GOODS / STEP` and `NO CUSTOMERS` at5-column density. Those need shorter captions while retaining the full selected-building explanation. The replay tools now archive every process's audit/performance/audio output and combine all overflow findings. Re-run before accepting the whole slice.

Android's interrupted compile was an actual systemd-oomd terminal-scope kill, not a game crash or a successful build. Build commands now use a bounded user resource scope, one importer, two IL2CPP/Bee jobs and BEE_BUILD_THREADS=2 for the outer native build. The current run is still pending; no current Android pass is inferred.
