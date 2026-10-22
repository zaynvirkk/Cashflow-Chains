/* THESIS: The playable town and collector's case are the interface.
 OWN-WORLD: Painted blue-hour architecture and amber routes; candlelit oxblood velvet and aged brass.
 STORY: Move an object, feel its weight, watch a real consequence answer the gesture.
 FIRST VIEWPORT: Full-bleed environment, substantial game pieces, restrained physical controls.
 FORM: A live 2D toy theatre, not a stack of application cards.
 FINISH: Inspect actual native and mobile captures; record the built world and remaining defects. */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace LocalGame {
 public sealed class CraftedPanel:MaskableGraphic {
  public Color Fill;public bool Primary,Velvet,Tile,BorderOnly,Medallion;public int Seed;public float Heat;Button button;float hover;bool over;
  protected override void Awake(){base.Awake();raycastTarget=false;}
  public void Refresh(){SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();Rect r=rectTransform.rect;bool disabled=button&&!button.interactable;Color fill=disabled?Color.Lerp(Fill,Color.black,.38f):Fill;
   if(BorderOnly){Color rail=FlatUI.Hex(Velvet?"#39291C":"#254448");Quad(vh,new Vector2(r.xMin,r.yMin),new Vector2(r.xMax,r.yMin),new Vector2(r.xMax,r.yMin+12),new Vector2(r.xMin,r.yMin+12),rail);Quad(vh,new Vector2(r.xMin,r.yMax-12),new Vector2(r.xMax,r.yMax-12),new Vector2(r.xMax,r.yMax),new Vector2(r.xMin,r.yMax),rail);Quad(vh,new Vector2(r.xMin,r.yMin),new Vector2(r.xMin+12,r.yMin),new Vector2(r.xMin+12,r.yMax),new Vector2(r.xMin,r.yMax),rail);Quad(vh,new Vector2(r.xMax-12,r.yMin),new Vector2(r.xMax,r.yMin),new Vector2(r.xMax,r.yMax),new Vector2(r.xMax-12,r.yMax),rail);Color rim=Velvet?FlatUI.Hex("#B38B4B"):FlatUI.Hex("#6D9B83");Line(vh,new Vector2(r.xMin+3,r.yMin+3),new Vector2(r.xMax-3,r.yMin+3),1,rim);Line(vh,new Vector2(r.xMin+3,r.yMax-3),new Vector2(r.xMax-3,r.yMax-3),1,rim);return;}
   if(Medallion){float radius=r.height*.46f;Disc(vh,r.center+Vector2.down*4,radius+2,new Color(0,0,0,.65f));Disc(vh,r.center,radius,FlatUI.Hex("#CEAA5E"));Disc(vh,r.center,radius-3,fill);return;}
   float cut=Tile?3:Velvet?8:16;var shadow=new Rect(r.xMin+1,r.yMin-4,r.width,r.height);if(!Tile&&fill.a>.95f)Polygon(vh,shadow,new Color(0,0,0,.65f),cut);
   Color edge=Velvet?FlatUI.Hex("#A77A36"):Primary?FlatUI.Hex("#FFE092"):FlatUI.Hex("#2484A5");if(Primary)edge=Color.Lerp(edge,Color.white,Heat);if(Tile){edge=Color.Lerp(fill,Color.white,.13f);edge.a=Velvet?.48f:.30f;}
   Ring(vh,r,edge,cut,1.6f);Polygon(vh,new Rect(r.xMin+1.6f,r.yMin+1.6f,r.width-3.2f,r.height-3.2f),fill,cut);
   if(!Tile){Line(vh,new Vector2(r.xMin+cut,r.yMax-3),new Vector2(r.xMax-cut,r.yMax-3),1,new Color(1,.88f,.58f,Primary?.65f:.17f));Line(vh,new Vector2(r.xMin+cut,r.yMin+3),new Vector2(r.xMax-cut,r.yMin+3),1,new Color(0,0,0,.45f));
    if(Velvet&&r.width>120){Diamond(vh,new Vector2(r.xMin+7,r.center.y),3,edge);Diamond(vh,new Vector2(r.xMax-7,r.center.y),3,edge);}}
   if(Primary&&!Velvet){var shine=new Color(1,1,.68f,.25f);Polygon(vh,new Rect(r.xMin+5,r.center.y,r.width-10,r.height*.45f),shine,12);Ring(vh,new Rect(r.xMin+5,r.yMin+5,r.width-10,r.height-10),new Color(1,1,.65f,.8f),12,2);}
   // Material grain belongs to the object rather than a screen-wide filter.
   for(int i=0;i<0;i++){float t=Mathf.Repeat(i*.6180339f+Seed*.13f,1),u=Mathf.Repeat(i*.414213f+Seed*.23f,1);Vector2 a=new Vector2(r.xMin+5+t*(r.width-10),r.yMin+5+u*(r.height-10));Line(vh,a,a+new Vector2(Tile?6:12,0),.6f,new Color(1,.9f,.7f,.035f));}
  }
  void Update(){if(!button)button=GetComponentInParent<Button>();}
  static Vector2[] Corners(Rect r,float c)=>new[]{new Vector2(r.xMin+c,r.yMin),new Vector2(r.xMax-c,r.yMin),new Vector2(r.xMax,r.yMin+c),new Vector2(r.xMax,r.yMax-c),new Vector2(r.xMax-c,r.yMax),new Vector2(r.xMin+c,r.yMax),new Vector2(r.xMin,r.yMax-c),new Vector2(r.xMin,r.yMin+c)};
  static void Ring(VertexHelper vh,Rect r,Color c,float cut,float width){var outside=Corners(r,cut);var inside=Corners(new Rect(r.xMin+width,r.yMin+width,r.width-2*width,r.height-2*width),Mathf.Max(0,cut-width));for(int i=0;i<8;i++)Quad(vh,outside[i],outside[(i+1)%8],inside[(i+1)%8],inside[i],c);}
  static void Disc(VertexHelper vh,Vector2 p,float radius,Color c){int n=vh.currentVertCount;vh.AddVert(p,c,Vector2.zero);for(int i=0;i<=40;i++){float a=i*Mathf.PI/20;vh.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,c,Vector2.zero);}for(int i=0;i<40;i++)vh.AddTriangle(n,n+i+1,n+i+2);}
  public static void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color){int n=vh.currentVertCount;vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);vh.AddVert(c,color,Vector2.zero);vh.AddVert(d,color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n+2,n+3,n);}
  public static void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color c){var d=b-a;if(d.sqrMagnitude<.001f)return;var v=new Vector2(-d.y,d.x).normalized*width*.5f;Quad(vh,a-v,a+v,b+v,b-v,c);}
  public static void Diamond(VertexHelper vh,Vector2 p,float s,Color c){Quad(vh,p+Vector2.up*s,p+Vector2.right*s,p+Vector2.down*s,p+Vector2.left*s,c);}
  public static void Polygon(VertexHelper vh,Rect r,Color c,float cut){cut=Mathf.Min(cut,Mathf.Min(r.width,r.height)*.2f);Vector2[] p={new Vector2(r.xMin+cut,r.yMin),new Vector2(r.xMax-cut,r.yMin),new Vector2(r.xMax,r.yMin+cut),new Vector2(r.xMax,r.yMax-cut),new Vector2(r.xMax-cut,r.yMax),new Vector2(r.xMin+cut,r.yMax),new Vector2(r.xMin,r.yMax-cut),new Vector2(r.xMin,r.yMin+cut)};int n=vh.currentVertCount;vh.AddVert(r.center,c,Vector2.zero);foreach(var v in p)vh.AddVert(v,c,Vector2.zero);for(int i=0;i<8;i++)vh.AddTriangle(n,n+1+i,n+1+(i+1)%8);}
 }
 public sealed class WorldParallax:MonoBehaviour {
  Vector3 origin;void Start(){origin=transform.localPosition;transform.localScale=Vector3.one*1.025f;}
  void Update(){if(Studio.Profile?.ReducedMotion??false){transform.localPosition=origin;return;}var mouse=UnityEngine.InputSystem.Pointer.current;Vector2 p=mouse==null?Vector2.zero:mouse.position.ReadValue();var target=origin+new Vector3((p.x/Mathf.Max(1,Screen.width)-.5f)*-5,(p.y/Mathf.Max(1,Screen.height)-.5f)*-5,0);transform.localPosition=Vector3.Lerp(transform.localPosition,target,1-Mathf.Exp(-Time.unscaledDeltaTime*3));}
 }
 public sealed class SoftReveal:MonoBehaviour {
  float age;Vector3 size;void Awake(){size=transform.localScale;}void Update(){if(Studio.Profile?.ReducedMotion??false){transform.localScale=size;enabled=false;return;}age+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(age/.22f);transform.localScale=size*(1-.035f*Mathf.Pow(1-t,3));if(t>=1)enabled=false;}
 }
 public sealed class ResultNumber:MonoBehaviour {
  public long Target;float age;TMPro.TextMeshProUGUI label;void Start(){label=GetComponent<TMPro.TextMeshProUGUI>();}void Update(){if(!label)return;age+=Time.unscaledDeltaTime;float t=(Studio.Profile?.ReducedMotion??false)?1:Mathf.Clamp01(age/.42f);label.text=((long)(Target*(1-Math.Pow(1-t,3)))).ToString("N0");if(t>=1)enabled=false;}
 }
 public sealed class LivingArt:MonoBehaviour {
  public float Phase;public bool Hero;Vector3 original,position;float birth,landed=-100;public void Land(){landed=Time.unscaledTime;}void Start(){original=transform.localScale;position=transform.localPosition;birth=Time.unscaledTime;}
  void Update(){if(Studio.Profile?.ReducedMotion??false){transform.localScale=original;transform.localRotation=Quaternion.identity;return;}float t=Time.unscaledTime;float entry=Mathf.Min(1,(t-birth)*6);float breathe=Hero?.014f:.006f;float pulse=Hero?Mathf.Sin(entry*Mathf.PI)*.11f*(1-entry):0;float drop=Mathf.Clamp01((t-landed)/.28f);float bounce=Mathf.Sin(drop*Mathf.PI*2.3f)*Mathf.Exp(-drop*4)*.15f;transform.localScale=Vector3.Scale(original*(1+breathe*Mathf.Sin(t*1.5f+Phase)+pulse),new Vector3(1-bounce*.45f,1+bounce,1));if(Hero)transform.localPosition=position+Vector3.up*(4*Mathf.Sin(t*1.2f+Phase));if(Hero)transform.localRotation=Quaternion.Euler(0,0,.7f*Mathf.Sin(t*.55f+Phase));}
 }
 public sealed class GameFeel:MaskableGraphic {
  struct Fleck {public Vector2 P,V;public Color C;public float Age,Life,Size,Angle,Spin;public int Shape;}
  readonly List<Fleck> particles=new List<Fleck>(240);RectTransform source;static GameFeel instance;float clock;int serial;public static int ActiveParticles=>instance?instance.particles.Count:0;
  public static void Setup(RectTransform root){if(instance)return;var go=new GameObject("World particles",typeof(RectTransform),typeof(Canvas),typeof(GameFeel));var r=go.GetComponent<RectTransform>();r.SetParent(root.parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(720,1280);var c=go.GetComponent<Canvas>();c.overrideSorting=true;c.sortingOrder=60;instance=go.GetComponent<GameFeel>();instance.source=root;instance.raycastTarget=false;}
  public static void Burst(float x,float y,Color color,int count=18,int shape=0){var point=FlatUI.Stage?FlatUI.Stage.Point(x,y):new Vector2(x-360,640-y);Emit(point,color,count,shape);}
  static void Emit(Vector2 point,Color color,int count,int shape){if(!instance)return;if(Studio.Profile?.ActiveDecoration=="coral-confetti"){shape=1;color=FlatUI.Hex("#FF91BD");}else if(Studio.Profile?.ActiveDecoration=="gold-rain"){shape=2;color=FlatUI.Hex("#FFDB69");}else if(Studio.Profile?.ActiveDecoration=="mint-parade")color=FlatUI.Hex("#62FFCD");bool quiet=Studio.Profile?.ReducedMotion??false;count=quiet?Mathf.Min(3,count):count;for(int i=0;i<count&&instance.particles.Count<220;i++){float angle=(i*2.399963f+instance.serial++)%6.283185f;float speed=quiet?5:55+(i*23)%150;instance.particles.Add(new Fleck{P=point,V=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*speed,C=color,Life=quiet?.22f:.5f+(i%7)*.09f,Size=2+i%4,Angle=angle,Spin=i%2==0?2:-2,Shape=shape});}}
  public static void Celebrate(int stars){if(!instance)return;float h=instance.source.rect.height;Emit(new Vector2(0,h*.20f),Studio.Accent,30+stars*18,1);if(stars==3){Emit(new Vector2(-180,h*.15f),FlatUI.Hex("#FFE19A"),30,1);Emit(new Vector2(180,h*.15f),FlatUI.Hex("#FFE19A"),30,1);}}
  public static void FromScreen(Vector2 p,Color c,int n=18){if(!instance||!instance.source)return;RectTransformUtility.ScreenPointToLocalPointInRectangle(instance.source,p,null,out var local);Emit(local,c,n,0);}
  void Update(){if(!source)return;rectTransform.localScale=source.localScale;rectTransform.sizeDelta=source.rect.size;clock+=Time.unscaledDeltaTime;float dt=Mathf.Min(.04f,Time.unscaledDeltaTime);for(int i=particles.Count-1;i>=0;i--){var p=particles[i];p.Age+=dt;if(p.Age>=p.Life){particles.RemoveAt(i);continue;}p.P+=p.V*dt;p.V*=Mathf.Exp(-dt*1.6f);p.V.y-=dt*(Studio.Velvet?30:80);p.Angle+=p.Spin*dt;particles[i]=p;}SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();bool quiet=Studio.Profile?.ReducedMotion??false;
   if(!quiet)for(int i=0;i<22;i++){float x=(i%2==0?-1:1)*(rectTransform.rect.width/2-35+18*Mathf.Sin(i*7.3f+clock*.12f));float y=-rectTransform.rect.height/2+Mathf.Repeat(i*137+clock*(Studio.Velvet?4:9),rectTransform.rect.height);Color c=Studio.Velvet?new Color(1,.68f,.28f,.12f+.1f*Mathf.Sin(clock+i)):new Color(.7f,1,.72f,.18f+.12f*Mathf.Sin(clock*1.2f+i));Glow(vh,new Vector2(x,y),i%3+1,c);}
   foreach(var p in particles){Color c=p.C;c.a*=Mathf.Pow(1-p.Age/p.Life,.7f);if(p.Shape==2){var u=new Vector2(Mathf.Cos(p.Angle),Mathf.Sin(p.Angle))*p.Size*1.8f;var v=new Vector2(-u.y,u.x)*.5f;Color paper=new Color(.38f,1,.42f,c.a);CraftedPanel.Quad(vh,p.P-u-v,p.P+u-v,p.P+u+v,p.P-u+v,paper);CraftedPanel.Quad(vh,p.P-u*.15f-v,p.P+u*.15f-v,p.P+u*.15f+v,p.P-u*.15f+v,new Color(1,.94f,.65f,c.a));}else if(p.Shape==1){var u=new Vector2(Mathf.Cos(p.Angle),Mathf.Sin(p.Angle))*p.Size;var v=new Vector2(-u.y,u.x);CraftedPanel.Quad(vh,p.P+u,p.P+v,p.P-u,p.P-v,c);}else Glow(vh,p.P,p.Size,c);}
  }
  static void Glow(VertexHelper vh,Vector2 p,float radius,Color c){int n=vh.currentVertCount;vh.AddVert(p,c,Vector2.zero);var edge=c;edge.a=0;for(int i=0;i<9;i++){float a=i*Mathf.PI/4;vh.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius*3,edge,Vector2.zero);}for(int i=0;i<8;i++)vh.AddTriangle(n,n+i+1,n+i+2);CraftedPanel.Diamond(vh,p,radius*.45f,c);}
 }
 public sealed class RouteRibbon:MaskableGraphic {
  public Vector2 A,B;public Vector2 AuthoredA,AuthoredB;public bool UsesAuthored;public int Kind=-1;public Color Tint;public bool Active=true;float clock;
  void Update(){if(!(Studio.Profile?.ReducedMotion??false)){clock+=Time.unscaledDeltaTime;SetVerticesDirty();}}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(UsesAuthored&&FlatUI.Stage){A=FlatUI.Stage.Point(AuthoredA.x,AuthoredA.y);B=FlatUI.Stage.Point(AuthoredB.x,AuthoredB.y);}Color shade=Tint;shade.a=.24f;CraftedPanel.Line(vh,A,B,15,shade);shade.a=.8f;CraftedPanel.Line(vh,A,B,2.2f,shade);if(!Active)return;for(int i=0;i<3;i++){float t=Mathf.Repeat(clock*.44f+i/3f,1);var p=Vector2.Lerp(A,B,t);if(Kind>=0)ResourceToken.Draw(vh,p,7,Kind);else CraftedPanel.Diamond(vh,p,4.5f,Tint);}}
 }
 public static class WorldArt {
  static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
  [Serializable] sealed class AtlasFrames {public AtlasFrame[] Frames;}[Serializable] sealed class AtlasFrame {public int X,Y,W,H;}
  static AtlasFrames atlas;
  public static Sprite Find(string key,int index,int columns,int rows){if(sprites.TryGetValue(key,out var s))return s;var t=Resources.Load<Texture2D>("Art/V3/objects");if(!t)return null;float w=t.width/(float)columns,h=t.height/(float)rows;Rect frame=new Rect(index%columns*w,t.height-(index/columns+1)*h,w,h);if(atlas==null){var data=Resources.Load<TextAsset>("Art/V3/atlas-bounds");if(data)atlas=JsonUtility.FromJson<AtlasFrames>(data.text);}if(atlas!=null&&index<atlas.Frames.Length){var f=atlas.Frames[index];frame=new Rect(f.X,f.Y,f.W,f.H);}s=UnityEngine.Sprite.Create(t,frame,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);sprites[key]=s;return s;}
  public static Sprite FindVariant(string key,string path,int index,int columns,int rows,bool trimmed=true){if(sprites.TryGetValue(key,out var result))return result;var texture=Resources.Load<Texture2D>("Art/"+path);if(!texture)return null;float w=texture.width/(float)columns,h=texture.height/(float)rows;var frame=new Rect(index%columns*w,texture.height-(index/columns+1)*h,w,h);if(trimmed){var metadata=Resources.Load<TextAsset>("Art/"+path+"-bounds");if(metadata){var frames=JsonUtility.FromJson<AtlasFrames>(metadata.text).Frames;if(index<frames.Length){var f=frames[index];frame=new Rect(f.X,f.Y,f.W,f.H);}}}result=UnityEngine.Sprite.Create(texture,frame,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);sprites[key]=result;return result;}
  public static void Frame(RectTransform root,float x,float y,float w,float h,bool velvet){
   if(velvet&&Studio.Sprite("V3/case-rim")){var rim=Studio.Image(root,"V3/case-rim",x-24,y-24,w+48,h+48);rim.GetComponent<UnityEngine.UI.Image>().type=UnityEngine.UI.Image.Type.Sliced;return;}if(!velvet)return;var outside=Studio.Box(root,"World frame",x-12,y-12,w+24,h+24,FlatUI.Hex(velvet?"#140D0C":"#142E32"));outside.GetComponentInChildren<CraftedPanel>().BorderOnly=true;
   if(velvet){for(int i=0;i<2;i++){Studio.Box(root,"Brass clasp",x+w*(i==0?.16f:.78f),y-18,27,20,FlatUI.Hex("#C7974C"));Studio.Box(root,"Brass clasp",x+w*(i==0?.16f:.78f),y+h-3,27,20,FlatUI.Hex("#C7974C"));}Studio.Box(root,"Leather handle",x+w*.35f,y+h+2,w*.3f,22,FlatUI.Hex("#20110D"));}
  }
  public static void Link(RectTransform root,float ax,float ay,float bx,float by,Color c,int kind=-1){var r=FlatUI.Rect(root,"Living route",0,0,720,1280);var line=r.gameObject.AddComponent<RouteRibbon>();line.raycastTarget=false;line.A=FlatUI.Stage?FlatUI.Stage.Point(ax,ay):new Vector2(ax-360,640-ay);line.B=FlatUI.Stage?FlatUI.Stage.Point(bx,by):new Vector2(bx-360,640-by);line.Tint=c;line.Kind=kind;line.UsesAuthored=true;line.AuthoredA=new Vector2(ax,ay);line.AuthoredB=new Vector2(bx,by);}
 }
}
