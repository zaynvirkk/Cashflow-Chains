using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CashflowChains.Economy {
 public enum BalanceOrigin { Earned, Purchased, ImportEscrow }
 [Serializable] public sealed class Balance {public string ItemId;public long Earned,Purchased,ImportEscrow;public Balance Copy()=>(Balance)MemberwiseClone();}
 [Serializable] public sealed class LedgerEntry {
  public string EntryId,AccountId,ItemId,Reason,SourceId,CorrelationId,ConfigVersion;public long Delta,CreatedAt;public BalanceOrigin Origin;
  public LedgerEntry Copy()=>(LedgerEntry)MemberwiseClone();
 }
 [Serializable] public sealed class CommandReceipt {
  public string CommandId,Fingerprint,Result;public int ConfigRevision;public long Revision;public CommandReceipt Copy()=>(CommandReceipt)MemberwiseClone();
 }
 [Serializable] public sealed class EconomyState {
  public int Schema=1;public string AccountId;public long Revision,LastTrustedAt;
  public List<Balance> Balances=new List<Balance>();public List<LedgerEntry> Ledger=new List<LedgerEntry>();public List<CommandReceipt> Receipts=new List<CommandReceipt>();
  public EconomyState Copy()=>new EconomyState{Schema=Schema,AccountId=AccountId,Revision=Revision,LastTrustedAt=LastTrustedAt,Balances=Balances.Select(x=>x.Copy()).ToList(),Ledger=Ledger.Select(x=>x.Copy()).ToList(),Receipts=Receipts.Select(x=>x.Copy()).ToList()};
  public long Amount(string id)=>Balances.Where(x=>x.ItemId==id).Sum(x=>checked(x.Earned+x.Purchased));
  public void Validate(BalanceConfig config){
   if(Schema!=1||string.IsNullOrEmpty(AccountId)||AccountId.Length>100||Revision<0||LastTrustedAt<0||Balances==null||Ledger==null||Receipts==null||Balances.Any(x=>x==null)||Ledger.Any(x=>x==null)||Receipts.Any(x=>x==null))throw new ArgumentException("Invalid economy state");
   if(Balances.Select(x=>x.ItemId).Distinct().Count()!=Balances.Count||Ledger.Select(x=>x.EntryId).Distinct().Count()!=Ledger.Count||Receipts.Select(x=>x.CommandId).Distinct().Count()!=Receipts.Count||Revision!=Receipts.Count)throw new ArgumentException("Duplicate or incomplete ledger");
   foreach(var b in Balances){if(b.Earned<0||b.Purchased<0||b.ImportEscrow<0||checked(b.Earned+b.Purchased+b.ImportEscrow)>config.Item(b.ItemId).MaxStack)throw new ArgumentException("Invalid balance");foreach(BalanceOrigin origin in Enum.GetValues(typeof(BalanceOrigin))){long total=0;foreach(var e in Ledger.Where(x=>x.ItemId==b.ItemId&&x.Origin==origin))total=checked(total+e.Delta);long value=origin==BalanceOrigin.Earned?b.Earned:origin==BalanceOrigin.Purchased?b.Purchased:b.ImportEscrow;if(total!=value)throw new ArgumentException("Ledger does not reconcile");}}
   foreach(var e in Ledger)if(e.AccountId!=AccountId||e.Delta==0||e.CreatedAt<0||e.CreatedAt>LastTrustedAt||string.IsNullOrEmpty(e.EntryId)||string.IsNullOrEmpty(e.SourceId)||string.IsNullOrEmpty(e.ConfigVersion)||!Receipts.Any(r=>r.CommandId==e.CorrelationId)||!Balances.Any(b=>b.ItemId==e.ItemId)||!Enum.IsDefined(typeof(BalanceOrigin),e.Origin))throw new ArgumentException("Invalid ledger entry");
   for(int i=0;i<Receipts.Count;i++)if(Receipts[i].Revision!=i+1||string.IsNullOrEmpty(Receipts[i].Fingerprint))throw new ArgumentException("Invalid command receipt");
  }
 }
 public sealed class EconomyTransaction {
  readonly EconomyState state;readonly BalanceConfig config;readonly string command,source,reason;readonly long now;int ordinal;
  internal EconomyTransaction(EconomyState state,BalanceConfig config,string command,string source,string reason,long now){this.state=state;this.config=config;this.command=command;this.source=source;this.reason=reason;this.now=now;}
  Balance Get(string id){config.Item(id);var b=state.Balances.Find(x=>x.ItemId==id);if(b==null){b=new Balance{ItemId=id};state.Balances.Add(b);}return b;}
  void Change(string id,long delta,BalanceOrigin origin){if(delta==0)return;var b=Get(id);long value=origin==BalanceOrigin.Earned?b.Earned:origin==BalanceOrigin.Purchased?b.Purchased:b.ImportEscrow;value=checked(value+delta);if(value<0)throw new InvalidOperationException("Insufficient "+id);if(origin==BalanceOrigin.Earned)b.Earned=value;else if(origin==BalanceOrigin.Purchased)b.Purchased=value;else b.ImportEscrow=value;if(checked(b.Earned+b.Purchased+b.ImportEscrow)>config.Item(id).MaxStack)throw new InvalidOperationException("Storage cap "+id);state.Ledger.Add(new LedgerEntry{EntryId=command+":"+(ordinal++),AccountId=state.AccountId,ItemId=id,Delta=delta,Origin=origin,Reason=reason,SourceId=source,CorrelationId=command,ConfigVersion=config.Revision.ToString(),CreatedAt=now});}
  public void Grant(string id,long amount,BalanceOrigin origin=BalanceOrigin.Earned){if(amount<1)throw new ArgumentException("Nonpositive grant");Change(id,amount,origin);}
  public void Spend(string id,long amount){if(amount<1)throw new ArgumentException("Nonpositive spend");var b=Get(id);if(checked(b.Earned+b.Purchased)<amount)throw new InvalidOperationException("Insufficient "+id);long earned=Math.Min(amount,b.Earned);Change(id,-earned,BalanceOrigin.Earned);Change(id,-(amount-earned),BalanceOrigin.Purchased);}
  public long Available(string id)=>state.Amount(id);
 }
 // Trusted service/domain primitive. Not a client-facing grant API. A durable service must persist the candidate and its projections in one DB transaction.
 public sealed class CurrencyLedger {
  readonly object gate=new object();readonly BalanceConfig config;EconomyState state;
  public CurrencyLedger(BalanceConfig config,EconomyState state){config.Validate();state.Validate(config);this.config=config;this.state=state.Copy();}
  public EconomyState Snapshot {get{lock(gate)return state.Copy();}}
  public static string Hash(string value){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-","").ToLowerInvariant();}
  public CommandReceipt Execute(string id,string payload,string source,string reason,long trustedNow,Action<EconomyTransaction> command,Func<EconomyState,bool> persist){
   lock(gate){if(string.IsNullOrWhiteSpace(id)||id.Length>160||string.IsNullOrEmpty(source)||string.IsNullOrEmpty(reason)||trustedNow<state.LastTrustedAt||command==null||persist==null)throw new ArgumentException("Invalid command context");var prior=state.Receipts.Find(x=>x.CommandId==id);int revision=prior==null?config.Revision:Math.Max(1,prior.ConfigRevision);string fingerprint=Hash(revision+"|"+source+"|"+reason+"|"+payload);if(prior!=null){if(prior.Fingerprint!=fingerprint)throw new InvalidOperationException("Idempotency key reused with another payload");return prior.Copy();}
    var candidate=state.Copy();candidate.LastTrustedAt=trustedNow;command(new EconomyTransaction(candidate,config,id,source,reason,trustedNow));var receipt=new CommandReceipt{CommandId=id,Fingerprint=fingerprint,ConfigRevision=config.Revision,Result="committed",Revision=checked(candidate.Revision+1)};candidate.Revision=receipt.Revision;candidate.Receipts.Add(receipt);candidate.Validate(config);if(!persist(candidate.Copy()))throw new InvalidOperationException("Durable commit failed");state=candidate;return receipt.Copy();
   }
  }
 }
}
