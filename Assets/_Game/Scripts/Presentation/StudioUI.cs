using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using LocalGame.Data;
namespace LocalGame {
 public sealed class PressMotion:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
  Vector3 target=Vector3.one;public void OnPointerDown(PointerEventData e){target=Vector3.one*.965f;}public void OnPointerUp(PointerEventData e){target=Vector3.one;}public void OnPointerExit(PointerEventData e){target=Vector3.one;}
  void Update(){if((transform.localScale-target).sqrMagnitude<.000001f)return;transform.localScale=Vector3.Lerp(transform.localScale,target,1-Mathf.Exp(-Time.unscaledDeltaTime*24));}
 }
 public sealed class Soundtrack:MonoBehaviour {
  AudioSource music;readonly List<AudioSource> voices=new List<AudioSource>();Profile profile;int track=-1;
  public void Setup(Profile p){profile=p;music=gameObject.AddComponent<AudioSource>();music.loop=true;music.volume=.32f;for(int i=0;i<8;i++)voices.Add(gameObject.AddComponent<AudioSource>());PlayTrack(0);}
  public void PlayTrack(int index){if(track==index)return;track=index;music.clip=Resources.Load<AudioClip>("Audio/music"+index);music.Play();Refresh();}
  public void Refresh(){if(music)music.mute=!profile.Music;}
  public void Cue(string name){if(profile==null)return;if(profile.Sound){bool important=name=="win"||name=="escape"||name=="result"||name=="alarm";AudioSource source=important?voices[0]:null;if(!important)for(int i=1;i<voices.Count;i++)if(!voices[i].isPlaying){source=voices[i];break;}if(source){if(important)source.Stop();source.pitch=1;source.volume=.55f;source.PlayOneShot(Resources.Load<AudioClip>("Audio/"+name));}}
   if(profile.Haptics&&(name=="place"||name=="take")){
#if UNITY_ANDROID && !UNITY_EDITOR
    try{using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))using(var vibrator=activity.Call<AndroidJavaObject>("getSystemService","vibrator"))using(var effects=new AndroidJavaClass("android.os.VibrationEffect"))using(var effect=effects.CallStatic<AndroidJavaObject>("createOneShot",12L,40))vibrator.Call("vibrate",effect);}catch(Exception){}
#endif
   }}
  void OnApplicationPause(bool value){if(value){if(music)music.Pause();}else if(music)music.UnPause();}
 }
 public static class Studio {
  static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();static TMP_FontAsset bold,serif;
  public static Action CloseModal;public static Profile Profile;public static Soundtrack Sound;public static bool Velvet;
  public static Color Ink=>FlatUI.Hex(Velvet?"#100D13":"#071B2A");public static Color Panel=>FlatUI.Hex(Velvet?"#2A1C26":"#123442");public static Color Muted=>FlatUI.Hex(Velvet?"#BDAA9D":"#A1BAC0");public static Color Text=>FlatUI.Hex(Velvet?"#F2E5CD":"#F6F0DA");public static Color Accent=>FlatUI.Hex(Velvet?"#DAB66D":new[]{"#62E3BB","#A9D27D","#78BDE6","#D6A3DA","#F3BB60","#81DBD5"}[Profile?.Theme??0]);public static Color Danger=>FlatUI.Hex("#F07583");
  public static Sprite Sprite(string key){if(sprites.TryGetValue(key,out var cached))return cached;var t=Resources.Load<Texture2D>("Art/"+key);if(!t)return null;var s=UnityEngine.Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,key=="round"?new Vector4(24,24,24,24):Vector4.zero);sprites[key]=s;return s;}
  public static RectTransform Image(Transform p,string key,float x,float y,float w,float h,Color? tint=null){var r=FlatUI.Panel(p,key,x,y,w,h,tint??Color.white);var im=r.GetComponent<UnityEngine.UI.Image>();im.sprite=Sprite(key);im.raycastTarget=false;return r;}
  public static RectTransform Box(Transform p,string name,float x,float y,float w,float h,Color? tint=null){var r=Image(p,"round",x,y,w,h,tint??Panel);r.name=name;r.GetComponent<UnityEngine.UI.Image>().type=UnityEngine.UI.Image.Type.Sliced;return r;}
  public static TextMeshProUGUI Label(Transform p,string text,float x,float y,float w,float h,int size=28,Color? color=null,TextAlignmentOptions align=TextAlignmentOptions.Left,bool heavy=false,bool display=false){var t=FlatUI.Text(p,"Text",text,x,y,w,h,size,color??Text,align);t.fontSizeMin=size*.9f;t.enableAutoSizing=true;if(heavy){if(!bold)bold=TMP_FontAsset.CreateFontAsset(Resources.Load<Font>("DejaVuSans-Bold"));t.font=bold;}if(display&&Velvet){if(!serif)serif=TMP_FontAsset.CreateFontAsset(Resources.Load<Font>("DejaVuSerif"));t.font=serif;}return t;}
  public static Button Button(Transform p,string title,float x,float y,float w,float h,Action action,bool primary=false,Color? fill=null){var r=Box(p,title,x,y,w,h,fill??(primary?Accent:Panel));r.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<UnityEngine.UI.Image>();var colors=b.colors;colors.highlightedColor=new Color(.92f,.97f,.95f);colors.pressedColor=new Color(.82f,.88f,.86f);b.colors=colors;b.onClick.AddListener(()=>{Sound?.Cue("tap");action();});if(Profile==null||!Profile.ReducedMotion)r.gameObject.AddComponent<PressMotion>();Label(r,title,28,30,664,1220,26,primary?Ink:Text,TextAlignmentOptions.Center,true);return b;}
  public sealed class DragVisual {
   public RectTransform Rect;public UnityEngine.UI.Image Art;
   public void Move(RectTransform root,Vector2 screen,bool legal){if(!Rect)return;RectTransformUtility.ScreenPointToLocalPointInRectangle(root,screen,null,out var point);Rect.anchoredPosition=point;Art.color=legal?new Color(1,1,1,.86f):new Color(1,.44f,.52f,.68f);}
   public void End(){if(Rect)UnityEngine.Object.Destroy(Rect.gameObject);Rect=null;}
  }
  public static DragVisual Ghost(RectTransform root,string key,int width,int height,int rotation,float cell){var wrap=new GameObject("Drag preview",typeof(RectTransform),typeof(Canvas)).GetComponent<RectTransform>();wrap.SetParent(root,false);wrap.anchorMin=wrap.anchorMax=new Vector2(.5f,.5f);wrap.sizeDelta=Vector2.zero;var canvas=wrap.GetComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=30;var image=new GameObject("Object",typeof(RectTransform),typeof(UnityEngine.UI.Image)).GetComponent<RectTransform>();image.SetParent(wrap,false);image.anchorMin=image.anchorMax=new Vector2(.5f,.5f);image.sizeDelta=new Vector2(width*cell,height*cell);bool odd=rotation%2!=0;image.anchoredPosition=new Vector2(((odd?height:width)*.5f-.5f)*cell,-((odd?width:height)*.5f-.5f)*cell);image.localRotation=Quaternion.Euler(0,0,-rotation*90);var art=image.GetComponent<UnityEngine.UI.Image>();art.sprite=Sprite(key);art.raycastTarget=false;return new DragVisual{Rect=wrap,Art=art};}
  public static void Clear(Transform p){CloseModal=null;foreach(Transform c in p){c.gameObject.SetActive(false);UnityEngine.Object.Destroy(c.gameObject);}}
  public static void Backdrop(RectTransform root){Image(root,"backdrop",0,0,720,1280);}
  public static void Header(Transform p,string eyebrow,string title,string subtitle,Action back){Label(p,eyebrow.ToUpperInvariant(),38,32,540,32,20,Accent,heavy:true);Label(p,title,36,79,540,72,48,display:true,heavy:!Velvet);Label(p,subtitle,38,162,640,65,26,Muted);if(back!=null)Button(p,"‹",588,25,96,96,back);}
  public static RectTransform Modal(RectTransform root,string title,Action close){CloseModal=close;var shade=FlatUI.Panel(root,"Scrim",0,0,720,1280,new Color(0,0,0,.86f));shade.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;var box=Box(shade,"Dialog",25,140,670,1030,Panel);Label(box,title,44,46,510,155,41,Accent,display:Velvet,heavy:!Velvet);Button(box,"×",580,24,112,116,close);return box;}
  public static string Stars(int stars)=>new string('★',stars)+new string('☆',3-stars);
  public static void StarRow(Transform p,int stars,float y){Label(p,Stars(stars),40,y,640,80,58,Accent,TextAlignmentOptions.Center);}
  public static bool Cell(RectTransform grid,Vector2 screen,int width,int height,out int x,out int y)=>FlatUI.CellAt(grid,screen,width,height,out x,out y);
 }
 public sealed class FloatingNumber:MonoBehaviour {
  public float Life=.8f;float age;Vector3 original;CanvasGroup group;void Start(){original=transform.localPosition;group=gameObject.AddComponent<CanvasGroup>();}void Update(){age+=Time.deltaTime;transform.localPosition=original+new Vector3(0,age*40,0);group.alpha=Mathf.Clamp01((Life-age)*3);if(age>Life)Destroy(gameObject);}
 }
}
