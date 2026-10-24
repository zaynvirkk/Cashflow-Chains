using System;
using System.Linq;
using CashflowChains.Economy;
namespace CashflowChains.Engagement {
 [Serializable] public sealed class InvestorRewardPair {public ItemAmount[] Free,Premium;}
 [Serializable] public sealed class InvestorLoot {public string Id;public int Weight;public ItemAmount[] Items;}
 [Serializable] public sealed class InvestorProduct {public string Id,Title,Family,Description;public int MinimumAssets;public bool Consumable;public ItemAmount[] Items;}
 [Serializable] public sealed class InvestorCatalog {
  public int Revision=1,SeasonDays=28,TierXp=30,DailyXpCap=100,ProfitableDaysCap=5;public InvestorRewardPair[] Daily,Milestones,Season;public InvestorLoot[] Bronze;public InvestorProduct[] Products;
  public InvestorProduct Product(string id)=>Products.SingleOrDefault(p=>p.Id==id)??throw new ArgumentException("Unknown investor product");
  public int BronzeIndex(int roll){if(roll<0||roll>=10000)throw new ArgumentOutOfRangeException(nameof(roll));int sum=0;for(int i=0;i<Bronze.Length;i++){sum+=Bronze[i].Weight;if(roll<sum)return i;}throw new InvalidOperationException("Incomplete chest distribution");}
  public void Validate(BalanceConfig c){
   if(Revision!=1||SeasonDays!=28||TierXp<1||DailyXpCap<1||ProfitableDaysCap<1||Daily==null||Daily.Length!=7||Milestones==null||Milestones.Length!=6||Season==null||Season.Length!=30||Bronze==null||Bronze.Length==0||Bronze.Sum(x=>x.Weight)!=10000||Products==null||Products.Select(x=>x.Id).Distinct().Count()!=Products.Length)throw new ArgumentException("Invalid investor catalog");
   foreach(var p in Daily.Concat(Milestones).Concat(Season)){if(p==null)throw new ArgumentException("Missing reward");ValidateItems(c,p.Free);ValidateItems(c,p.Premium);}
   foreach(var d in Bronze){if(d==null||d.Weight<1||string.IsNullOrWhiteSpace(d.Id))throw new ArgumentException("Invalid chest odds");ValidateItems(c,d.Items);}
   if(Bronze.Select(x=>x.Id).Distinct().Count()!=Bronze.Length)throw new ArgumentException("Duplicate chest outcome");
   foreach(var p in Products){if(p==null||string.IsNullOrEmpty(p.Id)||string.IsNullOrEmpty(p.Title)||string.IsNullOrEmpty(p.Description)||p.MinimumAssets<1||!new[]{"starter","growth","season","daily","materials","gems"}.Contains(p.Family))throw new ArgumentException("Invalid investor product");ValidateItems(c,p.Items,true);}
  }
  static void ValidateItems(BalanceConfig c,ItemAmount[] items,bool empty=false){if(items==null||!empty&&items.Length==0||items.Select(x=>x.ItemId).Distinct().Count()!=items.Length)throw new ArgumentException("Invalid reward contents");foreach(var x in items)if(x==null||x.Quantity<1||x.Quantity>c.Item(x.ItemId).MaxStack)throw new ArgumentException("Invalid reward quantity");}
 }
}
