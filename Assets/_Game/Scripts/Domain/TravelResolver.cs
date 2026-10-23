using System;
using System.Collections.Generic;
using System.Linq;

namespace CashflowChains.Domain {
 public static class RoadRouting {
  public const int DayMinutes=24;
  public static int OutputKind(Piece p)=>p.SourceKind>=0?p.SourceKind:Array.FindIndex(p.Make,n=>n>0);
  public static bool CanDeliver(Node source,Node target){if(source==null||target==null||source.Id==target.Id)return false;int kind=OutputKind(Pieces.Get(source.Kind));return kind>=0&&Pieces.Get(target.Kind).Need[kind]>0;}
  public static int Blocks(Node a,Node b)=>Math.Abs(a.X-b.X)+Math.Abs(a.Y-b.Y);
  public static int Minutes(Node a,Node b){int speed=a.Kind=="logistics"||a.Kind=="freight_yard"?3:a.Kind=="warehouse"||a.Kind=="tram"?2:1;return Math.Max(1,(Blocks(a,b)+speed-1)/speed);}
  public static bool IsRelay(Piece p,int kind)=>p.SourceKind<0&&p.Credits==0&&p.Need[kind]>0&&p.Need.Sum()==p.Need[kind]&&p.Make[kind]==p.Need[kind];
  public static Node[] Targets(Board board,Node source){int kind=OutputKind(Pieces.Get(source.Kind));if(kind<0)return new Node[0];var targets=board.Nodes.Where(t=>CanDeliver(source,t));if(source.TargetId!=0)return targets.Where(t=>t.Id==source.TargetId).ToArray();if(IsRelay(Pieces.Get(source.Kind),kind))targets=targets.Where(t=>!IsRelay(Pieces.Get(t.Kind),kind));return targets.OrderBy(t=>Blocks(source,t)).ThenBy(t=>t.Id).ToArray();}
 }

