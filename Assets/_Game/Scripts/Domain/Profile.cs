using System;
using System.Collections.Generic;
using System.Linq;
namespace LocalGame.Data {
[Serializable] public sealed partial class Profile {
 [Serializable] public sealed class GrantRecord{public string Id;public long Amount;}
 public List<string> Entitlements=new List<string>(),StoreReceipts=new List<string>();
 public List<GrantRecord> Grants=new List<GrantRecord>(),Spends=new List<GrantRecord>();
 public int Schema=2,Selected,Theme; public int[] Stars=new int[104]; public long[] Best=new long[104];
 public long Credits=350;public bool Music=true,Sound=true,Haptics=true,ReducedMotion,HighContrast;
 public bool TutorialSeen,FastResolve;public List<string> Collection=new List<string>(),Receipts=new List<string>();
 public int TotalStars=>Stars.Sum();public int Cleared=>Stars.Count(x=>x>0);public int Unlocked=>Math.Min(71,Stars.Take(72).Count(x=>x>0)+2);
 public bool Award(int level,int stars,long score,string receipt,IEnumerable<string> collected=null) {
  if(level<0||level>=104||stars<0||stars>3||score<0||score>1000000000||string.IsNullOrEmpty(receipt)||receipt.Length>100)throw new ArgumentException("Invalid award");
  if(Receipts.Contains(receipt))return false;Receipts.Add(receipt);if(Receipts.Count>2000)Receipts.RemoveAt(0);
  Credits=checked(Credits+Math.Max(0,stars-Stars[level])*100L);Stars[level]=Math.Max(stars,Stars[level]);Best[level]=Math.Max(Best[level],score);
  if(collected!=null)foreach(string id in collected)if(!Collection.Contains(id))Collection.Add(id);return true;
 }
 public bool GrantCosmetic(string product,string receipt){if((product!="cc.skin.neon"&&product!="bb.cosmetic.noir")||string.IsNullOrEmpty(receipt)||receipt.Length>512)throw new ArgumentException("Invalid purchase");if(StoreReceipts.Contains(receipt))return false;StoreReceipts.Add(receipt);if(!Entitlements.Contains(product))Entitlements.Add(product);Theme=product=="cc.skin.neon"?3:5;return true;}
 public bool Grant(string id,long amount){if(string.IsNullOrEmpty(id)||amount<0||amount>1000)throw new ArgumentException("Invalid grant");if(Grants.Any(g=>g.Id==id))return false;Grants.Add(new GrantRecord{Id=id,Amount=amount});Credits=checked(Credits+amount);return true;}
 public static long StyleCost(int theme)=>theme<1||theme>5?0:theme*750L;
 public bool StyleUnlocked(int theme)=>theme>=0&&theme<6&&(theme<=Cleared/12||Spends.Any(s=>s.Id=="style-"+theme)||theme==3&&Entitlements.Contains("cc.skin.neon")||theme==5&&Entitlements.Contains("bb.cosmetic.noir"));
 bool Spend(string id,long amount){if(amount<=0||Credits<amount||Spends.Any(s=>s.Id==id))return false;Spends.Add(new GrantRecord{Id=id,Amount=amount});Credits-=amount;return true;}
 public bool BuyStyle(int theme){if(theme<1||theme>5||StyleUnlocked(theme))return false;return Spend("style-"+theme,StyleCost(theme));}
 public int EarnedBlueprintCapacity=>Math.Min(8,2+Stars.Take(72).Count(x=>x>0)/12+Spends.Count(s=>s.Id.StartsWith("blueprint-")));
 public int BlueprintCapacity=>EarnedBlueprintCapacity+(Entitlements.Contains("cc.blueprint.4")?4:0);
 public long BlueprintPrice=>500+250L*Spends.Count(s=>s.Id.StartsWith("blueprint-"));
 public bool BuyBlueprintSlot(){if(Cleared==0||EarnedBlueprintCapacity>=8)return false;int count=Spends.Count(s=>s.Id.StartsWith("blueprint-"));return Spend("blueprint-"+(count+1),BlueprintPrice);}
 public void SelectDaily(int level){if(level<0||level>=104)throw new ArgumentException("Invalid daily puzzle");Selected=level;}
 public void Select(int level){if(level<0||!CanSelect(level))throw new ArgumentException("Locked level");Selected=level;}
 public void SetTheme(int theme){if(!StyleUnlocked(theme))throw new ArgumentException("Locked palette");Theme=theme;}
 public static Profile Validate(Profile p) {
  if(p!=null){p.MigrateCommercialFields();p.ValidateCommerce();if(p.Stars!=null&&p.Stars.Length==72&&p.Best!=null&&p.Best.Length==72){Array.Resize(ref p.Stars,104);Array.Resize(ref p.Best,104);}}
  if(p!=null){if(p.Grants==null)p.Grants=new List<GrantRecord>();if(p.Spends==null)p.Spends=new List<GrantRecord>();}
  if(p!=null){if(p.Entitlements==null)p.Entitlements=new List<string>();if(p.StoreReceipts==null)p.StoreReceipts=new List<string>();}
  if(p==null||p.Entitlements.Count>8||p.Entitlements.Any(e=>e!="bb.cosmetic.noir"&&!CommerceCatalog.All.Any(x=>x.Id==e&&!x.Consumable))||p.StoreReceipts.Count>1000||p.StoreReceipts.Distinct().Count()!=p.StoreReceipts.Count||p.Spends.Count>11||p.Spends.Any(g=>g==null||string.IsNullOrEmpty(g.Id)||g.Amount<=0||g.Amount>1000000)||p.Spends.Select(g=>g.Id).Distinct().Count()!=p.Spends.Count||p.Schema!=2||p.Stars==null||p.Stars.Length!=104||p.Stars.Any(x=>x<0||x>3)||p.Best==null||p.Best.Length!=104||p.Best.Any(x=>x<0||x>1000000000)||p.Grants==null||p.Grants.Count>1000||p.Grants.Any(g=>g==null||string.IsNullOrEmpty(g.Id)||g.Amount<0||g.Amount>1000)||p.Grants.Select(g=>g.Id).Distinct().Count()!=p.Grants.Count||p.Credits!=350+p.TotalStars*100L+p.Grants.Sum(g=>g.Amount)+p.Purchases.Sum(g=>g.CityCoins)-p.Spends.Sum(g=>g.Amount)||p.Selected<0||p.Selected>103||p.Theme<0||p.Theme>5||p.Collection==null||p.Collection.Count>100||p.Collection.Distinct().Count()!=p.Collection.Count||p.Receipts==null||p.Receipts.Count>2000||p.Receipts.Any(string.IsNullOrEmpty)||p.Receipts.Distinct().Count()!=p.Receipts.Count)throw new ArgumentException("Invalid profile");return p;
 }
}
}