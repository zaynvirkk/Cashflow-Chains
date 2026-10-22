using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
namespace LocalGame {
 public sealed class ResponsiveCanvas:MonoBehaviour {
  public CanvasScaler Scaler;int width,height;
  void Update(){if(width==Screen.width&&height==Screen.height)return;width=Screen.width;height=Screen.height;bool wide=width>height;Scaler.referenceResolution=wide?new Vector2(1280,720):new Vector2(720,1280);Scaler.matchWidthOrHeight=wide?1:0;}
 }
 public sealed class ResponsiveStage:MonoBehaviour {
  public string View="title";RectTransform rect;Vector2 previous;string previousView;public bool Wide=>Width>Height;public float Width=>Mathf.Max(1,((RectTransform)transform).rect.width);public float Height=>Mathf.Max(1,((RectTransform)transform).rect.height);
  public float FontScale=>Wide&&View!="board"&&View!="title"&&View!="menu"&&View!="settings"&&View!="map"&&View!="hub"?Mathf.Clamp(Height/1280f,.5f,1):1;
  void Update(){var k=Keyboard.current;if(k!=null&&k.f11Key.wasPressedThisFrame){if(Screen.fullScreen)Screen.SetResolution(1280,720,FullScreenMode.Windowed);else Screen.SetResolution(Display.main.systemWidth,Display.main.systemHeight,FullScreenMode.FullScreenWindow);}}
  void LateUpdate(){if(!rect)rect=(RectTransform)transform;var size=rect.rect.size;if(size==previous&&previousView==View)return;previous=size;previousView=View;foreach(var e in GetComponentsInChildren<StageElement>(true))e.Apply();foreach(var t in GetComponentsInChildren<StageText>(true))t.Apply();}
  public Rect BoardArea {
   get{float sourceY=Studio.Velvet?260:310,sourceH=Studio.Velvet?690:644;if(Wide){float scale=Mathf.Min((Width-610)/644,(Height-(Studio.Velvet?215:70))/sourceH);scale=Mathf.Max(.35f,scale);return new Rect((Width-644*scale)/2,(Height-sourceH*scale)/2+(Studio.Velvet?65:0),644*scale,sourceH*scale);}float top=224,lower=Height-430;float side=Mathf.Min(Width-48,lower-top);side=Mathf.Max(260,side);return new Rect((Width-side)/2,top+Mathf.Max(0,(lower-top-side)/2),side,side);}

  }
  public Rect OnBoard(Rect r){var b=BoardArea;float sourceY=Studio.Velvet?260:310;float s=b.width/644;return new Rect(b.x+(r.x-38)*s,b.y+(r.y-sourceY)*s,r.width*s,r.height*s);}
  public Vector2 Point(float x,float y,bool board=true){if(View=="title"&&x>=80&&y>=400&&y<770){var title=Layout("Title connection",new Rect(x,y,0,0));return new Vector2(title.x-Width/2,Height/2-title.y);}if(board&&View=="board"){var r=OnBoard(new Rect(x,y,0,0));return new Vector2(r.x-Width/2,Height/2-r.y);}return new Vector2(x/720*Width-Width/2,Height/2-y/1280*Height);}
  public Rect Layout(string name,Rect r){float w=Width,h=Height,extra=h-1280;bool board=View=="board";if(board&&Wide&&name=="City wallet")return new Rect(28,386,252,24);
   if(name=="Title wordmark")return Wide?new Rect(52,45,w*.42f,245):new Rect(38,60,644,270);
   if(name=="Living route"||name=="Scrim")return new Rect(0,0,w,h);
   if(name=="Reading shade")return Wide?(board?new Rect(0,0,292,h):View=="title"?new Rect(0,0,w*.48f,h):new Rect(0,0,w,210)):new Rect(0,0,w,260*Mathf.Min(1,h/1280));
   if(name=="Control shade")return Wide?(board?new Rect(w-292,0,292,h):new Rect(0,h-175,w,175)):new Rect(0,966+extra,w,314);
   if(board&&name=="Ledger backing")return Wide?new Rect(16,103,276,Height-207):Studio.Velvet?new Rect(24,Height-428,672,116):new Rect(24,2,672,134);
   if(board&&!Wide){
    if(name=="City grid"||name=="Velvet case"||name=="World frame"||name.EndsWith("case-rim")||name=="Flow")return OnBoard(r);
    if(name=="Case lid"){var b=BoardArea;return new Rect(b.x-18,b.y-165,b.width+36,170);}
    if(name=="Case inscription"){var b=BoardArea;return new Rect(b.x+40,b.y-120,b.width-80,80);}
    if(name=="Selected treasure")return new Rect(516,390,184,360);
    if(name=="Coach")return new Rect(38,Height-360,450,95);
    if(name=="Game brand")return new Rect(24,8,Studio.Velvet?170:290,100);
    if(!Studio.Velvet){
     if(r.y<40&&name=="Text")return new Rect(330,18,260,30);
     if(r.y>=60&&r.y<135&&name=="Text")return new Rect(330,53,260,45);
     if(r.x==588&&r.y==38)return new Rect(624,18,72,72);
     if(r.y==142)return new Rect(r.x>400?404:32,106,r.x>400?280:362,32);
     if(name=="Goal")return new Rect(24,148,672,66);
     if(name=="Inspector")return new Rect(24,Height-354,558,128);
     if(r.y==974&&r.x>500)return new Rect(592,Height-354,104,128);
     if(r.y==1094)return new Rect(30,Height-421,660,56);
     if(r.y==1152){if(r.x>450)return new Rect(72,Height-208,576,96);int i=r.x<100?0:r.x<250?1:2;return new Rect(24+i*168,Height-88,158,66);}
     if(name=="City desk")return new Rect(528,Height-88,168,66);
    }else{
     if(r.y<40&&name=="Text")return new Rect(210,22,370,30);
     if(r.y>=60&&r.y<135&&name=="Text")return new Rect(210,64,370,58);
     if(r.x==588&&r.y==38)return new Rect(624,20,72,72);
     if(r.y==143)return new Rect(38,Height-412,320,52);
     if(r.y==146)return new Rect(390,Height-408,292,40);
     if(r.y==212)return new Rect(38,Height-350,134,32);
     if(r.y==219)return new Rect(r.x,Height-342,r.width,r.height);
     if(r.y==205)return new Rect(422,Height-353,260,45);
     if(name=="Shelf")return new Rect(30,Height-280,660,120);
     if(r.y==972)return new Rect(r.x,Height-280,r.width,r.height);
     if(r.y>=1090&&r.y<1150)return new Rect(38,Height-155,644,55);
     if(r.y==1160){if(r.x<210)return new Rect(516,760,184,65);if(r.x<430)return new Rect(516,837,184,75);return new Rect(388,Height-88,306,72);}
     if(name=="Tool roll")return new Rect(30,Height-88,340,72);
    }
   }
   if(board&&Wide){if(name=="Case lid"){var b=BoardArea;return new Rect(b.x-18,b.y-148,b.width+36,153);}if(name=="Case inscription"){var b=BoardArea;return new Rect(b.x+20,b.y-112,b.width-40,70);}if(name=="Selected treasure")return new Rect(Width-285,100,260,210);if(name=="Game brand")return new Rect(28,16,230,85);if(name=="City desk")return new Rect(28,Height-80,252,56);if(name=="Tool roll")return new Rect(28,Height-80,252,56);if(name=="Coach")return new Rect(28,150,252,90);}
   if(!Wide){if(Mathf.Abs(w-720)<1&&Mathf.Abs(h-1280)<1)return r;
    if(board){if(name=="City grid"||name=="Velvet case"||name=="World frame"||name.EndsWith("case-rim")||name=="Flow"||(r.y>=310&&r.y<954&&r.width<180))return OnBoard(r);if(r.y>=966)return new Rect(r.x,r.y+extra,r.width,r.height);float s=Mathf.Min(1,h/1280);return new Rect(r.x,r.y*s,r.width,r.height*s);}
    if(View=="title"){float offset=r.y>=1000?extra:r.y>=770?extra*.65f:r.y>=400?extra*.35f:extra*.1f;return new Rect(r.x,r.y+offset,r.width,r.height);}
    return new Rect(r.x,r.y*h/1280,r.width,r.height*h/1280);
   }
   if(board){float right=w-290;
    if(name=="City grid"||name=="Velvet case"||name=="World frame"||name.EndsWith("case-rim")||name=="Flow"||(r.y>=310&&r.y<954&&r.width<180))return OnBoard(r);
    if(r.y<40&&name=="Text")return new Rect(28,112,252,36);
    if(r.y>=60&&r.y<135&&name=="Text")return new Rect(28,158,252,75);
    if(r.x==588&&r.y==38)return new Rect(w-86,22,58,58);
    if(!Studio.Velvet){
     if(r.y==142)return new Rect(28,r.x>400?318:252,252,62);
     if(name=="Goal")return new Rect(28,414,252,148);
     if(name=="Inspector")return new Rect(right,120,260,216);
     if(r.y==974&&r.x>500)return new Rect(right,352,260,74);
     if(r.y==1094)return new Rect(right,431,260,98);
     if(r.y==1152){if(r.x<100)return new Rect(w-157,22,58,58);if(r.x<250)return new Rect(right,h-181,126,62);if(r.x<400)return new Rect(right+136,h-181,124,62);return new Rect(right,h-106,260,78);}
    }else{
     if(r.y==143)return new Rect(28,271,252,58);
     if(r.y==146)return new Rect(28,350,252,45);
     if(r.y==212)return new Rect(28,430,252,42);
     if(r.y==219)return new Rect(28+(r.x-180),487,25,15);
     if(r.y==205)return new Rect(28,551,252,68);
     if(name=="Shelf")return new Rect(right,155,260,266);
     if(r.y==972){if(r.x>=590)return new Rect(right,437,260,48);int i=r.x>200?1:0;return new Rect(right,155+i*139,260,127);}
     if(r.y>=1090&&r.y<1150)return new Rect(right,496,260,78);
     if(r.y==1160){if(r.x<210)return new Rect(right,h-137,118,59);if(r.x<430)return new Rect(right+128,h-137,132,59);return new Rect(right,h-67,260,53);}
    }
   }
   if(View=="hub"&&name.StartsWith("District island ")){int i=int.Parse(name.Substring(16));float cw=(w-112)/4,ch=(h-270)/2;return new Rect(38+i%4*(cw+12),235+i/4*(ch+12),cw,ch);}
   if(View=="hub"&&r.y<225){if(r.x==588)return new Rect(w-90,28,58,58);return new Rect(44,r.y*.85f,r.width/720*(w-120),r.height);}
   if(View=="title"){
    if(r.y>=400&&r.y<770){float s=Mathf.Min(w*.47f/720,h*.48f/360);return new Rect(w*.515f+r.x*s,h*.23f+(r.y-400)*s,r.width*s,r.height*s);}
    if(r.y==105||r.y==211||r.y==209)return new Rect(52,r.y<150?85:182,w*.42f,115);
    if(r.y>=330&&r.y<390)return new Rect(55,310,w*.4f,52);
    if(r.y>=770&&r.y<920)return new Rect(55,400,w*.4f,114);
    if(r.y>=930&&r.y<990)return new Rect(55,535,w*.4f,56);
    if(r.y==1016)return new Rect(w*.54f,h-160,w*.41f,70);
    if(r.y==1134)return new Rect(w*.54f+(r.x>300?w*.211f:0),h-77,w*.199f,53);
   }
   if(View=="menu"&&r.y>=258&&r.y<1170&&name!="Text"){int i=Mathf.RoundToInt((r.y-258)/102);float cw=(w-148)/3;return new Rect(48+i%3*(cw+26),244+i/3*139,cw,110);}
   if(View=="settings"&&r.y>=270&&name!="Text"){
    if(r.y<850){int i=Mathf.RoundToInt((r.y-270)/112);float cw=(w-120)/2;return new Rect(44+i%2*(cw+32),238+i/2*114,cw,88);}
    int j=Mathf.RoundToInt((r.y-867)/108);float bw=(w-140)/3;return new Rect(44+j*(bw+26),h-106,bw,68);
   }
   if(View=="map"){
    if(name.StartsWith("district"))return new Rect(40,238,w*.26f,h-333);
    if(r.y>=546&&r.y<1050&&r.width==150){int i=Mathf.RoundToInt((r.y-546)/100)*4+Mathf.RoundToInt((r.x-38)/164);float left=w*.31f,cw=(w*.65f-52)/4,ch=(h-330)/5;return new Rect(left+i%4*(cw+16),236+i/4*ch,cw,ch-10);}
    if(r.y>=546&&r.y<1050&&r.width==204){int i=Mathf.RoundToInt((r.y-546)/126)*3+Mathf.RoundToInt((r.x-38)/220);float left=w*.31f,cw=(w*.65f-52)/4;return new Rect(left+i%4*(cw+16),236+i/4*125,cw,112);}
    if(r.y==1090)return new Rect(40+(r.x>300?170:0),h-77,160,52);
    if(r.y==1201)return new Rect(w*.5f,h-76,w*.46f,52);
   }
   if((View=="menu"||View=="settings"||View=="map")&&r.y<240){if(r.x==588)return new Rect(w-90,28,58,58);return new Rect(44,r.y*.85f,r.width/720*(w-120),r.height);}
   float margin=44,content=w-88;return new Rect(margin+r.x/720*content,r.y/1280*h,r.width/720*content,r.height/1280*h);
  }
 }
 public sealed class StageElement:MonoBehaviour {
  public ResponsiveStage Stage;public Rect Design;public void Apply(){if(!Stage)return;var r=(RectTransform)transform;var b=Stage.Layout(gameObject.name,Design);r.anchorMin=new Vector2(b.x/Stage.Width,1-(b.y+b.height)/Stage.Height);r.anchorMax=new Vector2((b.x+b.width)/Stage.Width,1-b.y/Stage.Height);r.offsetMin=r.offsetMax=Vector2.zero;}
 }
 public sealed class StageText:MonoBehaviour {
  public ResponsiveStage Stage;public float Size;public void Apply(){if(!Stage)return;var t=GetComponent<TMPro.TextMeshProUGUI>();float s=Stage.FontScale;if(Stage.Wide){for(Transform p=transform.parent;p!=null;p=p.parent)if(p.name=="Dialog"){s=Mathf.Min(Stage.Height-60,700)/1030f;break;}}t.fontSize=Size*s;t.fontSizeMax=Size*s;t.fontSizeMin=Size*s*.85f;}
 }
 public sealed class ResponsiveDialog:MonoBehaviour {public ResponsiveStage Stage;void LateUpdate(){if(!Stage)return;var r=(RectTransform)transform;if(Stage.Wide){r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(Mathf.Min(Stage.Width-80,820),Mathf.Min(Stage.Height-60,700));r.anchoredPosition=Vector2.zero;}else{r.anchorMin=new Vector2(25/720f,1-1170/1280f);r.anchorMax=new Vector2(695/720f,1-140/1280f);r.offsetMin=r.offsetMax=Vector2.zero;}}}
 public sealed class WorldCover:MonoBehaviour {
  public RectTransform Frame;Vector2 shift;float impact;
  public void Kick(){impact=.15f;}
  void LateUpdate(){var r=(RectTransform)transform;var im=GetComponent<UnityEngine.UI.Image>();var key=Frame.rect.width>Frame.rect.height?"V3/world-wide":"V3/world-portrait";var sprite=Studio.Sprite(key);if(sprite)im.sprite=sprite;if(!im.sprite||!Frame)return;float ratio=im.sprite.rect.width/im.sprite.rect.height;float w=Mathf.Max(Frame.rect.width,Frame.rect.height*ratio)*1.04f,h=w/ratio; r.sizeDelta=new Vector2(w,h);bool quiet=Studio.Profile?.ReducedMotion??false;var p=Pointer.current;Vector2 point=p==null?new Vector2(Screen.width/2,Screen.height/2):p.position.ReadValue();var target=quiet?Vector2.zero:new Vector2((Mathf.Clamp01(point.x/Mathf.Max(1,Screen.width))-.5f)*-8,(Mathf.Clamp01(point.y/Mathf.Max(1,Screen.height))-.5f)*-8);shift=Vector2.Lerp(shift,target,1-Mathf.Exp(-Time.unscaledDeltaTime*4));impact=Mathf.Max(0,impact-Time.unscaledDeltaTime);r.anchoredPosition=shift+(quiet?Vector2.zero:new Vector2(Mathf.Sin(impact*160),Mathf.Cos(impact*133))*impact*18);}
 }
}
