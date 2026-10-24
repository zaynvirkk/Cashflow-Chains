using System;
using System.Collections.Generic;
using System.Linq;
using CashflowChains.Economy;
using CashflowChains.Empire;
namespace CashflowChains.Engagement {
 [Serializable] public sealed class InvestorActivity {public string Id,Kind;public int Season,Xp;public long Day;public InvestorActivity Copy()=>(InvestorActivity)MemberwiseClone();}
 [Serializable] public sealed class InvestorClaim {public string Id,Kind;public int Index,Season;public long Day;public ItemAmount[] Items;public InvestorClaim Copy()=>new InvestorClaim{Id=Id,Kind=Kind,Index=Index,Season=Season,Day=Day,Items=Items.Select(x=>new ItemAmount(x.ItemId,x.Quantity)).ToArray()};}
 [Serializable] public sealed class InvestorPurchase {public string Id,Product;public int Season;public long Day;public InvestorPurchase Copy()=>(InvestorPurchase)MemberwiseClone();}
 [Serializable] public sealed class InvestorState {
  public int Schema=1;public long StartedAt;public List<InvestorActivity> Activity=new List<InvestorActivity>();public List<InvestorClaim> Claims=new List<InvestorClaim>();public List<InvestorPurchase> Purchases=new List<InvestorPurchase>();
  public InvestorState Copy()=>new InvestorState{Schema=Schema,StartedAt=StartedAt,Activity=Activity.Select(x=>x.Copy()).ToList(),Claims=Claims.Select(x=>x.Copy()).ToList(),Purchases=Purchases.Select(x=>x.Copy()).ToList()};
  public int Season(InvestorCatalog c,long now)=>checked((int)((now/86400-StartedAt/86400)/c.SeasonDays));
  public int Xp(int season)=>Activity.Where(x=>x.Season==season).Sum(x=>x.Xp);
  public bool Owns(string family,int season,InvestorCatalog c)=>Purchases.Any(x=>c.Product(x.Product).Family==family&&(family!="season"||x.Season==season));
  public void Validate(BalanceConfig config,EmpireAccount a){var c=config.Investor;if(c==null||Schema!=1||StartedAt<1||StartedAt>a.Economy.LastTrustedAt||Activity==null||Claims==null||Purchases==null||Activity.Any(x=>x==null)||Claims.Any(x=>x==null)||Purchases.Any(x=>x==null)||!a.Economy.Receipts.Any(r=>r.CommandId=="investor-rewards-v1"))throw new ArgumentException("Invalid investor rewards state");
   var receipts=a.Economy.Receipts;var ledger=a.Economy.Ledger;
   if(Activity.Select(x=>x.Id).Distinct().Count()!=Activity.Count||Claims.Select(x=>x.Id).Distinct().Count()!=Claims.Count||Purchases.Select(x=>x.Id).Distinct().Count()!=Purchases.Count)throw new ArgumentException("Duplicate investor projection");
   foreach(var p in Purchases){var product=c.Product(p.Product);if(!p.Id.StartsWith("investor-verified-purchase:",StringComparison.Ordinal)||!receipts.Any(r=>r.CommandId==p.Id&&r.Fingerprint==CurrencyLedger.Hash(r.ConfigRevision+"|investor-store|verified-purchase|"+p.Product+":"+p.Season))||p.Day<StartedAt/86400||p.Day>a.Economy.LastTrustedAt/86400||p.Season<0)throw new ArgumentException("Unverified investor ownership");}
   foreach(var e in Activity)if(e.Xp<0||e.Xp>c.DailyXpCap||e.Season<0||e.Day<StartedAt/86400||e.Day>a.Economy.LastTrustedAt/86400||e.Season!=(e.Day-StartedAt/86400)/c.SeasonDays||!receipts.Any(r=>r.CommandId==e.Id)||!new[]{"profit","acquisition","upgrade"}.Contains(e.Kind))throw new ArgumentException("Invalid investor activity");
   foreach(var day in Activity.GroupBy(x=>x.Day))if(day.Sum(x=>x.Xp)>c.DailyXpCap||day.Count(x=>x.Kind=="profit")>c.ProfitableDaysCap)throw new ArgumentException("Investor daily cap exceeded");
   foreach(var claim in Claims){if(claim.Day<StartedAt/86400||claim.Day>a.Economy.LastTrustedAt/86400||claim.Items==null||!receipts.Any(r=>r.CommandId==claim.Id))throw new ArgumentException("Uncommitted reward");ItemAmount[] expected;int limit=claim.Kind=="chest"?c.Bronze.Length:claim.Kind.StartsWith("daily",StringComparison.Ordinal)?c.Daily.Length:claim.Kind.StartsWith("milestone",StringComparison.Ordinal)?c.Milestones.Length:c.Season.Length;if(claim.Index<0||claim.Index>=limit||claim.Season<0)throw new ArgumentException("Invalid reward index");
    switch(claim.Kind){case "daily":expected=c.Daily[claim.Index].Free;break;case "daily-premium":expected=c.Daily[claim.Index].Premium;if(!Owns("daily",claim.Season,c))throw new ArgumentException("Daily tier not owned");break;case "milestone":expected=c.Milestones[claim.Index].Free;if(a.City==null||a.City.CareerStage<=claim.Index)throw new ArgumentException("Milestone unearned");break;case "milestone-premium":expected=c.Milestones[claim.Index].Premium;if(a.City==null||a.City.CareerStage<=claim.Index||!Owns("growth",claim.Season,c))throw new ArgumentException("Growth reward unearned");break;case "season":expected=c.Season[claim.Index].Free;if(Xp(claim.Season)<(claim.Index+1)*c.TierXp)throw new ArgumentException("Season reward unearned");break;case "season-premium":expected=c.Season[claim.Index].Premium;if(Xp(claim.Season)<(claim.Index+1)*c.TierXp||!Owns("season",claim.Season,c))throw new ArgumentException("Season premium unearned");break;case "chest":expected=c.Bronze[claim.Index].Items;break;default:throw new ArgumentException("Unknown reward track");}
    if(!Same(expected,claim.Items)||ledger.Where(e=>e.CorrelationId==claim.Id).Any(e=>e.Delta>0&&!claim.Items.Any(i=>i.ItemId==e.ItemId))||claim.Items.Any(i=>ledger.Where(e=>e.CorrelationId==claim.Id&&e.ItemId==i.ItemId&&e.Delta>0).Sum(e=>e.Delta)!=i.Quantity))throw new ArgumentException("Reward differs from its committed ledger");
   }
  }
  static bool Same(ItemAmount[] a,ItemAmount[] b)=>a.Length==b.Length&&a.All(x=>b.Any(y=>x.ItemId==y.ItemId&&x.Quantity==y.Quantity));
 }
 public static class InvestorProgress {
  public static void Record(EmpireAccount a,InvestorCatalog c,string id,string kind,long now){var s=a.Investor;if(s==null)return;long day=now/86400;var today=s.Activity.Where(x=>x.Day==day).ToArray();if(kind=="profit"&&today.Count(x=>x.Kind=="profit")>=c.ProfitableDaysCap)return;int remaining=c.DailyXpCap-today.Sum(x=>x.Xp);if(remaining<=0)return;s.Activity.Add(new InvestorActivity{Id=id,Kind=kind,Day=day,Season=s.Season(c,now),Xp=Math.Min(remaining,kind=="profit"?10:kind=="acquisition"?30:20)});}
 }
}
