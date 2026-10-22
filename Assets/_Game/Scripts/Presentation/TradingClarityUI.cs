using System.Linq;
using CashflowChains.Domain;
using LocalGame;
using TMPro;
using UnityEngine;

namespace CashflowChains.Presentation {
 public sealed partial class MobileGameApp {
  string TradingCoach(RoundResult forecast){
   int sales=forecast.Events.Where(e=>e.Reason=="sale").Sum(e=>e.Count);
   if(data.Profile.Selected==0)return sales>0?sales+" customers forecast · 14 starting supplies":"Move the café beside residents. It has 14 supplies.";
   if(data.Profile.Selected==1)return sales>0?"Customers + farm supplies → café sales":"The café needs stock. Turn the farm’s delivery arrow.";
   if(data.Profile.Selected==2)return sales>0?"Both routes work. Your café can serve its customers.":"Bring residents close. Aim the farm at the café.";
   return sales+" sales forecast · 8 trading steps · tap a building for its role";
  }
  string BuildingOutput(Node n,RoundResult forecast){
   var piece=Pieces.Get(n.Kind);long income=forecast.Events.Where(e=>e.Source==n.Id).Sum(e=>e.Credits);
   if(income>0)return "$"+income.ToString("N0");if(income<0)return income+" COST";
   if(piece.SourceKind==0)return piece.SourceRate+" VISITS / STEP";
   if(piece.SourceKind==1)return piece.SourceRate+" GOODS / STEP";
   if(piece.SourceKind==2)return piece.SourceRate+" POWER / STEP";
   if(n.InitialGoods>0)return n.InitialGoods+" IN STOCK";
   if(piece.Credits>0)return forecast.Events.Any(e=>e.Target==n.Id&&e.Kind==0)?"NO SUPPLIES":"NO CUSTOMERS";
   return n.Kind=="bank"?"+25% SALES":n.Kind=="park"?"LOCAL APPEAL":n.Kind=="landmark"?"DIVERSITY":"DELIVERY";
  }
  void DrawOpeningTarget(RoundResult forecast){
   if(data.Profile.Selected!=0||data.Profile.Stars[0]>0||forecast.Credits>0)return;
   var home=run.Board.Nodes.FirstOrDefault(n=>n.Kind=="home");if(home==null)return;
   int direction=home.Rotation;int x=home.X+Resolver.Dx[(direction+1)%4],y=home.Y+Resolver.Dy[(direction+1)%4];
   if(x<0||y<0||x>=run.Board.Width||y>=run.Board.Height||run.Board.Nodes.Any(n=>n.X==x&&n.Y==y))return;
   var target=FlatUI.Rect(grid,"Suggested café site",0,0,720,1280).gameObject.AddComponent<FootprintPreview>();target.Columns=run.Board.Width;target.Rows=run.Board.Height;target.Cells=new[]{Vector2Int.zero};target.raycastTarget=false;target.Change(x,y,true);
   Studio.Label(grid,"CAFE\nHERE",x*720f/run.Board.Width+12,y*1280f/run.Board.Height+45,720f/run.Board.Width-24,1280f/run.Board.Height-90,22,FlatUI.Hex("#D1FFF0"),TextAlignmentOptions.Center,true);
  }
 }
}
