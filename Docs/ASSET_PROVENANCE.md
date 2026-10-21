# Original asset record

All building, object, location and interface sprites were authored as SVG paths for this project and rasterized using CairoSVG. Sources are in `Assets/_Game/Art/Source`; runtime PNGs are in `Resources/Art`. Native text is never baked into the art. Black Bag's normalized production masks are in `Docs/ProductionLoot.json`; the supplied seeds remain untouched under `Docs/SourcePackage`.

The music and cues are original deterministic synthesis with no external recordings or samples. The generator layers sine-based keys, bass and soft noise percussion; exports are 22.05 kHz mono waveforms. Cashflow has six 24-second loops and 24 cues; Black Bag has four 24-second loops and 24 cues. Runtime music streams with Vorbis compression; short cues decompress on load. Music and effects can be disabled independently.

DejaVu Sans and Serif are distributed under the included `FONT_LICENSE.txt`. Python art tooling uses CairoSVG, NumPy and Pillow only during authoring, not during gameplay. There is no runtime image or model service. Per-file hashes are recorded in `ORIGINAL_ASSETS.json`.

Black Bag uses one continuous illustration per footprint, clipped to the authoritative cell union. `Tools/author-loot.py` exports all 48 objects; `Tools/verify-art.py` checks alpha boundaries across all 192 rotations. Source masks remain separate from art. Distinct catalog items have distinct raster files.
