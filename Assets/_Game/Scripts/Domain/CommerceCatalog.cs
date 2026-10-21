using System;using System.Collections.Generic;using System.Linq;
namespace LocalGame.Data {
 public sealed class CommerceProduct {
  public readonly string Id,Title,Description;public readonly bool Consumable;public readonly int Gems,Rerolls,CityCoins;
  public CommerceProduct(string id,string title,string description,bool consumable=false,int gems=0,int rerolls=0,int coins=0){Id=id;Title=title;Description=description;Consumable=consumable;Gems=gems;Rerolls=rerolls;CityCoins=coins;}
 }
 public static class CommerceCatalog {
  public static readonly CommerceProduct[] All={
   new CommerceProduct("cc.start.01","FOUNDER STARTER","500 city coins once, two founder finishes and a permanent founder badge.",coins:500),
   new CommerceProduct("cc.gems.small","90 SPARK GEMS","90 gems for optional decorations and draft rerolls.",true,90),
   new CommerceProduct("cc.gems.medium","520 SPARK GEMS","520 gems for optional decorations and draft rerolls.",true,520),
   new CommerceProduct("cc.gems.large","1,150 SPARK GEMS","1,150 gems for optional decorations and draft rerolls.",true,1150),
   new CommerceProduct("cc.reroll.5","FIVE DRAFT REROLLS","Five optional tray rerolls in Draft Practice. Core puzzles never require them.",true,rerolls:5),
   new CommerceProduct("cc.blueprint.4","FOUR EXTRA BLUEPRINTS","Four permanent slots beyond the eight earnable slots. Existing layouts are preserved."),
   new CommerceProduct("cc.skin.neon","NEON CITY","Eight coordinated neon building skins. Cosmetic only; no scoring advantage."),
   new CommerceProduct("cc.scenario.harbor","HARBOR STORIES","12 additional harbor puzzles, two conditions and a new district."),
   new CommerceProduct("cc.scenario.finance","FINANCE AFTER DARK","20 expert puzzles and eight advanced buildings in an optional district.")
  };
  public static CommerceProduct Get(string id)=>All.FirstOrDefault(p=>p.Id==id)??throw new ArgumentException("Unknown product");
  public static bool Eligible(Profile p,string id){var product=Get(id);if(p.Cleared==0)return false;if(!product.Consumable&&p.Entitlements.Contains(id))return false;switch(id){case "cc.start.01":return p.Cleared>=12||p.PlaySeconds>=600&&p.Cleared>=3;case "cc.blueprint.4":return p.UsedBlueprints>=2;case "cc.skin.neon":return p.Stars.Take(12).All(s=>s>0);case "cc.scenario.harbor":return p.Stars.Take(60).Count(s=>s>0)>=36;case "cc.scenario.finance":return p.Stars.Take(72).Count(s=>s>0)>=60;case "cc.reroll.5":return p.DraftsPlayed>0;default:return p.Cleared>=3;}}
 }
 public sealed partial class Profile {
  [Serializable] public sealed class PurchaseRecord {public string Id,Sku;public int Gems,Rerolls,CityCoins;}
  [Serializable] public sealed class PremiumSpend {public string Id;public int Gems,Tickets;}
  public List<PurchaseRecord> Purchases=new List<PurchaseRecord>();public List<PremiumSpend> PremiumSpends=new List<PremiumSpend>();
  public List<string> DiscoveredCombos=new List<string>(),Decorations=new List<string>();
  public string ActiveDecoration="";public int Gems,RerollTickets,UsedBlueprints,DraftsPlayed,Finish;public double PlaySeconds;
  public void MigrateCommercialFields(){if(Purchases==null)Purchases=new List<PurchaseRecord>();if(PremiumSpends==null)PremiumSpends=new List<PremiumSpend>();if(DiscoveredCombos==null)DiscoveredCombos=new List<string>();if(Decorations==null)Decorations=new List<string>();if(ActiveDecoration==null)ActiveDecoration="";if(Entitlements==null)Entitlements=new List<string>();}
  public bool CanAccess(int level)=>level>=0&&level<104&&(level<72||Entitlements.Contains(level<84?"cc.scenario.harbor":"cc.scenario.finance"));
  public bool CanSelect(int level){if(!CanAccess(level))return false;if(level<72)return level<=Unlocked;int start=level<84?72:84;return level<=start+Stars.Skip(start).Take(level<84?12:20).Count(s=>s>0)+1;}
  public bool ApplyPurchase(string sku,string receipt){var item=CommerceCatalog.Get(sku);if(string.IsNullOrWhiteSpace(receipt)||receipt.Length>512||Purchases.Count>=10000)throw new ArgumentException("Invalid purchase reference");var prior=Purchases.Find(r=>r.Id==receipt);if(prior!=null){if(prior.Sku!=sku)throw new ArgumentException("Order product mismatch");return false;}
   if(!item.Consumable&&Purchases.Any(r=>r.Sku==sku)){if(!Entitlements.Contains(sku))Entitlements.Add(sku);return false;}
   var record=new PurchaseRecord{Id=receipt,Sku=sku,Gems=item.Gems,Rerolls=item.Rerolls,CityCoins=item.CityCoins};
   int gems=checked(Gems+item.Gems),tickets=checked(RerollTickets+item.Rerolls);long coins=checked(Credits+item.CityCoins);Purchases.Add(record);Gems=gems;RerollTickets=tickets;Credits=coins;if(!item.Consumable&&!Entitlements.Contains(sku))Entitlements.Add(sku);return true;
  }
  public void ReconcilePurchases(IEnumerable<string> owned,bool historyVerified){if(!historyVerified)return;var known=new HashSet<string>(owned.Where(s=>CommerceCatalog.All.Any(p=>p.Id==s&&!p.Consumable)));Entitlements.RemoveAll(s=>s.StartsWith("cc.")&&!known.Contains(s));foreach(var s in known)if(!Entitlements.Contains(s))Entitlements.Add(s);if(Finish==1&&!Entitlements.Contains("cc.skin.neon")||Finish==2&&!Entitlements.Contains("cc.start.01"))Finish=0;}
  public bool SpendDraftReroll(string operation){if(string.IsNullOrEmpty(operation)||operation.Length>100||PremiumSpends.Any(s=>s.Id==operation))return false;int ticket=RerollTickets>0?1:0,cost=ticket==1?0:10;if(Gems<cost)return false;PremiumSpends.Add(new PremiumSpend{Id=operation,Gems=cost,Tickets=ticket});Gems-=cost;RerollTickets-=ticket;return true;}
  public bool BuyDecoration(string id){int cost=id=="mint-parade"?50:id=="coral-confetti"?60:id=="gold-rain"?80:0;if(cost==0||Decorations.Contains(id)||Gems<cost)return false;PremiumSpends.Add(new PremiumSpend{Id="decoration-"+id,Gems=cost});Decorations.Add(id);Gems-=cost;return true;}
  public void SetFinish(int finish){if(finish<0||finish>2||finish==1&&!Entitlements.Contains("cc.skin.neon")||finish==2&&!Entitlements.Contains("cc.start.01"))throw new ArgumentException("Unowned finish");Finish=finish;}
  public void ValidateCommerce(){if(Purchases.Any(r=>r==null)||PremiumSpends.Any(r=>r==null))throw new ArgumentException("Invalid ledger entry");if(Purchases.Count>10000||Purchases.Select(r=>r.Id).Distinct().Count()!=Purchases.Count||Purchases.Any(r=>r==null||string.IsNullOrEmpty(r.Id)||r.Id.Length>512))throw new ArgumentException("Invalid purchase ledger");foreach(var r in Purchases){var p=CommerceCatalog.Get(r.Sku);if(r.Gems!=p.Gems||r.Rerolls!=p.Rerolls||r.CityCoins!=p.CityCoins)throw new ArgumentException("Altered purchase fulfillment");}if(Purchases.Where(r=>!CommerceCatalog.Get(r.Sku).Consumable).GroupBy(r=>r.Sku).Any(g=>g.Count()>1)||PremiumSpends.Count>10000||PremiumSpends.Select(s=>s.Id).Distinct().Count()!=PremiumSpends.Count||PremiumSpends.Any(s=>s==null||string.IsNullOrEmpty(s.Id)||s.Gems<0||s.Gems>80||s.Tickets<0||s.Tickets>1)||Gems!=Purchases.Sum(r=>r.Gems)-PremiumSpends.Sum(s=>s.Gems)||RerollTickets!=Purchases.Sum(r=>r.Rerolls)-PremiumSpends.Sum(s=>s.Tickets)||Gems<0||RerollTickets<0||Finish<0||Finish>2||double.IsNaN(PlaySeconds)||double.IsInfinity(PlaySeconds)||PlaySeconds<0||PlaySeconds>315576000||UsedBlueprints<0||UsedBlueprints>12||DraftsPlayed<0||DraftsPlayed>100000||DiscoveredCombos.Any(n=>!CashflowChains.Domain.ComboBook.All.Any(c=>c.Name==n))||DiscoveredCombos.Count>32||DiscoveredCombos.Distinct().Count()!=DiscoveredCombos.Count||ActiveDecoration!=""&&!Decorations.Contains(ActiveDecoration)||Decorations.Count>3||Decorations.Any(d=>d!="mint-parade"&&d!="coral-confetti"&&d!="gold-rain"))throw new ArgumentException("Invalid commercial state");}
 }
}
