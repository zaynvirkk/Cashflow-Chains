using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LocalGame
{
    public sealed class PointerTarget:MonoBehaviour,IPointerDownHandler,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public Action Click,StartDrag;public Action<Vector2> EndDrag,Drag;
        bool dragged;public void OnPointerDown(PointerEventData e){FlatUI.LastPress=e.position;}
        public void OnPointerClick(PointerEventData e){if(!dragged)Click?.Invoke();dragged=false;}
        public void OnBeginDrag(PointerEventData e){var button=GetComponent<UnityEngine.UI.Button>();if(button&&!button.IsInteractable())return;dragged=true;StartDrag?.Invoke();}
        public void OnDrag(PointerEventData e){if(dragged)Drag?.Invoke(e.position);}
        public void OnEndDrag(PointerEventData e){if(dragged)EndDrag?.Invoke(e.position);}
    }
    public sealed class SafeCanvas:MonoBehaviour
    {
        public RectTransform Safe;Rect last;int w,h;
        void Update(){if(Screen.width<1||Screen.height<1)return;if(last==Screen.safeArea&&w==Screen.width&&h==Screen.height)return;last=Screen.safeArea;w=Screen.width;h=Screen.height;Safe.anchorMin=new Vector2(last.x/w,last.y/h);Safe.anchorMax=new Vector2(last.xMax/w,last.yMax/h);Safe.offsetMin=Safe.offsetMax=Vector2.zero;}
    }
    public sealed class PortraitFit:MonoBehaviour
    {
        public RectTransform Safe;
        void LateUpdate(){if(!Safe)return;float fit=Mathf.Min(Safe.rect.width/720f,Safe.rect.height/1280f);if(fit>0)transform.localScale=new Vector3(fit,fit,1);}
    }
    public static class FlatUI
    {
        public static Color Hex(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        public static TMP_FontAsset Font;public static ResponsiveStage Stage;public static string InputScreen;public static Vector2 LastPress;
        public static RectTransform Root(Color background)
        {
            if(!Font)Font=TMP_FontAsset.CreateFontAsset(Resources.Load<Font>("Fonts/V3/AlegreyaSans-Regular")??Resources.Load<Font>("DejaVuSans"));
            var go=new GameObject("GameCanvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(720,1280);scaler.matchWidthOrHeight=Screen.width>Screen.height&&Screen.width/(float)Screen.height>=1280f/720f?1:0;scaler.referenceResolution=Screen.width>Screen.height?new Vector2(1280,720):new Vector2(720,1280);go.AddComponent<ResponsiveCanvas>().Scaler=scaler;
            var fill=Panel(go.transform,"Backdrop",0,0,720,1280,background);fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.offsetMin=fill.offsetMax=Vector2.zero;
            var safe=new GameObject("SafeArea",typeof(RectTransform)).GetComponent<RectTransform>();safe.SetParent(go.transform,false);safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;go.AddComponent<SafeCanvas>().Safe=safe;
            var world=new GameObject("Painted world",typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(WorldCover)).GetComponent<RectTransform>();world.SetParent(go.transform,false);world.SetSiblingIndex(1);world.anchorMin=world.anchorMax=new Vector2(.5f,.5f);var picture=world.GetComponent<UnityEngine.UI.Image>();picture.sprite=Studio.Sprite("V3/world-portrait");picture.raycastTarget=false;world.GetComponent<WorldCover>().Frame=go.GetComponent<RectTransform>();var root=new GameObject("ResponsiveContent",typeof(RectTransform),typeof(ResponsiveStage)).GetComponent<RectTransform>();root.SetParent(safe,false);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;Stage=root.GetComponent<ResponsiveStage>();root.gameObject.AddComponent<PresentationMap>();Canvas.ForceUpdateCanvases();
            if(!UnityEngine.Object.FindFirstObjectByType<EventSystem>())new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            return root;
        }
        public static RectTransform Rect(Transform parent,string name,float x,float y,float width,float height)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.gameObject.AddComponent<AuthoredRect>().Design=new Rect(x,y,width,height);r.anchorMin=new Vector2(x/720f,1-(y+height)/1280f);r.anchorMax=new Vector2((x+width)/720f,1-y/1280f);r.offsetMin=r.offsetMax=Vector2.zero;var stage=parent.GetComponent<ResponsiveStage>();if(stage){var item=r.gameObject.AddComponent<StageElement>();item.Stage=stage;item.Design=new Rect(x,y,width,height);item.Apply();}return r;
        }
        public static RectTransform Panel(Transform parent,string name,float x,float y,float width,float height,Color color)
        {var r=Rect(parent,name,x,y,width,height);var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;return r;}
        public static TextMeshProUGUI Text(Transform parent,string name,string text,float x,float y,float width,float height,int size,Color color,TextAlignmentOptions align=TextAlignmentOptions.Left)
        {
            var r=Rect(parent,name,x,y,width,height);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=Font;t.text=text;t.color=color;t.fontSize=size;t.enableAutoSizing=true;t.fontSizeMin=size*0.72f;t.fontSizeMax=size;t.alignment=align;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;var stage=parent.GetComponentInParent<ResponsiveStage>();if(stage){var label=r.gameObject.AddComponent<StageText>();label.Stage=stage;label.Size=size;label.Apply();}return t;
        }
        public static UnityEngine.UI.Button Button(Transform parent,string name,string title,float x,float y,float width,float height,Color background,Color foreground,Action click)
        {
            var r=Panel(parent,name,x,y,width,height,background);r.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=r.GetComponent<UnityEngine.UI.Image>();b.onClick.AddListener(()=>click());
            var text=Text(r,"Label",title,26,12,668,1256,24,foreground,TextAlignmentOptions.Center);return b;
        }
        public static PointerTarget Hit(RectTransform parent,Action click,Action start,Action<Vector2> end)
        {parent.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;var h=parent.gameObject.AddComponent<PointerTarget>();parent.gameObject.AddComponent<PressMotion>();h.Click=click;h.StartDrag=start;h.EndDrag=end;return h;}
        public static bool CellAt(RectTransform grid,Vector2 screen,int columns,int rows,out int x,out int y)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(grid,screen,null,out var p);var rect=grid.rect;
            x=Mathf.FloorToInt((p.x-rect.xMin)/rect.width*columns);y=Mathf.FloorToInt((rect.yMax-p.y)/rect.height*rows);return x>=0&&y>=0&&x<columns&&y<rows;
        }
        public static string Arg(string key)
        {var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:null;}
        public static void Capture(string game)
        {
            string directory=Arg("-capture-dir");if(directory==null)return;Directory.CreateDirectory(directory);ScreenCapture.CaptureScreenshot(Path.Combine(directory,game+"-"+DateTime.UtcNow.ToString("HHmmssfff")+".png"));
        }
    }
}
