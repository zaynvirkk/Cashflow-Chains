using System;
using System.Collections.Generic;
using System.Linq;
using LocalGame.Data;

namespace CashflowChains.Economy {
 [Serializable] public sealed class LegacyMigrationPlan {
  public string MigrationId,SnapshotHash;public int Schema=3;
  public long CashEscrow,GemEscrow,ResidualLegacyCash;public int RerollTickets,BlueprintCapacity,Selected,Theme,Finish;
  public int[] Stars;public long[] Best;public string[] EntitlementClaims,PurchaseReferences;
  public List<string> Collection,Combos,Decorations;
  public bool RequiresTrustedVerification=true;
 }
 public static class LegacyMigration {
  // Produces a reviewable import claim. It neither modifies the original save nor manufactures verified cloud ownership.
  public static LegacyMigrationPlan Prepare(Profile legacy,string originalPayload,BalanceConfig config){
   if(string.IsNullOrEmpty(originalPayload))throw new ArgumentException("Missing untouched backup");Profile.Validate(legacy);
   return new LegacyMigrationPlan{MigrationId="legacy-v2:"+CurrencyLedger.Hash(originalPayload),SnapshotHash=CurrencyLedger.Hash(originalPayload),CashEscrow=Math.Min(legacy.Credits,config.LegacyCashCap),ResidualLegacyCash=Math.Max(0,legacy.Credits-config.LegacyCashCap),GemEscrow=legacy.Gems,RerollTickets=legacy.RerollTickets,BlueprintCapacity=legacy.BlueprintCapacity,Selected=legacy.Selected,Theme=legacy.Theme,Finish=legacy.Finish,Stars=(int[])legacy.Stars.Clone(),Best=(long[])legacy.Best.Clone(),EntitlementClaims=legacy.Entitlements.ToArray(),PurchaseReferences=legacy.Purchases.Select(x=>x.Id).Concat(legacy.StoreReceipts).Distinct().ToArray(),Collection=new List<string>(legacy.Collection),Combos=new List<string>(legacy.DiscoveredCombos),Decorations=new List<string>(legacy.Decorations)};
  }
  public static void StageEscrow(LegacyMigrationPlan plan,CurrencyLedger ledger,long trustedNow,Func<EconomyState,bool> persist){
   if(plan==null||plan.Schema!=3||!plan.RequiresTrustedVerification||plan.CashEscrow<0||plan.GemEscrow<0)throw new ArgumentException("Invalid migration plan");
   ledger.Execute(plan.MigrationId,plan.SnapshotHash+":"+plan.CashEscrow+":"+plan.GemEscrow,"legacy-v2","unverified-import",trustedNow,t=>{if(plan.CashEscrow>0)t.Grant("CASH",plan.CashEscrow,BalanceOrigin.ImportEscrow);if(plan.GemEscrow>0)t.Grant("GEMS",plan.GemEscrow,BalanceOrigin.ImportEscrow);},persist);
  }
 }
 public sealed class SourceFinder {
  readonly BalanceConfig config;public SourceFinder(BalanceConfig config){this.config=config;}
  public SourceDefinition[] Available(string item,int clears,IEnumerable<string> activeFeatures){config.Item(item);var features=new HashSet<string>(activeFeatures);return config.Sources.Where(x=>x.ItemId==item&&x.UnlockClears<=clears&&features.Contains(x.Feature)).OrderBy(x=>!x.Free).ThenBy(x=>!x.Repeatable).ThenBy(x=>x.Id,StringComparer.Ordinal).ToArray();}
 }
}
