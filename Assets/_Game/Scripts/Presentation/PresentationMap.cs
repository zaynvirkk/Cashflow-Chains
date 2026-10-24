using System.Linq;
using System;using System.IO;using System.Collections.Generic;using UnityEngine;using UnityEngine.UI;
namespace LocalGame {
 public sealed class AuthoredRect:MonoBehaviour {public Rect Design;}
 public sealed class PresentationMap:MonoBehaviour {
  [Serializable] public sealed class Element {public string Name,Kind,DisplayText;public bool Interactable;public Rect Source,Pixels;public int Priority;}
  [Serializable] public sealed class Snapshot {public int Width,Height,Frame;public string Screen;public List<Element> Elements=new List<Element>();}
  Rect Source(Transform t){if(t==transform)return new Rect(0,0,720,1280);var a=t.GetComponent<AuthoredRect>();var p=Source(t.parent);if(!a)return p;var r=a.Design;return new Rect(p.x+r.x/720*p.width,p.y+r.y/1280*p.height,r.width/720*p.width,r.height/1280*p.height);}
  void LateUpdate(){string dir=FlatUI.Arg("-capture-dir");if(dir==null||Time.frameCount%6!=0)return;var s=new Snapshot{Width=Screen.width,Height=Screen.height,Frame=Time.frameCount,Screen=FlatUI.InputScreen};foreach(var r in GetComponentsInChildren<RectTransform>()){bool grid=r.name=="City grid"||r.name=="Velvet case";bool button=r.GetComponent<Button>();bool hit=r.GetComponent<PointerTarget>();bool wallet=r.name=="City wallet";if(!grid&&!button&&!hit&&!wallet)continue;var corners=new Vector3[4];r.GetWorldCorners(corners);Vector2 a=RectTransformUtility.WorldToScreenPoint(null,corners[0]),b=RectTransformUtility.WorldToScreenPoint(null,corners[2]);int priority=button?100:hit?75:50;for(Transform p=r;p&&p!=transform;p=p.parent)if(p.name=="Scrim")priority+=1000;s.Elements.Add(new Element{Name=r.name,Kind=grid?"grid":button?"button":wallet?"wallet":"pointer",DisplayText=button||wallet?r.GetComponentInChildren<TMPro.TextMeshProUGUI>()?.text:null,Interactable=button&&r.GetComponent<Button>().IsInteractable(),Source=Source(r),Pixels=new Rect(a.x,Screen.height-b.y,b.x-a.x,b.y-a.y),Priority=priority});}Directory.CreateDirectory(dir);string path=Path.Combine(dir,"ui-layout.json"),tmp=path+".tmp";File.WriteAllText(tmp,JsonUtility.ToJson(s));if(File.Exists(path))File.Replace(tmp,path,null);else File.Move(tmp,path);}
 }
 public sealed class AtmosphereShade:MaskableGraphic {
  public bool Top;protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;bool wide=FlatUI.Stage&&FlatUI.Stage.Wide;for(int i=0;i<20;i++){float a=i/20f,b=(i+1)/20f;float opacity=(Top?1-a:b)*.72f;var c=new Color(.01f,.018f,.025f,opacity);if(wide&&FlatUI.Stage.View=="board")CraftedPanel.Quad(vh,new Vector2(r.xMin+a*r.width,r.yMin),new Vector2(r.xMin+b*r.width,r.yMin),new Vector2(r.xMin+b*r.width,r.yMax),new Vector2(r.xMin+a*r.width,r.yMax),new Color(c.r,c.g,c.b,(Top?1-a:a)*.8f));else CraftedPanel.Quad(vh,new Vector2(r.xMin,r.yMax-a*r.height),new Vector2(r.xMax,r.yMax-a*r.height),new Vector2(r.xMax,r.yMax-b*r.height),new Vector2(r.xMin,r.yMax-b*r.height),c);}}
 }
 public sealed class CityRoads:MaskableGraphic {
  public int Columns,Rows;public int VisibleVehicles=int.MaxValue;public bool RenderLaneMarks=true,HighwayOnly;
  CashflowChains.Domain.TrafficSimulation traffic;
  sealed class SharedTraffic {public CashflowChains.Domain.TrafficSimulation Model;public float Last;}
  static readonly Dictionary<string,SharedTraffic> shared=new Dictionary<string,SharedTraffic>();
  SharedTraffic current;public double SimulationTime=>traffic?.Time??0;
  [Serializable] sealed class TrafficView {public double Time;public int OverlapCount,Columns,Rows;public float Width,Height;public List<CashflowChains.Domain.StreetCar> Cars;}
  void Update(){var r=rectTransform.rect;if(r.width<Columns*40||r.height<Rows*40)return;string key=Columns+":"+Rows+":"+r.width.ToString("F1")+":"+r.height.ToString("F1")+":"+HighwayOnly;if(current==null||current.Model.Width!=r.width||current.Model.Height!=r.height){if(!shared.TryGetValue(key,out current)){if(shared.Count>32)shared.Clear();current=new SharedTraffic{Model=new CashflowChains.Domain.TrafficSimulation(r.width,r.height,Columns,Rows,HighwayOnly),Last=Time.unscaledTime};shared[key]=current;}traffic=current.Model;}
   float now=Time.unscaledTime;if(!(Studio.Profile?.ReducedMotion??false))traffic.Advance(Math.Max(0,Math.Min(.05,now-current.Last)));current.Last=now;SetVerticesDirty();string dir=FlatUI.Arg("-capture-dir");if(dir!=null&&Time.frameCount%6==0){Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"traffic-state.json"),JsonUtility.ToJson(new TrafficView{Time=traffic.Time,OverlapCount=traffic.HasOverlap()?1:0,Columns=Columns,Rows=Rows,Width=r.width,Height=r.height,Cars=traffic.Cars.Take(VisibleVehicles).ToList()}));}}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;if(RenderLaneMarks)for(int axis=0;axis<2;axis++){int n=axis==0?Columns:Rows;float step=(axis==0?r.width:r.height)/n;for(int lane=1;lane<n;lane++){float k=(axis==0?r.xMin:r.yMin)+lane*step;for(float j=0;j<(axis==0?r.height:r.width);j+=28){Vector2 a=axis==0?new Vector2(k,r.yMin+j):new Vector2(r.xMin+j,k);CraftedPanel.Line(vh,a,a+(axis==0?Vector2.up:Vector2.right)*10,1,new Color(1,.84f,.48f,.35f));}}}
   if(traffic==null)return;foreach(var car in traffic.Cars.Take(VisibleVehicles)){Vector2 p=new Vector2(r.xMin+(float)car.X,r.yMin+(float)car.Y);Color paint=FlatUI.Hex(new[]{"#FF6B71","#55C6F1","#FFCC63","#E4ECE1"}[car.Id%4]);Vector2 d=(car.Axis==0?Vector2.up:Vector2.right)*car.Direction,u=new Vector2(-d.y,d.x);CarBody(vh,p+new Vector2(1,-1.8f),d,u,7.3f,3.4f,new Color(0,0,0,.4f));
    for(int axle=-1;axle<=1;axle+=2)for(int side=-1;side<=1;side+=2)Quad(vh,p+d*axle*3.6f+u*side*3.1f,d,u,1.4f,.6f,FlatUI.Hex("#071823"));
    CarBody(vh,p,d,u,7,3.1f,FlatUI.Hex("#102A40"));CarBody(vh,p,d,u,6.4f,2.8f,paint);
    Quad(vh,p-d*.5f,d,u,2.4f,2.1f,Color.Lerp(paint,Color.white,.2f));
    Quad(vh,p+d*2.3f,d,u,1.1f,2.05f,FlatUI.Hex("#B8F1F8"));Quad(vh,p-d*3.2f,d,u,.8f,2.0f,FlatUI.Hex("#21485D"));
    for(int side=-1;side<=1;side+=2){Quad(vh,p+d*5.5f+u*side*1.75f,d,u,.5f,.65f,FlatUI.Hex("#FFF0B4"));Quad(vh,p-d*5.5f+u*side*1.75f,d,u,.4f,.65f,FlatUI.Hex("#FF6659"));}}

  }
  static void CarBody(VertexHelper vh,Vector2 p,Vector2 d,Vector2 u,float length,float width,Color c){
   int start=vh.currentVertCount;vh.AddVert(p,c,Vector2.zero);
   foreach(var q in new[]{new Vector2(length-1.2f,-width),new Vector2(length,-width+1.2f),new Vector2(length,width-1.2f),new Vector2(length-1.2f,width),new Vector2(-length+1.2f,width),new Vector2(-length,width-1.2f),new Vector2(-length,-width+1.2f),new Vector2(-length+1.2f,-width)})vh.AddVert(p+d*q.x+u*q.y,c,Vector2.zero);
   for(int i=0;i<8;i++)vh.AddTriangle(start,start+1+i,start+1+(i+1)%8);
  }
  static void Quad(VertexHelper vh,Vector2 p,Vector2 d,Vector2 u,float length,float width,Color c){CraftedPanel.Quad(vh,p-d*length-u*width,p+d*length-u*width,p+d*length+u*width,p-d*length+u*width,c);}
 }
 public sealed class PortGlyph:MaskableGraphic {
  public bool Output;public int Direction,Kind=-1;
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;Vector2 p=r.center;float radius=Mathf.Min(r.width,r.height)*.36f;Color c=Output?FlatUI.Hex(Kind==0?"#72FFD1":Kind==1?"#FFD357":Kind==2?"#8EDBFF":"#D1FFF0"):FlatUI.Hex("#FFF0BB");if(!Output){for(int i=0;i<12;i++){float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;int n=vh.currentVertCount;vh.AddVert(p,c,Vector2.zero);vh.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius*.7f,c,Vector2.zero);vh.AddVert(p+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius*.7f,c,Vector2.zero);vh.AddTriangle(n,n+1,n+2);}return;}Vector2 d=Direction==0?Vector2.up:Direction==1?Vector2.right:Direction==2?Vector2.down:Vector2.left;Vector2 side=new Vector2(-d.y,d.x);CraftedPanel.Line(vh,p-d*radius,p+d*radius*.35f,Mathf.Max(1.2f,radius*.38f),c);int k=vh.currentVertCount;vh.AddVert(p+d*radius*1.1f,c,Vector2.zero);vh.AddVert(p+d*radius*.05f+side*radius*.75f,c,Vector2.zero);vh.AddVert(p+d*radius*.05f-side*radius*.75f,c,Vector2.zero);vh.AddTriangle(k,k+1,k+2);}
 }
}
