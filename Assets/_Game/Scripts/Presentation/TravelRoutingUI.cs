using System;
using System.Collections;
using System.Linq;
using CashflowChains.Domain;
using LocalGame;
using TMPro;
using UnityEngine;
namespace CashflowChains.Presentation {
 public sealed partial class MobileGameApp {
  CityRun CreateRun(int level,Board restored=null)=>new CityRun(level,restored,1);
  float TradingStepSeconds=>data.Profile.ReducedMotion?.08f:(run.Board.RulesVersion==1?.35f:data.Profile.Selected<3?.58f:.42f)/(data.Profile.FastResolve?2:1);
  void ChooseTravelRoute(int page=0){
   if(busy)return;var source=run.Board.Nodes.Find(n=>n.Id==selected);if(source==null)return;var piece=Pieces.Get(source.Kind);if(RoadRouting.OutputKind(piece)<0){TravelBusinessDetails(source);return;}
   var m=Studio.Modal(root,TravelInsight.Name(piece).ToUpperInvariant()+" ROUTES",Board);
   Studio.Image(m,source.Kind,270,180,180,180);
   Studio.Label(m,TravelInsight.Role(piece)+"\nRoads connect every plot. Distance sets arrival time; rotation does not.",44,385,632,170,27,Studio.Text,TextAlignmentOptions.Center);
   Studio.Button(m,source.TargetId==0?"AUTO · NEAREST WITH ROOM ✓":"AUTO · NEAREST WITH ROOM",44,580,632,85,()=>SetTravelDestination(source.Id,0),source.TargetId==0);
   var destinations=run.Board.Nodes.Where(n=>RoadRouting.CanDeliver(source,n)).OrderBy(n=>RoadRouting.Minutes(source,n)).ThenBy(n=>n.Id).ToArray();int pages=Math.Max(1,(destinations.Length+2)/3);page=Math.Max(0,Math.Min(page,pages-1));
   for(int i=0;i<3;i++){int index=page*3+i;if(index>=destinations.Length)break;var target=destinations[index];string title=TravelInsight.Name(Pieces.Get(target.Kind))+" #"+target.Id+" · "+RoadRouting.Minutes(source,target)+" MIN";Studio.Button(m,title.ToUpperInvariant()+(source.TargetId==target.Id?" ✓":""),44,700+i*115,632,95,()=>SetTravelDestination(source.Id,target.Id),source.TargetId==target.Id);}
   if(destinations.Length==0)Studio.Label(m,"No business here accepts "+TravelInsight.Resource(RoadRouting.OutputKind(piece))+". Build a compatible recipient first.",44,720,632,210,29,Studio.Text,TextAlignmentOptions.Center);
   if(pages>1){Studio.Button(m,"‹",44,1055,110,75,()=>ChooseTravelRoute((page+pages-1)%pages));Studio.Label(m,(page+1)+" / "+pages,200,1055,320,75,25,Studio.Text,TextAlignmentOptions.Center);Studio.Button(m,"›",566,1055,110,75,()=>ChooseTravelRoute((page+1)%pages));}
   Studio.Button(m,"KEEP BUILDING",44,1150,632,80,Board);
  }
  void SetTravelDestination(int id,int target){var before=run.Board.Copy();if(!run.SetDestination(id,target)){sound.Cue("deny");return;}Remember(before,id);Save();Board();sound.Cue("select");}
  void TravelBusinessDetails(Node node){var p=Pieces.Get(node.Kind);var result=run.Preview();var m=Studio.Modal(root,p.Label+" · HOW IT WORKS",Board);Studio.Image(m,node.Kind,245,190,230,230);Studio.Label(m,TravelInsight.Role(p),44,470,632,190,31,Studio.Text,TextAlignmentOptions.Center);Studio.Label(m,TravelInsight.Describe(run.Board,result,node.Id),44,710,632,240,29,Studio.Accent,TextAlignmentOptions.Center);Studio.Label(m,"One trading day is 24 simulated minutes. A shop sells after its customers and stock arrive. Moving closer can bring sales forward; forecasting and undo are free.",44,980,632,145,24,Studio.Text,TextAlignmentOptions.Center);Studio.Button(m,"KEEP BUILDING",44,1150,632,85,Board,true);}
  Vector2[] RoadPoints(Node from,Node to){return new[]{new Vector2(from.X+.5f,from.Y+.5f),new Vector2(from.X+1,from.Y+.5f),new Vector2(from.X+1,from.Y+1),new Vector2(to.X+1,from.Y+1),new Vector2(to.X+1,to.Y+.5f),new Vector2(to.X+.5f,to.Y+.5f)};}
  IEnumerator TravelRoad(RectTransform token,Node from,Node to,float duration){var marker=token.GetComponent<StageElement>();if(marker)Destroy(marker);token.anchorMin=token.anchorMax=Vector2.one*.5f;token.sizeDelta=Vector2.one*20;var points=RoadPoints(from,to).Select(p=>new Vector2(38+p.x*644/run.Board.Width,310+p.y*644/run.Board.Height)).ToArray();var lengths=new float[points.Length];float total=0;for(int i=1;i<points.Length;i++){total+=Vector2.Distance(points[i-1],points[i]);lengths[i]=total;}float elapsed=0;while(token&&elapsed<duration){elapsed+=Time.deltaTime;float along=Mathf.Clamp01(elapsed/duration)*total;int segment=1;while(segment<points.Length-1&&lengths[segment]<along)segment++;float span=lengths[segment]-lengths[segment-1];var p=Vector2.Lerp(points[segment-1],points[segment],span<=0?1:(along-lengths[segment-1])/span);var local=FlatUI.Stage?FlatUI.Stage.Point(p.x,p.y):new Vector2(p.x-360,640-p.y);token.anchoredPosition=local;yield return null;}if(token)Destroy(token.gameObject);}
  void DrawRoadRoute(Node a,Node b,Color color,int kind){var points=RoadPoints(a,b);for(int i=1;i<points.Length;i++){if((points[i]-points[i-1]).sqrMagnitude<.001f)continue;WorldArt.Link(root,38+points[i-1].x*644/run.Board.Width,310+points[i-1].y*644/run.Board.Height,38+points[i].x*644/run.Board.Width,310+points[i].y*644/run.Board.Height,color,kind);var line=root.GetComponentsInChildren<RouteRibbon>().Last();line.Active=false;}}
 }
}
