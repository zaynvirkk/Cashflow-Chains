using System;
using System.Collections.Generic;
using System.Linq;
using CashflowChains.Economy;
using CashflowChains.Domain;

namespace CashflowChains.Empire {
 public enum EmpireEventKind { RoundVerified, ConstructionStarted, ConstructionCompleted, ProductionCollected, DispatchStarted, DispatchClaimed, ResearchStarted, ResearchCompleted, SpeedupSpent }
 [Serializable] public sealed class EmpireEvent {public string Id,EntityId;public EmpireEventKind Kind;public long At,Quantity;public EmpireEvent Copy()=>(EmpireEvent)MemberwiseClone();}
 [Serializable] public sealed class OwnedBusiness {
  public string Id;public int Level,Stars,Experience;public long ProducedAt,Carry;public OwnedBusiness Copy()=>(OwnedBusiness)MemberwiseClone();
 }
 [Serializable] public sealed class ConstructionProject {
  public string Id,BusinessId;public int TargetLevel,Slot,ReductionBp,SpeedupSeconds;public long StartedAt,FinishAt;public ConstructionProject Copy()=>(ConstructionProject)MemberwiseClone();
 }
 [Serializable] public sealed partial class EmpireAccount {
  public int Schema=3,Builders=1,ModelRevision=2;public EconomyState Economy;public List<OwnedBusiness> Businesses=new List<OwnedBusiness>();public List<ConstructionProject> Construction=new List<ConstructionProject>();public List<EmpireEvent> Events=new List<EmpireEvent>();
  public EmpireAccount Copy()=>new EmpireAccount{Schema=Schema,Builders=Builders,ModelRevision=ModelRevision,Dispatches=Dispatches.Select(x=>x.Copy()).ToList(),Research=new List<string>(Research),ResearchQueue=ResearchQueue.Select(x=>x.Copy()).ToList(),Economy=Economy.Copy(),Businesses=Businesses.Select(x=>x.Copy()).ToList(),Construction=Construction.Select(x=>x.Copy()).ToList(),Events=Events.Select(x=>x.Copy()).ToList()};
  public void Validate(BalanceConfig config){
   if(ModelRevision<2){Dispatches=Dispatches??new List<DispatchOperation>();Research=Research??new List<string>();ResearchQueue=ResearchQueue??new List<ResearchProject>();ModelRevision=2;}if(ModelRevision!=2)throw new ArgumentException("Unknown empire model revision");
   if(Schema!=3||Builders!=1||Economy==null||Businesses==null||Construction==null||Events==null||Businesses.Any(x=>x==null)||Construction.Any(x=>x==null)||Events.Any(x=>x==null))throw new ArgumentException("Invalid empire state");Economy.Validate(config);
   if(Businesses.Select(x=>x.Id).Distinct().Count()!=Businesses.Count||Construction.Count>Builders||Construction.Select(x=>x.BusinessId).Distinct().Count()!=Construction.Count||Construction.Select(x=>x.Slot).Distinct().Count()!=Construction.Count||Events.Select(x=>x.Id).Distinct().Count()!=Events.Count)throw new ArgumentException("Duplicate empire projection");
   foreach(var b in Businesses){config.Business(b.Id);if(b.Level<0||b.Level>config.Upgrades.Length||b.Stars<0||b.Stars>5||b.Experience<0||b.ProducedAt<0||b.ProducedAt>Economy.LastTrustedAt||b.Carry<0||b.Carry>=36000000)throw new ArgumentException("Invalid business projection");}
   foreach(var p in Construction){var owned=Businesses.Find(x=>x.Id==p.BusinessId);if(owned==null||p.TargetLevel!=owned.Level+1||p.Slot<0||p.Slot>=Builders||p.StartedAt<0||p.FinishAt<p.StartedAt||p.SpeedupSeconds<0||p.ReductionBp<0||p.ReductionBp>5000||p.FinishAt-p.StartedAt!=EconomyMath.Duration(config.Upgrade(p.TargetLevel).Seconds,p.ReductionBp)-p.SpeedupSeconds||p.StartedAt>Economy.LastTrustedAt||!Economy.Receipts.Any(x=>x.CommandId==p.Id))throw new ArgumentException("Invalid construction projection");}
   ValidateOperations(config);
   foreach(var e in Events)if(e.At<0||e.At>Economy.LastTrustedAt||!Enum.IsDefined(typeof(EmpireEventKind),e.Kind)||string.IsNullOrEmpty(e.Id))throw new ArgumentException("Invalid event projection");
  }
 }
 public sealed class ProductionQuote {public string ItemId;public long Amount,Carry,Elapsed;}
 public static class ProductionPolicy {
  public static long ScaledHourly(BalanceConfig config,string businessId,int level,int productionBp=0){if(level<0||level>config.Upgrades.Length||productionBp<0)throw new ArgumentException("Invalid production tier or bonus");var def=config.Business(businessId);return level==0?0:checked(def.BaseHourly*(10000L+Math.Min((level-1)*800+productionBp,config.ProductionBonusCapBp)));}
  public static ProductionQuote Quote(BalanceConfig config,OwnedBusiness b,long now,int productionBp=0,int storageBp=0){
   if(now<b.ProducedAt)throw new ArgumentException("Clock moved backwards");var def=config.Business(b.Id);long elapsed=Math.Min(config.OfflineCapSeconds,now-b.ProducedAt);long numerator=checked(ScaledHourly(config,b.Id,b.Level,productionBp)*elapsed+b.Carry);long storage=EconomyMath.WithBonus(def.Storage,storageBp,10000);long amount=Math.Min(storage,numerator/36000000);
   return new ProductionQuote{ItemId=def.Output,Amount=amount,Carry=amount==storage?0:numerator%36000000,Elapsed=elapsed};
  }
 }
 // Local sandbox and trusted service share commands. A production transport must bind account/auth/trusted time and persist atomically.
 public sealed partial class EmpireEngine {
  readonly object gate=new object();readonly BalanceConfig config;EmpireAccount state;readonly Func<EmpireAccount,bool> persist;
  public EmpireEngine(BalanceConfig config,EmpireAccount state,Func<EmpireAccount,bool> persist){config.Validate();state.Validate(config);this.config=config;this.state=state.Copy();this.persist=persist??throw new ArgumentNullException(nameof(persist));}
  public EmpireAccount Snapshot{get{lock(gate)return state.Copy();}}
  int Cleared(EmpireAccount a)=>a.Economy.Receipts.Count(x=>x.CommandId.StartsWith("certified-first-clear:"));
  CommandReceipt Command(string id,string payload,string source,string reason,long now,Action<EmpireAccount,EconomyTransaction> action){
   lock(gate){var candidate=state.Copy();var ledger=new CurrencyLedger(config,candidate.Economy);var r=ledger.Execute(id,payload,source,reason,now,t=>action(candidate,t),economy=>{candidate.Economy=economy;candidate.Validate(config);return persist(candidate.Copy());});candidate.Economy=ledger.Snapshot;state=candidate;return r;}
  }
  static void Event(EmpireAccount a,string id,EmpireEventKind kind,string entity,long now,long quantity)=>a.Events.Add(new EmpireEvent{Id=id+":"+kind,Kind=kind,EntityId=entity,At=now,Quantity=quantity});
  public CommandReceipt RecordCertified(int level,Board board,long now){
   lock(gate){var quote=RoundRewardPolicy.VerifyCertified(config,level,board);if(quote.Stars<1)throw new InvalidOperationException("No successful chain");if(level>=72||level>Math.Min(71,Cleared(state)+2))throw new InvalidOperationException("Challenge unavailable");string key="certified-first-clear:"+level;
    return Command(key,"level="+level,"campaign-v1","first-clear",now,(a,t)=>{foreach(var item in quote.Items)t.Grant(item.ItemId,item.Quantity);var contributors=CityCollections.Contributors(board,Resolver.Run(board,Campaign.Get(level).Multiplier));foreach(var b in a.Businesses)if(board.Nodes.Any(n=>n.Kind==b.Id&&contributors.Contains(n.Id)))b.Experience=checked(b.Experience+quote.Stars*10);Event(a,key,EmpireEventKind.RoundVerified,level.ToString(),now,quote.RoundScore);});
   }
  }
  public CommandReceipt StartConstruction(string id,string business,long now){return Command(id,business,"construction","build-cost",now,(a,t)=>{
   var def=config.Business(business);if(Cleared(a)<def.UnlockClears)throw new InvalidOperationException("Complete more certified cities");if(a.Construction.Count>=a.Builders||a.Construction.Any(x=>x.BusinessId==business))throw new InvalidOperationException("Builder occupied");var owned=a.Businesses.Find(x=>x.Id==business);if(owned==null){owned=new OwnedBusiness{Id=business,ProducedAt=now};a.Businesses.Add(owned);}var upgrade=config.Upgrade(owned.Level+1);foreach(var cost in upgrade.Costs)t.Spend(cost.ItemId,cost.Quantity);int slot=Enumerable.Range(0,a.Builders).First(x=>!a.Construction.Any(p=>p.Slot==x));a.Construction.Add(new ConstructionProject{Id=id,BusinessId=business,TargetLevel=upgrade.Level,Slot=slot,StartedAt=now,ReductionBp=EmpireBenefits.Bonus(config,a,"build-time-bp"),FinishAt=checked(now+EconomyMath.Duration(upgrade.Seconds,EmpireBenefits.Bonus(config,a,"build-time-bp")))});Event(a,id,EmpireEventKind.ConstructionStarted,business,now,upgrade.Level);
  });}
  public CommandReceipt FinishConstruction(string id,string projectId,long now){return Command(id,projectId,"construction","build-complete",now,(a,t)=>{
   var project=a.Construction.Find(x=>x.Id==projectId)??throw new InvalidOperationException("Project unavailable");if(now<project.FinishAt)throw new InvalidOperationException("Construction is not finished");var b=a.Businesses.Single(x=>x.Id==project.BusinessId);
   // Settle old-rate output only through the real completion boundary. The next collection accrues the new rate.
   long horizon=Math.Max(b.ProducedAt,now-config.OfflineCapSeconds);
   if(b.Level>0&&project.FinishAt>horizon){var snapshot=b.Copy();snapshot.ProducedAt=horizon;var old=ProductionPolicy.Quote(config,snapshot,project.FinishAt,EmpireBenefits.Bonus(config,a,"production-bp"),EmpireBenefits.Bonus(config,a,"storage-bp"));if(old.Amount>0)t.Grant(old.ItemId,old.Amount);b.Carry=old.Carry;}
   b.Level=project.TargetLevel;b.ProducedAt=Math.Max(project.FinishAt,horizon);a.Construction.Remove(project);Event(a,id,EmpireEventKind.ConstructionCompleted,b.Id,now,b.Level);
  });}
  public CommandReceipt CollectAll(string id,long now){return Command(id,"all","production","collect",now,(a,t)=>{
   long total=0;foreach(var b in a.Businesses.Where(x=>x.Level>0)){var project=a.Construction.Find(x=>x.BusinessId==b.Id);long through=project!=null&&project.FinishAt<now?project.FinishAt:now;if(through<b.ProducedAt)continue;var q=ProductionPolicy.Quote(config,b,through,EmpireBenefits.Bonus(config,a,"production-bp"),EmpireBenefits.Bonus(config,a,"storage-bp"));if(q.Amount>0){t.Grant(q.ItemId,q.Amount);total=checked(total+q.Amount);}b.ProducedAt=through;b.Carry=q.Carry;}Event(a,id,EmpireEventKind.ProductionCollected,"all",now,total);
  });}
 }
}
