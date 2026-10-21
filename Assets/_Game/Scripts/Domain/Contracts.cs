using System;
using System.Collections.Generic;
using System.Linq;
using LocalGame.Data;
namespace CashflowChains.Domain {
 [Serializable] public sealed class RouteContract {public int Template;public string Id;public long StartTicks,EndTicks;public Board Layout;public bool Claimed;public long Score;}
 public static class ContractDesk {
  public static readonly string[] Names={"Morning deliveries","Neighbourhood order","Coffee circuit","Garden supplies","The local exchange","Evening customers","Freight appointment","Factory dispatch","Night market","Banking hours","Hotel service","The long route","District festival","Harbour exchange","Metropolis commission"};
  public static int Duration(int template)=>(template+1)*60;public static long Reward(int template)=>50+template*10;
  public static bool Send(List<RouteContract> contracts,int template,CityRun run,Profile profile,long now) {
   if(template<0||template>=15||template>profile.Cleared/3||contracts.Any(c=>c.Template==template)||now<=0||contracts.Count(c=>!c.Claimed)>=3)return false;
   var result=run.Preview();if(result.Credits<run.Mission.Target||profile.Stars[run.Mission.Index]==0)return false;
   contracts.Add(new RouteContract{Template=template,Id="route-"+template,StartTicks=now,EndTicks=checked(now+TimeSpan.FromSeconds(Duration(template)).Ticks),Layout=run.Board.Copy(),Score=result.Credits});return true;
  }
  public static bool Claim(RouteContract contract,Profile profile,long now) {
   if(contract.Claimed||now<contract.EndTicks)return false;var replay=new CityRun(contract.Layout.Challenge,contract.Layout).Preview();if(replay.Credits!=contract.Score)throw new InvalidOperationException("Contract replay changed");
   profile.Grant(contract.Id,Reward(contract.Template));contract.Claimed=true;return true;
  }
  public static void Validate(List<RouteContract> contracts) {
   if(contracts==null||contracts.Count>15||contracts.Select(c=>c.Template).Distinct().Count()!=contracts.Count)throw new ArgumentException("Invalid contracts");
   foreach(var c in contracts){if(c==null||c.Template<0||c.Template>=15||c.Id!="route-"+c.Template||c.StartTicks<=0||c.EndTicks-c.StartTicks!=TimeSpan.FromSeconds(Duration(c.Template)).Ticks||c.Score!=new CityRun(c.Layout.Challenge,c.Layout).Preview().Credits)throw new ArgumentException("Invalid route");}
  }
 }
 public static class Milestones {
  static readonly int[] Clears={1,2,3,5,8,12,18,24,30,36,42,48,54,60,72};
  static readonly int[] Stars={3,6,9,15,24,36,48,60,72,90,108,132,156,180,216};
  public static string Name(int index)=>index<15?"City builder "+(index+1)+" · "+Clears[index]+" cities":"The perfect rhythm "+(index-14)+" · "+Stars[index-15]+" stars";
  public static bool Earned(Profile profile,int index)=>index<15?profile.Cleared>=Clears[index]:profile.TotalStars>=Stars[index-15];
 }
}
