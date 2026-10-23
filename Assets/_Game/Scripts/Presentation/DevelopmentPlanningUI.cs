using System;
using System.Collections.Generic;
using System.Linq;
using CashflowChains.Domain;
using CashflowChains.Economy;
using CashflowChains.Empire;
using LocalGame;
using TMPro;

namespace CashflowChains.Presentation
{
    public sealed partial class MobileGameApp
    {
        string developmentSourceBusiness;
        int developmentSourceIndex;
        sealed class DevelopmentRoute { public string Title, Detail; public Action Open; }

        bool TryDevelopmentBack()
        {
            if (!empireSandbox || screen != "empire-sources") return false;
            ReturnToDevelopment(developmentSourceBusiness);
            return true;
        }

        void ReturnToDevelopment(string id) { EmpireScreen(); EmpireBusiness(id); }

        void ChooseDevelopmentGoal(string id)
        {
            var previous = data.Goal;
            data.Goal = DevelopmentPlanning.Choose(empireConfig, empire.Snapshot, id);
            Save();
            if (!canSave) { data.Goal = previous; message = "Your plan could not be saved. Existing progress is preserved."; EmpireScreen(); return; }
            sound.Cue("select");
            EmpireBusiness(id);
        }

        void DrawDevelopmentGoal()
        {
            string caption;
            Action open;
            if (data.Goal == null)
            {
                caption = "YOUR NEXT PLAN · Choose a business to develop";
                open = () => EmpireBusiness(empireConfig.Businesses[0].Id);
            }
            else
            {
                var goal = data.Goal;
                string name = Pieces.Get(empireConfig.Business(goal.BusinessId).Recipe).Label;
                string progress;
                var quote = DevelopmentPlanning.Inspect(empireConfig, empire.Snapshot, goal.BusinessId);
                if (goal.Reached(empire.Snapshot)) progress = "PLAN COMPLETE";
                else if (quote.Action == DevelopmentAction.Unlock) progress = quote.Clears + " / " + quote.UnlockClears + " CITY WINS";
                else if (quote.ProjectId != null) progress = "CONSTRUCTION RUNNING";
                else if (quote.Affordable) progress = "READY TO DEVELOP";
                else { var missing = quote.Requirements.First(r => r.Missing > 0); progress = missing.Missing + " " + ItemName(missing.ItemId) + " TO GO"; }
                caption = "YOUR PLAN · " + name + " LV." + goal.TargetLevel + "\n" + progress;
                open = () => EmpireBusiness(goal.BusinessId);
            }
            Studio.Button(root, caption, 38, 210, 644, 68, open);
        }

