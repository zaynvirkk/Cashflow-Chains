# Persistent-city economic diagnostics

Run `DOTNET_PROCESSOR_COUNT=2 python3 Tools/city_economy_sim.py` from the repository root. This compiles the exact pure C# domain and a separate tool harness using Unity's pinned compiler/runtime. No Unity assets are modified. `source.json` binds domain/config/harness inputs.

All policies pay for a home and café from the ordinary200Cash founding grant;zero default homes or residents. Different demographics affect actual demand and staff availability.

Five fixed,earned-only policies run180 simulated operating days each: one café,one price change,daily advertising,extra hiring without more demand,and reinvestment in positive forecast margins. Reinvestment buys missing materials at actual Cash prices and uses actual construction/worker constraints. It has no strategic lookahead,upgrades,grid expansion or land acquisition;its stopping point is not evidence that all gameplay is exhausted.

Every day checks finite coffee demand,resource conservation,nonnegative stock,no purchased balances and no grants when the same clock is submitted twice. All five runs pass these checks. The small starting neighborhood shows that spending on staff/advertising can lower margin when population is the bottleneck;price and supply decisions can change outcomes. These are deterministic examples,not universal recommendations or optimum policies.

`economyAccepted` intentionally remains false. These cases do not cover all content paths,pricing/upgrade curves,paid/free gaps,resource sinks,disruption recovery,manager/event economies or long-term account storage. Native play,human comprehension/enjoyment,retention and commercial data remain separate. The older five-tier simulator's rejection is preserved;this new diagnostic does not overturn it into a commercial approval.
