using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using CashflowChains.City;
using CashflowChains.Economy;
using CashflowChains.Empire;
internal static class CityEconomyHarness {
 static readonly JsonSerializerOptions Json=new JsonSerializerOptions{IncludeFields=true,WriteIndented=true};
 static int Main(string[] args){try{var config=JsonSerializer.Deserialize<BalanceConfig>(File.ReadAllText(args[0]),Json);config.Validate();var scenarios=new List<object>();
 foreach(string policy in new[]{"single-cafe","premium-price","advertising-spend","hire-without-demand","reinvest-positive-margin"}){
  var e=new EmpireEngine(config,new EmpireAccount{Economy=new EconomyState{AccountId="synthetic-city-"+policy}},_=>true);long now=1000;e.OpenCity(now);e.BuildCity("founding-home","home",1,1,++now);now+=3;e.FinishCityBuild("founding-home",now);e.BuildCity("founding-cafe","cafe",2,1,++now);now+=3;e.FinishCityBuild("founding-cafe",now);e.AdvanceCityTo(now);int rejectedProjects=0,built=1;var milestones=new List<object>();
  for(int step=0;step<180;step++){
   var a=e.Snapshot;int day=a.City.Day;var r=e.CurrentCityDay();
   if(r.CoffeeServed>r.CoffeeDemand||r.Resources.Any(x=>x.Closing<0||x.Initial+x.Produced+x.Imported!=x.Consumed+x.Exported+x.Spilled+x.Closing))throw new Exception("Resources or demand violated");
   if(day==3&&policy=="premium-price")e.SetCityPrice("premium",2,12500,now);
   if(day==3&&policy=="advertising-spend")e.MarketCityBusiness("marketing",2,15,now);
   if(day==3&&policy=="hire-without-demand")e.StaffCityBusiness("hire",2,2,now);
   if(policy=="reinvest-positive-margin"&&a.City.Jobs.Count<a.City.BuilderSlots){
    var empty=Enumerable.Range(0,a.City.Width*a.City.Height).FirstOrDefault(n=>!a.City.Properties.Any(p=>p.X==n%a.City.Width&&p.Y==n/a.City.Width));int x=empty%a.City.Width,y=empty/a.City.Width;
    var choices=config.City.Businesses.Select(d=>e.QuoteCityInvestment(d.Kind,x,y)).Where(q=>q.Unlocked&&q.BuilderAvailable&&q.AfterNet>q.BeforeNet&&a.Economy.Amount("CASH")-q.Costs.Sum(cost=>cost.ItemId=="CASH"?cost.Quantity:Math.Max(0,cost.Quantity-q.Owned.First(o=>o.ItemId==cost.ItemId).Quantity)*config.City.Good(cost.ItemId).ImportPrice)>=60&&a.City.Properties.Where(p=>p.Open).Sum(p=>CityWorkforce.Staff(config.City.Business(p.Kind),p))+config.City.Business(q.Kind).BaseStaff<=CityPopulation.Workers(config.City,a.City.Properties,a.City.GridTier,a.City.Day)).OrderByDescending(q=>(decimal)(q.AfterNet-q.BeforeNet)/q.Costs.First(c=>c.ItemId=="CASH").Quantity).ToArray();
    foreach(var q in choices){try{foreach(var cost in q.Costs.Where(c=>c.ItemId!="CASH")){int missing=(int)Math.Max(0,cost.Quantity-q.Owned.First(o=>o.ItemId==cost.ItemId).Quantity);if(missing>0)e.BuyCityMaterials("material-"+day+"-"+cost.ItemId,cost.ItemId,missing,now);}e.BuildCity("reinvest-"+day,q.Kind,x,y,now);built++;break;}catch(InvalidOperationException){rejectedProjects++;}}
   }
   now=e.Snapshot.City.Trading.FinishAt;e.AdvanceCityTo(now);a=e.Snapshot;long revision=a.Economy.Revision,cash=a.Economy.Amount("CASH");e.AdvanceCityTo(now);if(e.Snapshot.Economy.Revision!=revision||e.Snapshot.Economy.Amount("CASH")!=cash)throw new Exception("Duplicate clock reward");
   if(a.Economy.Balances.Any(b=>b.Purchased!=0))throw new Exception("Synthetic run received paid stock");
   if(step==0||step==6||step==29||step==89||step==179)milestones.Add(new{closedDay=a.City.Day-1,cash,population=a.City.Population(config.City),properties=a.City.Properties.Count,unpaidBills=a.City.Payables,nextNet=e.ForecastCity().Net,serializedBytes=System.Text.Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(a,Json))});
  }
  scenarios.Add(new{policy,built,rejectedProjects,milestones});
 }
 File.WriteAllText(args[1],JsonSerializer.Serialize(new{safetyChecksPassed=true,economyAccepted=false,daysPerScenario=180,policies=5,scope="Synthetic scenario simulation through actual C# commands with simulated clocks. No native/human/payment evidence. Small fixed policies do not establish optimal strategy,retention,commercial balance or profitability. Positive-margin bot has no lookahead,upgrades,grid expansion or land acquisition;stalling is diagnostic,not proof all investments are exhausted.",scenarios},Json));Console.WriteLine("Five synthetic city policies,180 days each: conservation,finite demand,earned-only stock and repeated-clock idempotency pass. Economy acceptance remains false.");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
