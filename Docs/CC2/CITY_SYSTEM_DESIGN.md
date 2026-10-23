# One-city systems contract

This extends the operator's23October direction and the complete Game Bible. It separates the working city slice from intended subsequent systems. It is a design contract,not a claim that every system below exists. Current implementation/limits are in PERSISTENT_CITY_PROGRESS.md;commercial dependencies are in CITY_COMMERCIAL_INTEGRATION.md.

The player owns businesses and property inside a city. They do not own residents,their income,or the government. Residential rent,shop sales and export receipts are different revenue sources. Wages,suppliers,utilities,freight,tax,maintenance and debt service are different outflows. A connection represents a useful exchange or journey,not a rule that two tiles must touch. Roads provide access;distance affects time,capacity and delivery cost. The first city slice uses automatically connected rectangular streets;editable roads,preferred suppliers and route capacity need separate implementation.

## Decisions must be explainable before commitment

For a project,show its upfront Cash/material/labor needs,completion time,operating capacity,expected revenue,recurring costs and the assumption most likely to invalidate its margin. Current affordability uses unreserved owned resources. Forecasts are conditional and must not label potential output as guaranteed sales. A farm with negative standalone Cash can still be useful if it reduces downstream purchases;show its actual physical output and avoided-import comparison rather than inventing farm revenue.

For a running business,show a short causal chain: eligible demand → share won → capacity/stock served → customer receipts → costs → margin → financing outflow. At most the leading bottlenecks belong on the primary screen;the detailed ledger remains inspectable. Distinguish jobs offered,available workers and actual employees. Finite population and daily needs prevent an unlimited-coffee loop. Demand not served by the player can go to NPC competitors or remain unfilled.

Current simplified demand is bounded by population/capacity;it does not yet model individual household wallets. A later household budget model must include external/commuter income,earned wages,rent and discretionary spending without counting the player's internal transfers as new wealth. Similarly,a hotel bed is capacity,not an arriving tourist or a paid booking. A port/airport is access,not unlimited tourist income.

## Industry relationships and their risk

| Network | Necessary exchange | Useful decision and downside | Current state |
| --- | --- | --- | --- |
| Housing and commerce | Residents provide finite demand;shops exchange goods/services for Cash | More housing needs jobs/infrastructure;more shops compete for the same demand | Empty founding map,actual household profiles/ages/budgets/rent/demand/worker eligibility implemented;individual bank accounts/income and commute markets pending |
| Food and manufacturing | Farms grow inputs;factories convert supplied materials;shops serve final demand | Self-supply saves import expense but adds capital,wages,power,storage and idle-capacity risk | Finite stock/inputs/outputs/import prices/storage implemented;specialized agricultural chains pending |
| Construction | Labor,materials,Cash and builder time produce usable property | Own a construction company to gain supply/capacity;pay its ongoing cost even when little building is needed | Materials/builders/company and earned second builder implemented;full project labor market pending |
| Utilities | Delivered energy/water/waste capacity enables operations | Buy capacity or build local supply;idle infrastructure costs money and shortages limit service | Staff-dependent power/grid billing implemented;water/waste/reliability pending |
| Logistics and trade | Freight capacity moves real stock between suppliers and users | Local stock versus imports;exports compete with useful local reserves;long routes cost time/Cash | Distance/freight/storage/import-export limits implemented;vehicles,editable routes and congestion economics pending |
| Tourism | Access brings a finite pool;rooms/attractions compete for bookings/spend | Ports/airports/hotels need supporting infrastructure;empty capacity still has costs | Limited seasonal port arrivals and hotel beds implemented;airports,visitor budgets and attraction effects pending |
| Finance | Principal is received once;fixed fees and repayments are future outflows | Compare added operating margin with repayment;missed obligations remain owed | One fixed-fee loan,scheduled/early payments and arrears implemented;credit ratings/refinancing pending |
| Marketing and partners | Spend competes for existing demand;agreements exchange costs for defined terms | Advertising may lose money;a supplier deal needs enough volume;a sponsor pays only on actual conditions | Three marketing budgets,one supplier agreement and conditional coffee sponsorship implemented;negotiation/brand systems pending |
| Government | Published taxes/permits/capacity rules constrain businesses | Plan for known obligations;earn useful infrastructure access through legitimate goals | Sales tax,build permit cost and grid/land access implemented;policy choices/public services pending |
| Competitors and seasons | Other businesses compete for demand;calendar changes real input/output conditions | Diversification and reserves reduce concentration risk;capacity can be unprofitable in a weak season | NPC demand weight and four harvest/energy/visitor seasons implemented;distinct rivals and dynamic market response pending |

No random catastrophic loss is needed to make a decision matter. Predictable obligations,limited demand,opportunity cost and scarce operating capacity already create risk. Introduce additional uncertainty only with inspectable causes and bounded recovery. Preserve a viable earned recovery route after a bad investment;do not sell a real-money rescue against an invented crisis.

## Progressive introduction and ownership

The founding loop teaches homes → finite customers → café supplies → supported sales → net Cash. Next introduce a second productive investment,one visible supply shortage,then utility/capacity and price/staff decisions. Financing follows a working understanding of margin and repayment. Trade,tourism,research,managers and advanced industries should unlock as the player has a real use for them. These are milestones inside the same city,not replacement maps. No skip action substitutes for learning the first short operations.

Keep names,placements,chosen prices,supply preferences,visual themes and development targets durable. Expansion reveals adjacent owned land. The camera retains a useful fit/zoom/pan across expansion and restart. Prestige or a new district must not silently erase the personal city. Future blueprint and specialization tools should make alternative layouts viable,while preserving the equal-rule historical puzzles.

## Retention and commercial integration

Persistent stock,completed projects,dispatches,earned rewards,collections and actual event milestones create reasons to return. They must report what happened during absence and what useful decision is next. Use a bounded offline policy;avoid losing an unclaimed paid entitlement or fabricating a near-miss reward. A season/event clock must correspond to a real authoritative schedule.

A voluntary resource-source panel can connect a selected goal to production,contracts,earned merchants,chests and an eligible paid route. Affordable projects promote the project action. A verified permanent builder benefit should mean an explicit additional concurrent project,including for someone who already earned the construction-company builder. Storage/freight benefits must modify actual useful capacity. A premium pass must have a real free/paid reward track and exactly-once claims. Managers/fragments/recruitment require useful roles,disclosed pools/guarantees and server-authoritative paid results. No inactive product appears as a working purchase.

In-game business advertising spends fictional Cash to compete for customers. Optional rewarded advertising is a distinct commercial feature with consent,real ad completion and verified benefits. In-game supplier/sponsor terms do not authorize external advertising campaigns or real-world partnership contact. Paid convenience,resource development,cosmetics,passes and cards remain the Bible's connected commercial system;new account/billing/reward/LiveOps implementation is still required.

Measure comprehension,repeat sessions,meaningful alternate plans,resource bottlenecks,purchase value,refunds and long-horizon inflation together. Current tests establish deterministic behavior and actual local interactions. They do not establish sustained enjoyment,retention or profit.
