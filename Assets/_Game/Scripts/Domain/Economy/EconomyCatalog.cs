using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CashflowChains.Empire;

namespace CashflowChains.Economy {
 [Serializable] public sealed class ItemDefinition {
  public string Id,Kind,DisplayKey; public long MaxStack; public bool Exclusive,CanExpire;
 }
 [Serializable] public sealed class ItemAmount {
  public string ItemId; public long Quantity;
  public ItemAmount() {} public ItemAmount(string id,long quantity){ItemId=id;Quantity=quantity;}
 }
 [Serializable] public sealed class SourceDefinition {
  public string Id,ItemId,Feature; public bool Free,Repeatable; public int UnlockClears;
 }
 [Serializable] public sealed class ExchangeDefinition {
  public string Id,Input,Output; public long Cost,Quantity;
 }
 [Serializable] public sealed class UpgradeDefinition {
  public int Level,Seconds; public ItemAmount[] Costs;
 }
 [Serializable] public sealed class BusinessDefinition {
  public string Id,Recipe,Rarity,Role,Output,Ability; public int UnlockClears,BaseHourly,Storage;
 }
 [Serializable] public sealed class BalanceConfig {
  public int Schema=1,Revision=1,OfflineCapSeconds=28800,ProductionBonusCapBp=10000,TimeReductionCapBp=5000;
  public long LegacyCashCap=1000000,DailyOperationCash=600;
  public int OperationCash=30,OperationWood=3,FirstContractSeconds=60,ContractCash=60,ContractWood=8;
  public ItemDefinition[] Items; public SourceDefinition[] Sources; public ExchangeDefinition[] Exchanges;
  public UpgradeDefinition[] Upgrades; public BusinessDefinition[] Businesses;
  public int[] StarFragments; public int[] FirstCash,FirstWood;
  public string[] EnabledFeatures;public OperationsCatalog Operations;
  public ItemDefinition Item(string id)=>Items.FirstOrDefault(x=>x.Id==id)??throw new ArgumentException("Unknown item: "+id);
  public BusinessDefinition Business(string id)=>Businesses.FirstOrDefault(x=>x.Id==id)??throw new ArgumentException("Unknown business: "+id);
  public UpgradeDefinition Upgrade(int level)=>Upgrades.FirstOrDefault(x=>x.Level==level)??throw new ArgumentException("Unauthored upgrade tier");
  public void Validate(){
   if(Schema!=1||Revision<1||OfflineCapSeconds<1||OfflineCapSeconds>28800||TimeReductionCapBp<0||TimeReductionCapBp>5000||ProductionBonusCapBp<0||ProductionBonusCapBp>10000||LegacyCashCap<31550||DailyOperationCash<OperationCash||OperationCash<=0||OperationWood<=0||FirstContractSeconds<1||ContractCash<=0||ContractWood<=0)throw new ArgumentException("Invalid economy limits");
   if(Items==null||Items.Length==0||Items.Any(x=>x==null||string.IsNullOrEmpty(x.Id)||string.IsNullOrEmpty(x.DisplayKey)||x.MaxStack<1)||Items.Select(x=>x.Id).Distinct().Count()!=Items.Length||Items.Any(x=>x.Id=="ROUND_SCORE"||x.Id=="GEMS"&&x.CanExpire))throw new ArgumentException("Invalid item catalog");
   if(Sources==null||Sources.Select(x=>x.Id).Distinct().Count()!=Sources.Length||Sources.Any(x=>x==null||x.UnlockClears<0||x.UnlockClears>72))throw new ArgumentException("Invalid sources");
   foreach(var source in Sources)Item(source.ItemId);
   foreach(var item in Items.Where(x=>!x.Exclusive)){var free=Sources.Where(x=>x.ItemId==item.Id&&x.Free).ToArray();if(free.Length<2||!free.Any(x=>x.Repeatable))throw new ArgumentException("Missing sustainable free routes: "+item.Id);}
   if(Upgrades==null||Upgrades.Length<5||Upgrades.Select(x=>x.Level).Distinct().Count()!=Upgrades.Length)throw new ArgumentException("Invalid upgrades");
   foreach(var u in Upgrades){if(u.Level<1||u.Level>50||u.Seconds<1||u.Seconds>2592000||u.Costs==null||u.Costs.Length==0||u.Costs.Select(x=>x.ItemId).Distinct().Count()!=u.Costs.Length)throw new ArgumentException("Invalid upgrade");foreach(var c in u.Costs)if(c.Quantity<1||c.Quantity>Item(c.ItemId).MaxStack)throw new ArgumentException("Invalid cost");}
   if(Businesses==null||Businesses.Length<8||Businesses.Select(x=>x.Id).Distinct().Count()!=Businesses.Length)throw new ArgumentException("Invalid businesses");
   foreach(var b in Businesses){CashflowChains.Domain.Pieces.Get(b.Recipe);Item(b.Output);if(b.UnlockClears<3||b.BaseHourly<1||b.Storage<b.BaseHourly||string.IsNullOrEmpty(b.Ability))throw new ArgumentException("Invalid business");}
   if(StarFragments==null||!StarFragments.SequenceEqual(new[]{10,10,25,55,110,220})||FirstCash==null||FirstWood==null||FirstCash.Length!=3||FirstWood.Length!=3||FirstCash.Any(x=>x<0)||FirstWood.Any(x=>x<0)||EnabledFeatures==null)throw new ArgumentException("Invalid onboarding");
   if(Exchanges==null)throw new ArgumentException("Missing exchanges");
   foreach(var e in Exchanges){Item(e.Input);Item(e.Output);if(e.Cost<1||e.Quantity<1||e.Cost>1000000000||e.Quantity>1000000000)throw new ArgumentException("Invalid exchange");}
   if(Operations==null)throw new ArgumentException("Missing operations catalog");Operations.Validate(this);
   // Exact rational relaxation: even a profitable cycle smaller than floating-point precision is rejected.
   var ids=Items.Select(x=>x.Id).ToArray();var numerators=ids.Select(_=>BigInteger.One).ToArray();var denominators=ids.Select(_=>BigInteger.One).ToArray();
   for(int iteration=0;iteration<ids.Length;iteration++){bool changed=false;foreach(var e in Exchanges){int a=Array.IndexOf(ids,e.Input),b=Array.IndexOf(ids,e.Output);BigInteger num=numerators[a]*e.Quantity,den=denominators[a]*e.Cost;if(num*denominators[b]>numerators[b]*den){var gcd=BigInteger.GreatestCommonDivisor(num,den);numerators[b]=num/gcd;denominators[b]=den/gcd;changed=true;if(iteration==ids.Length-1)throw new ArgumentException("Positive exchange cycle");}}if(!changed)break;}
  }
 }
 public static class EconomyMath {
  public static long WithBonus(long value,int bonus,int cap){if(value<0||bonus<0||cap<0)throw new ArgumentException();return checked(value*(10000L+Math.Min(bonus,cap)))/10000;}
  public static long Duration(long seconds,int reductionBp){if(seconds<1||reductionBp<0)throw new ArgumentException();return Math.Max(1,checked(seconds*(10000L-Math.Min(reductionBp,5000)))/10000);}
 }
}
