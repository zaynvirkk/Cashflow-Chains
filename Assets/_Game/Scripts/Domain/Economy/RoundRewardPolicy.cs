using System;
using System.Linq;
using CashflowChains.Domain;

namespace CashflowChains.Economy {
 public sealed class RoundRewardQuote {
  public readonly int Level,Stars;public readonly long RoundScore;public readonly ItemAmount[] Items;public readonly string ProofHash;
  internal RoundRewardQuote(int level,int stars,long score,ItemAmount[] items,string hash){Level=level;Stars=stars;RoundScore=score;Items=items;ProofHash=hash;}
 }
 public static class RoundRewardPolicy {
  public static RoundRewardQuote VerifyCertified(BalanceConfig config,int level,Board submitted){
   if(level<0||level>=104||submitted==null||submitted.DraftMode||submitted.DraftRoll!=0)throw new ArgumentException("Not a certified proof");var run=new CityRun(level,submitted.Copy());var result=run.Preview();if(!result.Conserved())throw new InvalidOperationException("Unconserved result");int stars=run.Stars(result.Credits);var items=stars==0?new ItemAmount[0]:level<3?new[]{new ItemAmount("CASH",config.FirstCash[level]),new ItemAmount("WOOD",config.FirstWood[level])}:new[]{new ItemAmount("CASH",80),new ItemAmount("WOOD",8)};if(stars>0&&level==0)items=items.Concat(new[]{new ItemAmount("CHEST:bronze",1)}).ToArray();
   // Includes every mission constraint and the exact trace. Service also checks issued challenge/account/ownership and first-clear status in its durable transaction.
   string hash=CurrencyLedger.Hash(config.Revision+"|"+level+"|"+submitted.Fingerprint()+"|"+submitted.Blocked+"|"+result.Trace());return new RoundRewardQuote(level,stars,result.Credits,items,hash);
  }
  public static CommandReceipt ClaimFirstClear(CurrencyLedger ledger,string issuedAccount,int level,Board submitted,BalanceConfig config,long trustedNow,Func<EconomyState,bool> persist){
   var access=ledger.Snapshot;if(access.AccountId!=issuedAccount)throw new ArgumentException("Wrong account");int cleared=access.Receipts.Count(x=>x.CommandId.StartsWith("certified-first-clear:"));if(level>=72||level>Math.Min(71,cleared+2))throw new InvalidOperationException("Challenge not unlocked or entitlement unverified");var quote=VerifyCertified(config,level,submitted);if(quote.Stars<1)throw new InvalidOperationException("No successful chain");
   // Business key is independent of client request id/layout: a second winning layout cannot farm first-clear resources.
   string key="certified-first-clear:"+level;var prior=ledger.Snapshot.Receipts.Find(x=>x.CommandId==key);if(prior!=null)return prior;
   return ledger.Execute(key,"level="+level,"campaign-v1","first-clear",trustedNow,t=>{foreach(var item in quote.Items)t.Grant(item.ItemId,item.Quantity);},persist);
  }
 }
}
