using System;using System.Collections.Generic;using System.Linq;
namespace CashflowChains.Domain {
 public sealed class CityInsight {
  public long Before,After;public long Delta=>After-Before;public string Title,Detail;public bool Improved=>Delta>0;
  public static CityInsight Compare(Board before,Board after,int multiplier,int selected){
   var prior=Resolver.Run(before,multiplier);var next=Resolver.Run(after,multiplier);var insight=new CityInsight{Before=prior.Credits,After=next.Credits};var node=after.Nodes.Find(n=>n.Id==selected);var newLinks=next.Events.Where(e=>e.Reason=="shipment"&&!prior.Events.Any(p=>p.Reason=="shipment"&&p.Source==e.Source&&p.Target==e.Target&&p.Kind==e.Kind)).ToArray();
   if(insight.Delta>0){var link=newLinks.FirstOrDefault(e=>e.Kind==1)??newLinks.FirstOrDefault();insight.Title=after.RulesVersion==1?"EARLIER ARRIVALS":link==null?"SMARTER CITY":link.Kind==0?"CUSTOMERS CONNECTED":link.Kind==1?"SUPPLIES CONNECTED":"POWER CONNECTED";insight.Detail="+"+insight.Delta.ToString("N0")+" forecast. "+(after.RulesVersion==1?TravelInsight.Describe(after,next,selected):link==null?Describe(after,next,selected):link.Kind==0?"Residents can now visit the "+Label(after,link.Target)+".":Label(after,link.Source)+" now delivers "+Resource(link.Kind)+" to the "+Label(after,link.Target)+".");}
   else if(insight.Delta<0){insight.Title="CONNECTION CHANGED";insight.Detail=insight.Delta.ToString("N0")+" forecast. "+Describe(after,next,selected)+" Undo is free.";}
   else{insight.Title="TRY A CONNECTION";insight.Detail=Describe(after,next,selected);}
   return insight;
  }
  static string Label(Board board,int id){var n=board.Nodes.Find(x=>x.Id==id);return n==null?"the city":Pieces.Get(n.Kind).Label.ToLowerInvariant();}
  public static string Describe(Board board,RoundResult forecast,int selected){
   if(board.RulesVersion==1)return TravelInsight.Describe(board,forecast,selected);
   var n=board.Nodes.Find(z=>z.Id==selected);if(n==null)return "Choose a building to see how it works.";var p=Pieces.Get(n.Kind);var shipments=forecast.Events.Where(e=>e.Reason=="shipment"&&e.Target==n.Id).ToArray();var sales=forecast.Events.Where(e=>e.Reason=="sale"&&e.Source==n.Id).ToArray();
   if(p.SourceKind==0){var visits=forecast.Events.Where(e=>e.Reason=="shipment"&&e.Source==n.Id&&e.Kind==0).Sum(e=>e.Count);return visits>0?p.Label+": residents make "+visits+" customer visits this round.":p.Label+" houses residents. Aim their path arrow at a neighboring shop entrance.";}
   if(p.SourceKind>=0){var sent=forecast.Events.Where(e=>e.Reason=="shipment"&&e.Source==n.Id).ToArray();return sent.Length==0?p.Label+": point its arrow at a business that needs "+Resource(p.SourceKind)+".":p.Label+" will deliver "+sent.Sum(e=>e.Count)+" "+Resource(p.SourceKind)+" this round.";}
   if(p.Rate>0){var missing=new List<string>();for(int k=0;k<3;k++)if(p.Need[k]>0&&(k==1?n.InitialGoods:k==2?n.InitialEnergy:0)==0&&!shipments.Any(e=>e.Kind==k))missing.Add(Resource(k));if(missing.Count>0)return p.Label+" needs "+string.Join(" + ",missing)+". "+(missing.Contains("customers")?"Residents must reach a shop entrance.":"Aim a supplier’s arrow at its entrance.");if(sales.Length>0)return p.Label+" will serve "+sales.Sum(e=>e.Count)+" customers → $"+sales.Sum(e=>e.Credits).ToString("N0")+" round sales.";return p.Label+": inputs must arrive early enough to be used.";}
   if(n.Kind=="bank")return "BANK boosts one adjacent selling business by 25%. Banks do not stack.";
   return p.Label+": "+p.Hint;
  }
  public static string Resource(int kind)=>kind==0?"customers":kind==1?"supplies":"power";
 }
}