 public static class TravelResolver {
  sealed class Trip {public int Source,Target,Kind,Count,Due,Minutes;}
  public static RoundResult Run(Board board,int basisPoints){
   var nodes=board.Nodes.OrderBy(n=>n.Id).ToArray();int len=nodes.Length;var pieces=nodes.Select(n=>Pieces.Get(n.Kind)).ToArray();var stock=new int[len,3];var output=new int[len,3];var pending=new int[len,3];var trips=new List<Trip>();var carry=new int[len];var r=new RoundResult{Beats=RoadRouting.DayMinutes};int imported=0;
   var targets=nodes.Select(n=>RoadRouting.Targets(board,n).Select(t=>Array.FindIndex(nodes,k=>k.Id==t.Id)).ToArray()).ToArray();
   for(int i=0;i<len;i++){stock[i,1]=nodes[i].InitialGoods;stock[i,2]=nodes[i].InitialEnergy;r.Initial[1]+=stock[i,1];r.Initial[2]+=stock[i,2];}
   for(int minute=1;minute<=r.Beats;minute++){
    foreach(var trip in trips.Where(t=>t.Due==minute).ToArray()){pending[trip.Target,trip.Kind]-=trip.Count;Add(stock,trip.Target,trip.Kind,trip.Count,r);r.Events.Add(new FlowEvent{Beat=minute,Source=nodes[trip.Source].Id,Target=nodes[trip.Target].Id,Kind=trip.Kind,Count=trip.Count,Reason="arrival"});trips.Remove(trip);}
    for(int i=0;i<len;i++){
     var p=pieces[i];if(p.SourceKind<0)continue;int amount=p.SourceRate;
     if(p.Id=="home"||p.Id=="apartment"){int parks=nodes.Count(n=>n.Kind=="park"&&RoadRouting.Blocks(n,nodes[i])==1);carry[i]+=amount*(10000+parks*1500);amount=carry[i]/10000;carry[i]%=10000;}
     if(p.Id=="port"){if(board.ImportQuota>0)amount=Math.Min(amount,Math.Max(0,board.ImportQuota-imported));imported+=amount;long cost=amount*(board.ImportCost==0?2L:board.ImportCost);r.Credits-=cost;r.Events.Add(new FlowEvent{Beat=minute,Source=nodes[i].Id,Target=nodes[i].Id,Count=amount,Credits=-cost,Reason="import"});}
     r.Generated[p.SourceKind]+=amount;Add(output,i,p.SourceKind,amount,r);
    }
    // Reserve all in-flight stock as well as on-hand inventory. A distant shop
    // cannot demand the same units again on every minute of the journey.
    for(int i=0;i<len;i++){
     int capacity=pieces[i].Id=="freight_yard"?10:6;
     foreach(int target in targets[i])for(int kind=0;kind<3;kind++){
      int demand=pieces[target].Need[kind]*Math.Max(1,pieces[target].Rate)*2-stock[target,kind]-pending[target,kind];int count=Math.Min(Math.Min(output[i,kind],capacity),Math.Max(0,demand));if(count==0)continue;
      int duration=RoadRouting.Minutes(nodes[i],nodes[target]);output[i,kind]-=count;pending[target,kind]+=count;capacity-=count;trips.Add(new Trip{Source=i,Target=target,Kind=kind,Count=count,Due=minute+duration,Minutes=duration});r.Events.Add(new FlowEvent{Beat=minute,Source=nodes[i].Id,Target=nodes[target].Id,Kind=kind,Count=count,TravelSteps=duration,ArrivalBeat=minute+duration,Reason="shipment"});
     }
    }
    // Arrival first,then recipes. Converter output departs on the next minute.
    for(int i=0;i<len;i++){
     var p=pieces[i];int operations=p.Rate;if(operations==0)continue;for(int k=0;k<3;k++)if(p.Need[k]>0)operations=Math.Min(operations,stock[i,k]/p.Need[k]);if(operations==0)continue;
     for(int k=0;k<3;k++){int used=operations*p.Need[k],made=operations*p.Make[k];stock[i,k]-=used;r.Consumed[k]+=used;r.Generated[k]+=made;Add(output,i,k,made,r);}
     if(p.Credits<=0)continue;var near=nodes.Where(n=>RoadRouting.Blocks(n,nodes[i])==1).ToArray();bool bank=near.Any(n=>n.Kind=="bank"&&nodes.Where(z=>Pieces.Get(z.Kind).Credits>0&&RoadRouting.Blocks(z,n)==1).OrderBy(z=>z.Id).First().Id==nodes[i].Id);int bp=Math.Min(30000,10000+(bank?2500:0)+near.Count(n=>n.Kind=="office"||n.Kind=="tech_office")*1000+(p.Id=="cafe"?near.Count(n=>n.Kind=="park")*500:0));long money=checked(checked(operations*p.Credits)*bp/10000*basisPoints/10000);r.Credits=checked(r.Credits+money);r.Events.Add(new FlowEvent{Beat=minute,Source=nodes[i].Id,Target=nodes[i].Id,Count=operations,Credits=money,Reason="sale"});
    }
   }
   if(nodes.Any(n=>n.Kind=="landmark")&&r.Events.Where(e=>e.Reason=="sale").Select(e=>nodes.First(n=>n.Id==e.Source).Kind).Distinct().Count()>=3){long bonus=r.Events.Where(e=>e.Reason=="sale").Sum(e=>e.Credits)/4;r.Credits+=bonus;r.Events.Add(new FlowEvent{Beat=r.Beats,Credits=bonus,Reason="diversity"});}
   foreach(var trip in trips)r.InTransit[trip.Kind]+=trip.Count;
   for(int k=0;k<3;k++){r.Remaining[k]=r.InTransit[k];for(int i=0;i<len;i++)r.Remaining[k]+=stock[i,k]+output[i,k];}
   if(!r.Conserved())throw new InvalidOperationException("Travel conservation failed");return r;
  }
  static void Add(int[,] store,int node,int kind,int count,RoundResult r){int total=store[node,kind]+count;store[node,kind]=Math.Min(64,total);r.Spilled[kind]+=Math.Max(0,total-64);}
 }

 public static class TravelCampaign {
  public static Board Convert(Board board,int version=1){if(version!=1)throw new ArgumentException("Unsupported city conversion");var b=board.Copy();b.RulesVersion=1;foreach(var n in b.Nodes){if(n.Kind=="farm")n.InitialEnergy=0;if(b.Challenge==0&&n.Kind=="cafe"&&n.Id==2&&n.InitialGoods==14)n.InitialGoods=48;}return b;}
  public static Mission Get(int level){var legacy=Campaign.Get(level,0);var solution=Convert(legacy.Solution);var start=Convert(legacy.Start);if(level==1){var farm=start.Nodes.First(n=>n.Kind=="farm");farm.X=0;farm.Y=0;}if(level==2){var home=start.Nodes.First(n=>n.Kind=="home");home.X=0;home.Y=0;var farm=start.Nodes.First(n=>n.Kind=="farm");farm.X=3;farm.Y=0;}
   long par=Resolver.Run(solution,legacy.Multiplier).Credits;if(par<=0)throw new InvalidOperationException("No viable travel witness "+level);
   return new Mission{Index=level,Title=level==0?"A shorter walk":level==1?"Fresh deliveries":level==2?"A well-run café":legacy.Title,District=legacy.District,Brief="Homes send customers. Farms send stock. Shops sell after arrivals. Shorter journeys leave more time to trade.",Modifier="24-minute trading day",Multiplier=legacy.Multiplier,Solution=solution,Start=start,Par=par,Target=Math.Max(80,par*(level<3?85:65)/100)};
  }
 }
}
