# Kickoff decisions

2026-10-20. Working names and package mechanics retained. Two local independent repositories; no shared library or runtime asset dependency. Android first, portrait, offline. No existing Syndicate Zero art/code is reused. Only Unity's local automation package is embedded from the installed cache after the authenticated 0.9 download failed; embedded version is 0.8.0-exp.1.

The package explicitly requests P00-P02 as the initial deliverable. Three small mechanic laboratory boards are provided so the rules can be exercised. These do not represent the 72-mission release catalog, which remains preserved and unvalidated. Source package files are byte-for-byte preserved in Docs/SourcePackage.

Native uGUI + TextMeshPro + Input System; Canvas safe area with a centered 9:16 content region. Code-generated prototype visuals, licensed DejaVu Sans. No 3D meshes or perspective. The overlay UI uses direct palette colors; world-camera post effects cannot replace UI readability.

Save schema 1: checksummed JSON envelope, flush + atomic replacement and prior backup. Invalid state rejected; backup recovery displayed; two unreadable copies require explicit Reset before any write. Resolution stored before animation, so a mid-animation crash resumes its result without granting twice.

C04: sources emit into outgoing queues; transport feeds next-beat inboxes; recipes consume delivered stock after the transport phase; converter output waits until a subsequent beat. Input/output stock capped at 64/resource/node with discarded overflow recorded. Edge capacity 6 shared across resource kinds. Node IDs, direction N/E/S/W and resource order people/goods/energy fix tie order. Eight beats bound traversal; every produced/consumed token is in the audit ledger. Factory growth is explicit conversion, not cycle duplication. Round cash is signed Int64 and rational basis-point math, floor rounding. Farm has 16 finite source goods across eight beats and requires one scenario-provided energy unit once. This resolves the unspecified farm energy stock budget for the slice.

Bank's auxiliary adjacency link chooses one adjacent commercial outlet by lowest node ID. Multiple bank links on the same sale do not stack. Office/Park bonuses additive, sink capped 3x, round cap 8x. First-clear laboratory reward is 500 once per board, separate from round score. Full launch progression/economy remains deferred.
