# Cashflow Chains asset provenance

Current runtime: original painted city environments, a 24-building atlas, planted plots and a bespoke wordmark generated for this project using the built-in image-generation tool. These are integrated sprites with native controls and text, rather than screenshots used as interactive UI. No assets from the named inspiration games are used.

- Art: `Assets/_Game/Resources/Art/V3/` and `V4/`. Exact executed prompts and supplied references: `Docs/VisualV3/EXECUTED_PROMPTS.json`, `Docs/VisualV4/EXECUTED_WORDMARK_PROMPTS.json` and `Docs/VisualV3/References/`.
- Icon: `Assets/_Game/Art/AppIcon.png`. Generated full-bleed city/chain illustration; standard Play icon exported by Unity's texture pipeline to `Release/Store/icon-512.png`.
- Atlas and wordmark alpha bounds are metadata, preserving original generated pixels. Authored vector sources remain in `Assets/_Game/Art/Source/`. Superseded assets outside Resources are in `Docs/SourceArchive/`.
- Display font: Bungee. Body: Barlow Condensed; DejaVu fallback. Font files remain unmodified, with OFL/DejaVu licenses under `Docs/Licenses/` and `Docs/FONT_LICENSE.txt`. Earlier licensed font experiments remain documented.
- Audio: original deterministic stereo synthesis from `Tools/sound-v3.py` and `Tools/sound-v4.py`, with no external recordings/samples. Runtime V4 contains six music tracks and action/ambient cues. Independent saved music/effects switches; reduced motion and haptics settings.
- Native C# produces material frames, route ribbons, resource tokens, floating payouts, particles, press springs, drag ghosts, legal-cell magnet previews and landing animations. These cannot grant currency or alter scoring.

`Docs/ORIGINAL_ASSETS.json` hashes the current runtime resources and preserved vector sources. Generated assets use no runtime model or network service. The hand-authored game wordmark is the only decorative text asset; game instructions, labels, amounts and controls are native text. Review actual captures in `Docs/REVIEW.html` rather than treating concept references as gameplay proof.


V5 expansion: eight advanced building icons and eight actual neon cosmetic sprites in Resources/Art/V5, plus an eight-district atlas. Exact prompts: Docs/VisualV5/EXECUTED_PROMPTS.json. Alpha rectangles are metadata; generated pixels are preserved. Building life, oriented traffic with visible roofs/headlights, smoke/steam/fountain animation and real per-building forecast plates are native code. Compact short-screen mode prioritizes readable building names when a second value line cannot fit; the full exact forecast and inspector remain available.
