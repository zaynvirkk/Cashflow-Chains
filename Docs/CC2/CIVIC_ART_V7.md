# Original civic atlas

24October2026. Generated with the built-in image-generation tool for the operator’s established-city direction. Source:
`/home/virk/.codex/generated_images/01a15983-b8fb-7972-b130-ff0a20641ad5/exec-a63c5326-a954-49a0-9338-94fbf393ab80.png`.

Project copy:Assets/_Game/Resources/Art/V7/civics.png. Eight equal cells in a4×2 atlas:police,fire,hospital,court,auction,water,transit,wholesale. Original front-facing orthographic2D illustrations with navy outlines and blue/red/gold/teal palettes. The hospital uses a green medical plus. There are no baked labels or copied game/brand assets. The original alpha and complete image are retained;runtime sprite rectangles select the cells. Existing original building art supplies the other city structures.

Prompt brief:exactly eight isolated consistent civic sprites on a genuine transparent background;front elevations with some roof visible;not isometric;readable miniature silhouettes,warm windows and refined shading;no characters,text,numbers,UI or backdrop. The generated atlas was visually inspected before import. Public ownership labels remain native UI,and art never awards resources or implies private ownership.

First Android inspection caught a missing hit-target Image and clipped long map labels. The shared hit helper now supports bare rectangles without painting a background;public map captions are shortened and full names/ownership remain in inspection. The regression is in PermanentCityLayoutTests. Failed run20261024T083656Z retains the original NullReferenceException and text audit.
