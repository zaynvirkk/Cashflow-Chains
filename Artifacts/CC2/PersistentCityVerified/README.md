# Persistent city — current regular desktop acceptance

Actual ordinary-input run: `Artifacts/CityWorldReplay/20261023T141441Z` on23October2026. Exact command: `CASHFLOW_TEST_DISPLAY=:96 CASHFLOW_CITY_PLAYER="$PWD/Builds/Linux/CashflowChains.x86_64" CASHFLOW_EXPECT_SANDBOX=0 python3 Tools/city_world_playtest.py`.

The regular player—not a fixture—founds,builds,waits for real clocks,trades,adds housing,names and moves within the city,reviews employees,compares and accepts financing,repays50Cash,purchases land from4×4 to6×4,retains placements and resumes after a cold restart. Three viewport shapes and both process readability audits pass.40 actual traffic samples move with no overlap. `report.json` binds the84 C#/config inputs and actual player files. No save,clock,inventory or transaction was injected.

153 Unity cases pass. The separate domain run verifies104 travel references,10,000 randomized conserved/deterministic layouts and24,000 non-overlap traffic steps. Local API tests are retained in the adjacent Backend directory. Audio analysis checks actual captured PCM,not human preference. Frame times are from software-rendered headless laptop playback while a CPU-bounded Android build also ran;they do not establish60fps phone performance.

This is a verified implementation slice,not approval of the full product,economy,retention,monetization or launch. Source handoffs and old failures remain preserved. Android current-source acceptance is separate. New real-money commerce remains inactive. No publication,spend or platform payment occurred.
