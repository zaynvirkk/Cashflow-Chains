#if UNITY_EDITOR || DEVELOPMENT_BUILD
using LocalGame.Data;
namespace CashflowChains.Domain {
 // Fixture for the separate developer build. No store orders or receipts are fabricated.
 public static class ContentQa {
  public static Profile Create(){var profile=new Profile();for(int i=0;i<104;i++){var mission=Campaign.Get(i);new CityRun(i,mission.Solution.Copy()).Activate(profile,"qa-domain-clear-"+i);}foreach(var product in CommerceCatalog.All)if(!product.Consumable)profile.Entitlements.Add(product.Id);profile.Selected=0;return Profile.Validate(profile);}
 }
}
#endif
