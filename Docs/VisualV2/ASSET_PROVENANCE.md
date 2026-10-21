# V2 asset provenance

The operator supplied ten style/composition concepts in `/home/virk/Downloads/Games 2.0`. All were inspected. The new runtime environments and object sheets were generated with the built-in image-generation tool from original descriptive prompts, then copied into `Assets/_Game/Resources/Art/V2`. They are project-bound game assets, not a flattened mockup. No assets were downloaded from the named inspiration games. Source generated files remain under `/home/virk/.codex/generated_images/01a15983-b8fb-7972-b130-ff0a20641ad5`.

`world.png`: portrait 1024×1536 painted environment, empty central playfield, no UI/text baked in. Cashflow: blue-hour town, luminous windows, verdigris and coral, cobblestone, canal reflections, framing ivy. Black Bag: candlelit midnight collector study, walnut, brass, oxblood velvet, empty table.

`objects.png`: transparent atlas, 6×4 building sprites (Cashflow) / 8×6 treasure sprites (Black Bag), hand-painted gouache/ink descriptive direction with isolated subjects and no text. Atlas subjects follow the corresponding domain catalog. Runtime slicing preserves the atlas source; Black Bag uses an original UI shader to clip each painted item to its authoritative cell union. The early alternate Cashflow building study is retained as a discarded concept, not a runtime dependency.

`GameFeel.cs`: original native uGUI meshes for material frames, world motes, bounded burst particles and living resource ribbons. No additional SDK, package or network call. All native text remains separate and localizable; DejaVu license unchanged.

`Tools/sound-v2.py`: original deterministic NumPy synthesis, stereo warm keys/bass/brushes for the town and minor celesta/pads for the study; new tactile wood/leather/metal transients and multi-note stingers. No external music or samples. Writes V2 assets separately, preserving the earlier score and cues. Source hashes are in ORIGINAL_ASSETS.json.
