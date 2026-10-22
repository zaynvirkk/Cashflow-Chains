using System;
using System.Linq;
using CashflowChains.Economy;
using CashflowChains.Domain;

namespace CashflowChains.Empire {
 [Serializable] public sealed class DispatchDefinition {
  public string Id,Title,RequiredRecipe,Business;public int UnlockClears,Seconds;public ItemAmount[] Costs,Rewards;
 }
 [Serializable] public sealed class ResearchDefinition {
  public string Id,Title,Tree,Prerequisite,Bonus;public int UnlockClears,Seconds,BonusBp;public ItemAmount[] Costs;
 }
 [Serializable] public sealed class OperationsCatalog {
  public int Revision=1,DailyDispatchStarts=12,SecondFreeDispatchClears=12,ResearchUnlockClears=6,ResearchUnlockClaims=3;
  public DispatchDefinition[] Dispatches;public ResearchDefinition[] Research;
  public DispatchDefinition Dispatch(string id)=>Dispatches.FirstOrDefault(x=>x.Id==id)??throw new ArgumentException("Unknown dispatch");
  public ResearchDefinition Node(string id)=>Research.FirstOrDefault(x=>x.Id==id)??throw new ArgumentException("Unknown research");
  public void Validate(BalanceConfig config){
   if(Revision<1||DailyDispatchStarts<1||DailyDispatchStarts>24||SecondFreeDispatchClears<3||ResearchUnlockClears<3||ResearchUnlockClaims<1||Dispatches==null||Dispatches.Length<15||Research==null||Research.Length<30||Dispatches.Any(x=>x==null)||Research.Any(x=>x==null)||Dispatches.Select(x=>x.Id).Distinct().Count()!=Dispatches.Length||Research.Select(x=>x.Id).Distinct().Count()!=Research.Length)throw new ArgumentException("Invalid operations catalog");
   foreach(var d in Dispatches){Pieces.Get(d.RequiredRecipe);config.Business(d.Business);if(string.IsNullOrEmpty(d.Id)||string.IsNullOrEmpty(d.Title)||d.Seconds<60||d.Seconds>28800||d.UnlockClears<3||d.UnlockClears>72)throw new ArgumentException("Invalid dispatch");Amounts(d.Costs,config);Amounts(d.Rewards,config);}
   string[] bonuses={"production-bp","storage-bp","dispatch-time-bp","dispatch-yield-bp","build-time-bp","research-time-bp"};
   foreach(var r in Research){if(string.IsNullOrEmpty(r.Id)||string.IsNullOrEmpty(r.Title)||r.Tree!="Operations"&&r.Tree!="Logistics"&&r.Tree!="Innovation"||!bonuses.Contains(r.Bonus)||r.BonusBp<1||r.BonusBp>1000||r.Seconds<1||r.Seconds>28800||r.UnlockClears<ResearchUnlockClears||r.UnlockClears>72)throw new ArgumentException("Invalid research");Amounts(r.Costs,config);var seen=new System.Collections.Generic.HashSet<string>{r.Id};var parent=r.Prerequisite;while(!string.IsNullOrEmpty(parent)){if(!seen.Add(parent))throw new ArgumentException("Circular research prerequisite");parent=Node(parent).Prerequisite;}}
  }
  static void Amounts(ItemAmount[] amounts,BalanceConfig config){if(amounts==null||amounts.Length==0||amounts.Any(x=>x==null)||amounts.Select(x=>x.ItemId).Distinct().Count()!=amounts.Length)throw new ArgumentException("Invalid item bundle");foreach(var x in amounts)if(x.Quantity<1||x.Quantity>config.Item(x.ItemId).MaxStack)throw new ArgumentException("Invalid bundle quantity");}
 }
 [Serializable] public sealed class DispatchOperation {
  public string Id,Template;public Board Layout;public ItemAmount[] Rewards;public long StartedAt,FinishAt,ClaimedAt;public int ReductionBp,YieldBp,SpeedupSeconds;public bool Claimed;
  public DispatchOperation Copy()=>new DispatchOperation{Id=Id,Template=Template,Layout=Layout.Copy(),Rewards=Rewards.Select(x=>new ItemAmount(x.ItemId,x.Quantity)).ToArray(),StartedAt=StartedAt,FinishAt=FinishAt,ClaimedAt=ClaimedAt,ReductionBp=ReductionBp,YieldBp=YieldBp,SpeedupSeconds=SpeedupSeconds,Claimed=Claimed};
 }
 [Serializable] public sealed class ResearchProject {public string Id,NodeId;public long StartedAt,FinishAt;public int ReductionBp,SpeedupSeconds;public ResearchProject Copy()=>(ResearchProject)MemberwiseClone();}
 public static class EmpireBenefits {
  public static int Bonus(BalanceConfig config,EmpireAccount a,string kind)=>Math.Min(kind.EndsWith("time-bp")?5000:10000,a.Research.Select(id=>config.Operations.Node(id)).Where(x=>x.Bonus==kind).Sum(x=>x.BonusBp));
  public static int DispatchSlots(BalanceConfig config,EmpireAccount a)=>a.Economy.Receipts.Count(x=>x.CommandId.StartsWith("certified-first-clear:"))>=config.Operations.SecondFreeDispatchClears?2:1;
 }
}