        void RenderDevelopmentBusiness(string id)
        {
            var account = empire.Snapshot;
            var definition = empireConfig.Business(id);
            var quote = DevelopmentPlanning.Inspect(empireConfig, account, id);
            if (quote.ProjectId != null) { EmpireQueue(); return; }
            var modal = Studio.Modal(root, Pieces.Get(definition.Recipe).Label + " FRANCHISE", EmpireScreen);
            Studio.Image(modal, definition.Recipe, 250, 185, 220, 220);
            string ownership = quote.Level == 0 ? "Build this business to start saving its production." : definition.Recipe == "home" ? "You own these homes. Residents pay rental income." : "You own this permanent business. Its output is saved.";
            Studio.Label(modal, definition.Rarity.ToUpperInvariant() + " · LEVEL " + quote.Level + "\n" + ownership, 44, 420, 632, 92, 25, Studio.Text, TextAlignmentOptions.Center);
            Studio.Label(modal, "PRODUCTION\n" + quote.BeforeHourly.ToString("0.##") + " → " + quote.AfterHourly.ToString("0.##") + " " + ItemName(quote.Output) + " / HOUR", 44, 535, 632, 105, 30, Studio.Accent, TextAlignmentOptions.Center, true);
            string context = quote.Action == DevelopmentAction.Unlock ? quote.Clears + " / " + quote.UnlockClears + " CITY WINS TO UNLOCK"
                : quote.Action == DevelopmentAction.TierLimit ? "AUTHORED DEVELOPMENT LIMIT REACHED"
                : EmpireDuration(quote.Seconds) + " CONSTRUCTION · STORES UP TO " + quote.Storage + " " + ItemName(quote.Output);
            Studio.Label(modal, context, 44, 655, 632, 65, 23, Studio.Text, TextAlignmentOptions.Center);
            Studio.Label(modal, "MATERIALS", 44, 735, 190, 40, 20, Studio.Muted);
            Studio.Label(modal, "OWNED / NEEDED", 244, 735, 240, 40, 20, Studio.Muted, TextAlignmentOptions.Right);
            for (int i = 0; i < quote.Requirements.Length; i++)
            {
                var requirement = quote.Requirements[i]; float y = 782 + i * 47;
                Studio.Label(modal, ItemName(requirement.ItemId), 44, y, 190, 42, 23, Studio.Text, heavy: true);
                Studio.Label(modal, requirement.Owned.ToString("N0") + " / " + requirement.Required.ToString("N0"), 244, y, 240, 42, 25, Studio.Text, TextAlignmentOptions.Right);
                Studio.Label(modal, requirement.Missing == 0 ? "READY" : requirement.Missing + " TO GO", 502, y, 174, 42, 21, requirement.Missing == 0 ? Studio.Accent : FlatUI.Hex("#FFE562"), TextAlignmentOptions.Right, true);
            }
            if (quote.NextLevel != 0)
            {
                bool chosen = data.Goal?.BusinessId == id && data.Goal.TargetLevel == quote.NextLevel;
                var pin = Studio.Button(modal, chosen ? "THIS IS MY PLAN" : "SAVE AS MY PLAN", 44, 970, 304, 80, () => ChooseDevelopmentGoal(id));
                pin.interactable = canSave && !chosen;
                Studio.Button(modal, "FIND SOURCES", 364, 970, 312, 80, () => OpenDevelopmentSources(id));
            }
            string title = quote.Action == DevelopmentAction.Unlock ? "KEEP PLAYING THE FREE CITIES"
                : quote.Action == DevelopmentAction.TierLimit ? "BACK TO MY CITY"
                : quote.Action == DevelopmentAction.BuilderBusy ? "VIEW MY BUILDER"
                : quote.Action == DevelopmentAction.FindResources ? "FIND MATERIALS"
                : quote.Level == 0 ? "START CONSTRUCTION" : "START UPGRADE";
            var action = Studio.Button(modal, title, 44, 1100, 632, 120, () =>
            {
                switch (quote.Action)
                {
                    case DevelopmentAction.Unlock: ContinueEmpirePuzzle(); break;
                    case DevelopmentAction.TierLimit: EmpireScreen(); break;
                    case DevelopmentAction.BuilderBusy: EmpireQueue(); break;
                    case DevelopmentAction.FindResources: OpenDevelopmentSources(id); break;
                    default: EmpireAction(() => empire.StartConstruction(Guid.NewGuid().ToString("N"), id, EmpireNow)); break;
                }
            }, true);
            action.interactable = empireWritable;
        }

