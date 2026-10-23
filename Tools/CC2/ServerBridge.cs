using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using CashflowChains.Economy;
using CashflowChains.Empire;
using CashflowChains.Domain;
internal static class ServerBridge {
 public sealed class Request {public string AccountId,Command,RequestId,BusinessId,ProjectId,Template,NodeId,SourceItem;public long TrustedNow;public int Level,Minutes;public Board Board;public EmpireAccount State;}
 internal static void Run(BalanceConfig config,string file){var request=JsonSerializer.Deserialize<Request>(File.ReadAllText(file),Program.Json);if(string.IsNullOrEmpty(request.AccountId)||request.TrustedNow<1)throw new ArgumentException("Missing server context");
  if(request.Command=="create"){Console.WriteLine(JsonSerializer.Serialize(new EmpireAccount{Economy=new EconomyState{AccountId=request.AccountId}},Program.Json));return;}
  if(request.State==null||request.State.Economy.AccountId!=request.AccountId)throw new ArgumentException("Wrong persisted account");var engine=new EmpireEngine(config,request.State,_=>true);
  if(request.Command=="development-quote"){
   var q=DevelopmentPlanning.Inspect(config,engine.Snapshot,request.BusinessId);
   string action=q.Action==DevelopmentAction.TierLimit?"tier-limit":q.Action==DevelopmentAction.BuilderBusy?"builder-busy":q.Action==DevelopmentAction.FindResources?"find-resources":q.Action.ToString().ToLowerInvariant();
   Console.WriteLine(JsonSerializer.Serialize(new{businessId=q.BusinessId,level=q.Level,nextLevel=q.NextLevel==0?(int?)null:q.NextLevel,primaryAction=action,affordable=q.Affordable,beforeHourly=q.BeforeHourly,afterHourly=q.AfterHourly,output=q.Output,seconds=q.Seconds,storage=q.Storage,requirements=q.Requirements.Select(r=>new{itemId=r.ItemId,required=r.Required,owned=r.Owned,missing=r.Missing}).ToArray()},Program.Json));return;
  }
  switch(request.Command){case "round":engine.RecordCertified(request.Level,request.Board,request.TrustedNow);break;case "build":engine.StartConstruction(request.RequestId,request.BusinessId,request.TrustedNow);break;case "finish":engine.FinishConstruction(request.RequestId,request.ProjectId,request.TrustedNow);break;case "collect":engine.CollectAll(request.RequestId,request.TrustedNow);break;case "dispatch":engine.StartDispatch(request.RequestId,request.Template,request.Board,request.TrustedNow);break;case "dispatch-claim":engine.ClaimDispatch(request.ProjectId,request.TrustedNow);break;case "research-start":engine.StartResearch(request.RequestId,request.NodeId,request.TrustedNow);break;case "research-finish":engine.FinishResearch(request.ProjectId,request.TrustedNow);break;case "speedup":engine.SpendSpeedup(request.RequestId,request.ProjectId,request.SourceItem,request.Minutes,request.TrustedNow);break;default:throw new ArgumentException("Unsupported command");}
  Console.WriteLine(JsonSerializer.Serialize(engine.Snapshot,Program.Json));
 }
}
