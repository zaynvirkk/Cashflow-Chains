using LocalGame;
using UnityEngine;
using UnityEngine.UI;
namespace CashflowChains.Presentation {
 // Public streets extend across the world;owned plots are a separate masked layer.
 public sealed class CityTerrain:MaskableGraphic {
  public int Columns,Rows;public float Cell;public CityRoads Traffic;public RectTransform ControlSpace,Viewport;
  readonly Vector3[] corners=new Vector3[4];Matrix4x4 prior;Vector2 priorSize;
  void Update(){var matrix=ControlSpace?ControlSpace.localToWorldMatrix:transform.localToWorldMatrix;var size=Viewport?Viewport.rect.size:Vector2.zero;if(!(Studio.Profile?.ReducedMotion??false)||matrix!=prior||size!=priorSize)SetVerticesDirty();prior=matrix;priorSize=size;}
  bool Junction(float distance,int count){int index=Mathf.RoundToInt(distance/Cell);return index>=0&&index<=count&&Mathf.Abs(distance-index*Cell)<23;}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(Cell<=0||Columns<1||Rows<1)return;var space=ControlSpace?ControlSpace:rectTransform;var r=space.rect;Color asphalt=FlatUI.Hex("#243345"),walk=FlatUI.Hex("#7C8D76"),paint=new Color(1,.87f,.6f,.5f);float left=r.xMin+Cell/2,top=r.yMax-Cell/2;float minX=r.xMin-Cell*4,maxX=r.xMax+Cell*4,minY=r.yMin-Cell*4,maxY=r.yMax+Cell*4;
   if(Viewport){Viewport.GetWorldCorners(corners);minX=minY=float.MaxValue;maxX=maxY=float.MinValue;foreach(var corner in corners){var p=space.InverseTransformPoint(corner);minX=Mathf.Min(minX,p.x);maxX=Mathf.Max(maxX,p.x);minY=Mathf.Min(minY,p.y);maxY=Mathf.Max(maxY,p.y);}minX-=Cell;maxX+=Cell;minY-=Cell;maxY+=Cell;}
   for(int x=0;x<=Columns;x++){float at=left+x*Cell;CraftedPanel.Line(vh,new Vector2(at,minY),new Vector2(at,maxY),28,walk);CraftedPanel.Line(vh,new Vector2(at,minY),new Vector2(at,maxY),25,asphalt);}
   for(int y=0;y<=Rows;y++){float at=top-y*Cell;CraftedPanel.Line(vh,new Vector2(minX,at),new Vector2(maxX,at),28,walk);CraftedPanel.Line(vh,new Vector2(minX,at),new Vector2(maxX,at),25,asphalt);}
   for(int x=0;x<=Columns;x++){float at=left+x*Cell;for(float y=Mathf.Floor(minY/28)*28;y<maxY;y+=28)if(!Junction(top-y-4.5f,Rows))CraftedPanel.Line(vh,new Vector2(at,y),new Vector2(at,y+9),1.2f,paint);}
   for(int y=0;y<=Rows;y++){float at=top-y*Cell;for(float x=Mathf.Floor(minX/28)*28;x<maxX;x+=28)if(!Junction(x+4.5f-left,Columns))CraftedPanel.Line(vh,new Vector2(x,at),new Vector2(x+9,at),1.2f,paint);}
   for(int x=0;x<=Columns;x++)for(int y=0;y<=Rows;y++){Vector2 p=new Vector2(left+x*Cell,top-y*Cell);for(int side=-1;side<=1;side+=2)for(int stripe=-2;stripe<=2;stripe++){CraftedPanel.Line(vh,p+new Vector2(side*18,stripe*3),p+new Vector2(side*23,stripe*3),1.1f,new Color(.92f,.95f,.88f,.7f));CraftedPanel.Line(vh,p+new Vector2(stripe*3,side*18),p+new Vector2(stripe*3,side*23),1.1f,new Color(.92f,.95f,.88f,.7f));}if(x>0&&x<Columns&&y>0&&y<Rows){double phase=(Traffic?.SimulationTime??0)%9;CraftedPanel.Diamond(vh,p+new Vector2(-18,-18),2.5f,FlatUI.Hex(phase<3.5?"#8BFFC3":"#FF857B"));CraftedPanel.Diamond(vh,p+new Vector2(18,18),2.5f,FlatUI.Hex(phase>=4.5&&phase<8?"#8BFFC3":"#FF857B"));}}
   if(ControlSpace){var matrix=rectTransform.worldToLocalMatrix*ControlSpace.localToWorldMatrix;var vertex=new UIVertex();for(int i=0;i<vh.currentVertCount;i++){vh.PopulateUIVertex(ref vertex,i);vertex.position=matrix.MultiplyPoint3x4(vertex.position);vh.SetUIVertex(vertex,i);}}
  }
 }
 public sealed class CityBuildPlotHint:MaskableGraphic {
  void Update(){if(!(Studio.Profile?.ReducedMotion??false))SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;float inset=5,length=Mathf.Min(r.width,r.height)*.18f;var color=FlatUI.Hex("#FFE58A");color.a=(Studio.Profile?.ReducedMotion??false)?1:.72f+Mathf.Sin(Time.unscaledTime*3)*.2f;foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1}){var p=new Vector2(x<0?r.xMin+inset:r.xMax-inset,y<0?r.yMin+inset:r.yMax-inset);CraftedPanel.Line(vh,p,p+Vector2.right*(-x*length),2.6f,color);CraftedPanel.Line(vh,p,p+Vector2.up*(-y*length),2.6f,color);}}
 }
}
