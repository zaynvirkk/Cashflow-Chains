# Acceptance traceability

| Requirement | Source and test | Evidence / status |
|---|---|---|
| CC-P00 / S02 / S04 | Editor/BuildProject.cs; pinned ProjectVersion and package lock; Tests/EditMode/SaveTests.cs | Unity 6000.3.26f1 import, desktop build; API36 configured. Android build/headless CI/physical phone NOT TESTED |
| CC-P01 placement and rotation | Scripts/Domain; Presentation/GameApp.cs, FlatUI.cs; CoreTests.cs | deterministic bounds/rotation/invalid moves; desktop pointer replay, phone touch NOT TESTED |
| CC-P02 deterministic rules | CoreTests.cs and SaveTests.cs | 21 EditMode tests, one 10,000-case property run; structured log in Artifacts/Tests/latest.json |
| CC-P02 resume and grants | Scripts/Infrastructure/AtomicSave.cs; full-game JSON, backup and duplicate-grant tests | flush/atomic backup, corruption fallback, actual cold process resume in native review |
| S06 pure original 2D prototype | original code geometry; fonts licensed in Docs/FONT_LICENSE.txt | actual player captures; production art gate NOT ACCEPTED |
| S07 supplied package | Tools/verify-package.py | 17 source files byte-verified, catalogs structurally checked; mission solvability NOT TESTED except three lab fixtures |
| R07 human study | no claimed substitute | NOT TESTED |
| CC-P03 | Master specification | NOT IMPLEMENTED / NOT ACCEPTED |
| CC-P04 | Master specification | NOT IMPLEMENTED / NOT ACCEPTED |
| CC-P05 | Master specification | NOT IMPLEMENTED / NOT ACCEPTED |
| CC-P06 | Master specification | NOT IMPLEMENTED / NOT ACCEPTED |
| CC-P07 | Master specification | NOT IMPLEMENTED / NOT ACCEPTED |
| CC-P08 | Master specification | NOT IMPLEMENTED / NOT ACCEPTED |
| CC-P09 | Master specification | NOT IMPLEMENTED / NOT ACCEPTED |
| CC-P10 | Master specification | NOT IMPLEMENTED / NOT ACCEPTED |