        void OpenDevelopmentSources(string businessId, int index = 0)
        {
            developmentSourceBusiness = businessId;
            var account = empire.Snapshot;
            var quote = DevelopmentPlanning.Inspect(empireConfig, account, businessId);
            var requirements = quote.Requirements.Where(r => r.Missing > 0).ToArray();
            if (requirements.Length == 0) requirements = quote.Requirements;
            if (requirements.Length == 0) { ReturnToDevelopment(businessId); return; }
            developmentSourceIndex = Math.Max(0, Math.Min(index, requirements.Length - 1));
            var item = requirements[developmentSourceIndex];
            Base("empire-sources");
            Studio.Box(root, "Project source heading", 18, 12, 684, 240, FlatUI.Hex("#002B40F5"));
            Studio.Header(root, "WORK TOWARD YOUR CHOSEN PROJECT", ItemName(item.ItemId), Pieces.Get(empireConfig.Business(businessId).Recipe).Label + " · LEVEL " + quote.NextLevel, () => ReturnToDevelopment(businessId));
            Studio.Box(root, "Material requirement", 38, 265, 644, 145);
            Studio.Label(root, item.Owned.ToString("N0") + " / " + item.Required.ToString("N0") + " OWNED", 60, 285, 600, 55, 34, Studio.Text, TextAlignmentOptions.Center, true);
            Studio.Label(root, item.Missing == 0 ? "READY FOR YOUR PROJECT" : item.Missing + " MORE NEEDED", 60, 348, 600, 45, 26, Studio.Accent, TextAlignmentOptions.Center);
            var routes = DevelopmentRoutes(businessId, item, quote);
            for (int i = 0; i < routes.Count && i < 3; i++)
            {
                var route = routes[i]; float y = 445 + i * 185;
                Studio.Box(root, "Earned acquisition route", 38, y - 8, 644, 173, FlatUI.Hex("#002B40F5"));
                Studio.Label(root, route.Detail, 54, y, 612, 70, 24, Studio.Text);
                Studio.Button(root, route.Title, 38, y + 78, 644, 85, route.Open, i == 0);
            }
            if (routes.Count == 1) Studio.Image(root, empireConfig.Business(businessId).Recipe, 230, 665, 260, 280);
            Studio.Box(root, "Materials navigation", 145, 1040, 430, 80, FlatUI.Hex("#002B40F5"));
            Studio.Button(root, "‹", 38, 1040, 95, 80, () => OpenDevelopmentSources(businessId, (developmentSourceIndex + requirements.Length - 1) % requirements.Length));
            Studio.Label(root, (developmentSourceIndex + 1) + " / " + requirements.Length + " MATERIALS", 155, 1055, 410, 48, 23, Studio.Muted, TextAlignmentOptions.Center);
            Studio.Button(root, "›", 587, 1040, 95, 80, () => OpenDevelopmentSources(businessId, (developmentSourceIndex + 1) % requirements.Length));
            Studio.Button(root, "BACK TO MY PROJECT", 38, 1175, 644, 85, () => ReturnToDevelopment(businessId));
            WriteEmpireProbe();
        }

