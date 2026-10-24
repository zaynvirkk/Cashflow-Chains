using System.Collections.Generic;
using CashflowChains.City;
using LocalGame;
using UnityEngine;
using UnityEngine.UI;
namespace CashflowChains.Presentation {
 // One regional highway; local access grows with actual construction.
 public sealed class CityTerrain:MaskableGraphic {
  public int Columns,Rows;public float Cell;public CityRoads Traffic;public RectTransform ControlSpace,Viewport;public CityStreets Streets;
  readonly Vector3[] corners=new Vector3[4];Matrix4x4 prior;Vector2 priorSize;int priorPhase=-1;
  void Update(){var matrix=ControlSpace?ControlSpace.localToWorldMatrix:transform.localToWorldMatrix;var size=Viewport?Viewport.rect.size:Vector2.zero;int phase=Traffic&&!Traffic.HighwayOnly?(Traffic.SimulationTime%9<3.5?0:Traffic.SimulationTime%9<4.5?1:Traffic.SimulationTime%9<8?2:3):-1;if(matrix!=prior||size!=priorSize||phase!=priorPhase)SetVerticesDirty();priorPhase=phase;prior=matrix;priorSize=size;}
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();if(Cell<=0||Columns<1||Rows<1||Streets==null)return;
   var space=ControlSpace?ControlSpace:rectTransform;var r=space.rect;
   float left=r.xMin+Cell/2,top=r.yMax-Cell/2,minX=r.xMin-Cell*4,maxX=r.xMax+Cell*4;
   if(Viewport){Viewport.GetWorldCorners(corners);minX=float.MaxValue;maxX=float.MinValue;foreach(var corner in corners){var p=space.InverseTransformPoint(corner);minX=Mathf.Min(minX,p.x);maxX=Mathf.Max(maxX,p.x);}minX-=Cell;maxX+=Cell;}
   Vector2 Point(int x,int y)=>new Vector2(left+x*Cell,top-y*Cell);
   Color asphalt=FlatUI.Hex("#293949"),walk=FlatUI.Hex("#A2A88A"),paint=FlatUI.Hex("#EBD8A0");
   var edges=Streets.LocalEdges;
   // Sidewalks first, then all road faces: no seam across joined segments.
   CraftedPanel.Line(vh,new Vector2(minX,top),new Vector2(maxX,top),34,walk);
   foreach(var e in edges)CraftedPanel.Line(vh,Point(e.X,e.Y),Point(e.X+e.DX,e.Y+e.DY),28,walk);
   CraftedPanel.Line(vh,new Vector2(minX,top),new Vector2(maxX,top),30,asphalt);
   foreach(var e in edges)CraftedPanel.Line(vh,Point(e.X,e.Y),Point(e.X+e.DX,e.Y+e.DY),24,asphalt);
   for(float x=Mathf.Floor(minX/28)*28;x<maxX;x+=28){bool junction=false;foreach(var e in edges)if(e.Y==0&&Mathf.Abs(x+4.5f-Point(e.X,0).x)<23){junction=true;break;}if(!junction)CraftedPanel.Line(vh,new Vector2(x,top),new Vector2(x+9,top),1.2f,paint);}
   var nodes=new HashSet<Vector2Int>();
   foreach(var e in edges){var a=Point(e.X,e.Y);var b=Point(e.X+e.DX,e.Y+e.DY);var direction=(b-a).normalized;for(float d=25;d<Cell-23;d+=28)CraftedPanel.Line(vh,a+direction*d,a+direction*(d+9),1.1f,paint);nodes.Add(new Vector2Int(e.X,e.Y));nodes.Add(new Vector2Int(e.X+e.DX,e.Y+e.DY));}
   foreach(var n in nodes){var p=Point(n.x,n.y);int degree=Streets.Degree(n.x,n.y);
    if(degree>=3)foreach(var step in new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down})if(Streets.HasRoad(n.x,n.y,n.x+step.x,n.y+step.y)){var dir=new Vector2(step.x,-step.y);var side=new Vector2(-dir.y,dir.x);for(int stripe=-2;stripe<=2;stripe++)CraftedPanel.Line(vh,p+dir*17+side*stripe*3,p+dir*22+side*stripe*3,1.1f,new Color(.95f,.96f,.88f,.8f));}
    if(Traffic&&!Traffic.HighwayOnly&&n.x>0&&n.x<Columns&&n.y>0&&n.y<Rows){double phase=Traffic.SimulationTime%9;CraftedPanel.Diamond(vh,p+new Vector2(-18,-18),2.5f,FlatUI.Hex(phase<3.5?"#89F5AC":"#EF7D6B"));CraftedPanel.Diamond(vh,p+new Vector2(18,18),2.5f,FlatUI.Hex(phase>=4.5&&phase<8?"#89F5AC":"#EF7D6B"));}
    if(degree==1){CityGameSkin.Round(vh,new Rect(p.x-13,p.y-13,26,26),13,walk,walk);CityGameSkin.Round(vh,new Rect(p.x-11,p.y-11,22,22),11,asphalt,asphalt);}
   }
   if(ControlSpace){var matrix=rectTransform.worldToLocalMatrix*ControlSpace.localToWorldMatrix;var vertex=new UIVertex();for(int i=0;i<vh.currentVertCount;i++){vh.PopulateUIVertex(ref vertex,i);vertex.position=matrix.MultiplyPoint3x4(vertex.position);vh.SetUIVertex(vertex,i);}}
  }
 }
 public sealed class CityBuildPlotHint:MaskableGraphic {
  void Update(){if(!(Studio.Profile?.ReducedMotion??false))SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;float inset=5,length=Mathf.Min(r.width,r.height)*.18f;var color=FlatUI.Hex("#FFE58A");color.a=(Studio.Profile?.ReducedMotion??false)?1:.72f+Mathf.Sin(Time.unscaledTime*3)*.2f;foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1}){var p=new Vector2(x<0?r.xMin+inset:r.xMax-inset,y<0?r.yMin+inset:r.yMax-inset);CraftedPanel.Line(vh,p,p+Vector2.right*(-x*length),2.6f,color);CraftedPanel.Line(vh,p,p+Vector2.up*(-y*length),2.6f,color);}}
 }
}
