using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using CashflowChains.Economy;
internal static class Simulation {
 sealed class AccountResult {public long Cash,Wood,Steel,Plans,SpentCents,LostCash,LostMaterials;public int Upgrades,NoActionDays,ShortageDays,ExhaustionDay,FirstManagerDay,TimeToTier3,Contracts;public bool SecondBuilder;}
 static int[] sessions={8,15,45,90};static int[] budgets={0,99,1000,4000,20000};
 static int Cost(UpgradeDefinition u,string id)=>(int)(u.Costs.FirstOrDefault(x=>x.ItemId==id)?.Quantity??0);
 static AccountResult Account(BalanceConfig c,int minutes,int budget,int seed,bool allowPurchase,int days,bool hiatus){
  var random=new Random(seed);var purchaseRandom=new Random(seed^0x353191);var a=new AccountResult{Cash=c.FirstCash.Sum(),Wood=c.FirstWood.Sum()};var levels=new int[c.Businesses.Length];var finish=new long[c.Businesses.Length];int starsCleared=3,builds=0;long last=0;double valuePerMinute=.2+purchaseRandom.NextDouble()*2.8; // cents/minute hypothesis, not measured purchasing behaviour
  for(int day=1;day<=days;day++){
   if(hiatus&&day>=21&&day<=34)continue;
   long now=day*86400L;long elapsed=Math.Min(c.OfflineCapSeconds,Math.Max(0,now-last));last=now;
   for(int i=0;i<levels.Length;i++){
    if(finish[i]>0&&finish[i]<=now){levels[i]++;finish[i]=0;builds++;a.Upgrades++;if(levels[i]==3&&a.TimeToTier3==0)a.TimeToTier3=day;}
    if(levels[i]==0)continue;var b=c.Businesses[i];long possible=EconomyMath.WithBonus(b.BaseHourly*elapsed/3600,levels[i]*800,c.ProductionBonusCapBp);long grant=Math.Min(b.Storage,possible);a.LostMaterials+=Math.Max(0,possible-grant);switch(b.Output){case "CASH":a.Cash+=grant;break;case "WOOD":a.Wood+=grant;break;case "STEEL":a.Steel+=grant;break;case "PLANS":a.Plans+=grant;break;}
   }
   int activeMinutes=Math.Max(1,(int)(minutes*(.75+random.NextDouble()*.5)));int operations=Math.Min((int)(c.DailyOperationCash/c.OperationCash),activeMinutes/2);a.Cash+=operations*c.OperationCash;a.Wood+=operations*c.OperationWood;a.Steel+=operations; a.Plans+=operations/5;starsCleared=Math.Min(72,starsCleared+Math.Max(1,activeMinutes/5));
   if(builds>0){int dispatches=Math.Max(1,activeMinutes/8);a.Cash+=dispatches*c.ContractCash;a.Wood+=dispatches*c.ContractWood;a.Steel+=dispatches*2;a.Plans+=dispatches;a.Contracts+=dispatches;if(a.FirstManagerDay==0)a.FirstManagerDay=day;}
   int capacity=a.SecondBuilder?2:1;int started=0;bool shortage=false;bool viableSecond=false;
   for(int i=0;i<levels.Length;i++){
    if(c.Businesses[i].UnlockClears>starsCleared||finish[i]>0||levels[i]>=c.Upgrades.Length)continue;
    var u=c.Upgrade(levels[i]+1);bool affordable=a.Cash>=Cost(u,"CASH")&&a.Wood>=Cost(u,"WOOD")&&a.Steel>=Cost(u,"STEEL")&&a.Plans>=Cost(u,"PLANS");
    if(!affordable){shortage=true;int price=99;double utilityMinutes=Math.Max(0,Cost(u,"CASH")-a.Cash)/(double)c.OperationCash*2;bool wants=utilityMinutes*valuePerMinute>price&&purchaseRandom.NextDouble()<.25;
     if(allowPurchase&&budget-a.SpentCents>=price&&wants){a.Cash+=200;a.Wood+=20;a.Steel+=5;a.Plans++;a.SpentCents+=price;affordable=a.Cash>=Cost(u,"CASH")&&a.Wood>=Cost(u,"WOOD")&&a.Steel>=Cost(u,"STEEL")&&a.Plans>=Cost(u,"PLANS");}
    }
    if(!affordable)continue;if(finish.Count(x=>x>0)>=capacity){viableSecond=true;continue;}
    a.Cash-=Cost(u,"CASH");a.Wood-=Cost(u,"WOOD");a.Steel-=Cost(u,"STEEL");a.Plans-=Cost(u,"PLANS");finish[i]=now+u.Seconds;started++;
    if(u.Seconds<=activeMinutes*60){levels[i]++;finish[i]=0;builds++;a.Upgrades++;if(levels[i]==3&&a.TimeToTier3==0)a.TimeToTier3=day;}
   }
   if(allowPurchase&&!a.SecondBuilder&&viableSecond&&budget-a.SpentCents>=399&&activeMinutes*valuePerMinute*3>399&&purchaseRandom.NextDouble()<.2){a.SpentCents+=399;a.SecondBuilder=true;}
   if(started==0)a.NoActionDays++;if(shortage)a.ShortageDays++;if(a.ExhaustionDay==0&&levels.All(x=>x>=c.Upgrades.Length))a.ExhaustionDay=day;
   // Caps represent warehouse collection limits, never remove purchased premium currency. Report all overflow loss.
   if(a.Cash>100000){a.LostCash+=a.Cash-100000;a.Cash=100000;}
   if(a.Cash<0||a.Wood<0||a.Steel<0||a.Plans<0||a.SpentCents>budget)throw new Exception("Simulator invariant violation");
  }return a;
 }
 static object Percentiles(IEnumerable<long> source){var v=source.OrderBy(x=>x).ToArray();return new{p10=v[(v.Length-1)/10],p50=v[(v.Length-1)/2],p90=v[(v.Length-1)*9/10]};}
 internal static void Run(BalanceConfig c,string[] args){int count=args.Length>0?int.Parse(args[0]):10000;int days=args.Length>1?int.Parse(args[1]):180;string output=args.Length>2?args[2]:"Artifacts/CC2/EconomySimulation/report.json";if(count<1||days<1||days>180)throw new ArgumentException();var reports=new List<object>();var alerts=new HashSet<string>();
  foreach(int minutes in sessions)foreach(int budget in budgets){var actual=new List<AccountResult>();var free=new List<AccountResult>();for(int n=0;n<count;n++){int seed=n+minutes*7919;actual.Add(Account(c,minutes,budget,seed,true,days,n%20==0));free.Add(Account(c,minutes,0,seed,false,days,n%20==0));}int exhausted=actual.Count(x=>x.ExhaustionDay>0);if(exhausted>0)alerts.Add("FINITE_CONTENT_EXHAUSTION: authored business tier horizon ends before180 days; later tiers/sinks required before shipping");if(actual.Any(x=>x.LostCash>0))alerts.Add("CASH_CAP_LOSS: production outpaces useful authored sinks for some accounts");if(actual.Count(x=>x.FirstManagerDay>0)<count)alerts.Add("FIRST_MANAGER_UNREACHED: projection contains an onboarding dead end");reports.Add(new{minutesPerDay=minutes,lifetimeBudgetCents=budget,accounts=count,noPurchaseCounterfactual=true,upgrades=Percentiles(actual.Select(x=>(long)x.Upgrades)),counterfactualUpgrades=Percentiles(free.Select(x=>(long)x.Upgrades)),cash=Percentiles(actual.Select(x=>x.Cash)),spendCents=Percentiles(actual.Select(x=>x.SpentCents)),purchasers=actual.Count(x=>x.SpentCents>0),firstManagerDay=Percentiles(actual.Select(x=>(long)x.FirstManagerDay)),timeToTier3=Percentiles(actual.Select(x=>(long)x.TimeToTier3)),noUpgradeDays=Percentiles(actual.Select(x=>(long)x.NoActionDays)),shortageDays=Percentiles(actual.Select(x=>(long)x.ShortageDays)),lostCash=Percentiles(actual.Select(x=>x.LostCash)),lostProduction=Percentiles(actual.Select(x=>x.LostMaterials)),exhaustedAccounts=exhausted});Console.Error.WriteLine("Simulated "+minutes+"min/day budget="+budget+" accounts="+count);}
  Directory.CreateDirectory(Path.GetDirectoryName(output));var report=new{simulationOnly=true,shippingAccepted=false,scope="Shared C# catalog/math; aggregate180-day construction/production/contract projection only. Manager/recruitment/event/merchant/pass economics not implemented and not validated. No conversion or revenue evidence.",configRevision=c.Revision,days,accountsPerCohort=count,cohorts=reports.Count,totalSyntheticAccounts=count*reports.Count,counterfactualAccounts=count*reports.Count,hiatusAccounts="5% omit days21-34",demandModel="Voluntary utility/price-sensitive Monte Carlo;0.2-3.0 cents/minute assumed willingness,25% opportunity acceptance; lifetime budgets are ceilings,never forced spend. Not calibrated to players.",alerts=alerts.ToArray(),cohortsData=reports};File.WriteAllText(output,JsonSerializer.Serialize(report,Program.Json));Console.WriteLine(output);
 }
}