        List<DevelopmentRoute> DevelopmentRoutes(string target, DevelopmentRequirement item, DevelopmentQuote quote)
        {
            var routes = new List<DevelopmentRoute>();
            var account = empire.Snapshot;
            var freeCity = Enumerable.Range(0, 72).Where(i => i <= Math.Min(71, quote.Clears + 2) && data.Profile.CanSelect(i) && !account.Economy.Receipts.Any(r => r.CommandId == "certified-first-clear:" + i)).Select(i => (int?)i).FirstOrDefault();
            if (freeCity.HasValue && (item.ItemId == "CASH" || item.ItemId == "WOOD"))
            {
                int city = freeCity.Value;
                var reward = RoundRewardPolicy.VerifyCertified(empireConfig, city, Campaign.Get(city).Solution).Items.First(r => r.ItemId == item.ItemId);
                routes.Add(new DevelopmentRoute { Title = "PLAY CITY " + (city + 1).ToString("00"), Detail = "+" + reward.Quantity + " " + ItemName(item.ItemId) + " for its first successful clear. Replays keep your score, not another first-clear reward.", Open = () => StartLevel(city) });
            }
            var active = account.Dispatches.FirstOrDefault(d => !d.Claimed && d.Rewards.Any(r => r.ItemId == item.ItemId));
            var delivery = empireConfig.Operations.Dispatches.Where(d => d.UnlockClears <= quote.Clears && d.Rewards.Any(r => r.ItemId == item.ItemId) && DispatchPlan(d.RequiredRecipe) != null).OrderBy(d => d.Seconds).FirstOrDefault();
            if (active != null)
                routes.Add(new DevelopmentRoute { Title = "VIEW MY RETURN", Detail = "A running delivery carries " + ItemName(item.ItemId) + ". Its saved return is ready at the displayed timer.", Open = () => EmpireDispatchDetail(active.Id, () => ReturnToDevelopment(target)) });
            else if (delivery != null && account.Businesses.Any(b => b.Level > 0))
            {
                var selectedRoute = delivery;
                bool slot = account.Dispatches.Count(d => !d.Claimed) < EmpireBenefits.DispatchSlots(empireConfig, account);
                bool daily = account.Dispatches.Count(d => d.StartedAt / 86400 == EmpireNow / 86400) < empireConfig.Operations.DailyDispatchStarts;
                bool capacity = slot && daily;
                bool fee = delivery.Costs.All(c => account.Economy.Amount(c.ItemId) >= c.Quantity);
                routes.Add(new DevelopmentRoute { Title = capacity && fee ? "VIEW DELIVERY OFFER" : "VIEW MY DELIVERY ROUTES", Detail = "+" + delivery.Rewards.First(r => r.ItemId == item.ItemId).Quantity + " BASE " + ItemName(item.ItemId) + " · " + EmpireDuration(delivery.Seconds) + " base time\nFee: " + string.Join(" · ", delivery.Costs.Select(c => c.Quantity + " " + ItemName(c.ItemId))) + (capacity && fee ? "" : !slot ? " · all delivery slots occupied" : !daily ? " · today’s start limit reached" : " · fee not yet affordable"), Open = capacity && fee ? (Action)(() => EmpireDispatchOffer(selectedRoute.Id, () => ReturnToDevelopment(target))) : EmpireDispatch });
            }
            var producer = empireConfig.Businesses.Where(b => b.Output == item.ItemId && b.UnlockClears <= quote.Clears).OrderByDescending(b => account.Businesses.Any(o => o.Id == b.Id && o.Level > 0)).FirstOrDefault();
            if (producer != null)
            {
                var owned = account.Businesses.FirstOrDefault(b => b.Id == producer.Id && b.Level > 0);
                long at = EmpireNow;
                var pending = account.Construction.FirstOrDefault(p => p.BusinessId == producer.Id && p.FinishAt < at);
                long through = pending == null ? at : pending.FinishAt;
                long output = owned == null ? 0 : ProductionPolicy.Quote(empireConfig, owned, Math.Max(owned.ProducedAt, through), EmpireBenefits.Bonus(empireConfig, account, "production-bp"), EmpireBenefits.Bonus(empireConfig, account, "storage-bp")).Amount;
                routes.Add(new DevelopmentRoute { Title = output > 0 ? "COLLECT OWNED OUTPUT" : owned == null ? "DEVELOP " + Pieces.Get(producer.Recipe).Label : "VIEW " + Pieces.Get(producer.Recipe).Label + " PRODUCTION", Detail = owned == null ? "This unlocked business produces " + ItemName(item.ItemId) + " after construction. Its project uses the listed materials and your builder." : output > 0 ? output + " " + ItemName(item.ItemId) + " is ready in this business. Collection returns to your project." : "This owned business produces at its real hourly rate, within its storage and eight-hour cap.", Open = output > 0 ? (Action)(() => EmpireAction(() => empire.CollectAll(Guid.NewGuid().ToString("N"), EmpireNow), () => ReturnToDevelopment(target))) : () => EmpireBusiness(producer.Id) });
            }
            if (routes.Count == 0)
            {
                var next = empireConfig.Operations.Dispatches.Where(d => d.Rewards.Any(r => r.ItemId == item.ItemId)).OrderBy(d => d.UnlockClears).FirstOrDefault();
                routes.Add(new DevelopmentRoute { Title = "VIEW DELIVERY REQUIREMENTS", Detail = next == null ? "Inspect the available business sources for this material." : "First delivery route: " + next.Title + " · " + next.UnlockClears + " city wins and a successful " + Pieces.Get(next.RequiredRecipe).Label + " plan.", Open = () => { EmpireDispatch(); if (next != null) EmpireDispatchOffer(next.Id); } });
                if (freeCity.HasValue) { int city = freeCity.Value; routes.Add(new DevelopmentRoute { Title = "CONTINUE THE FREE CITIES", Detail = "Complete city " + (city + 1) + " to work toward the actual unlock. It does not directly grant this material.", Open = () => StartLevel(city) }); }
            }
            return routes;
        }
    }
}
