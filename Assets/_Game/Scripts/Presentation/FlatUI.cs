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
    public sealed class PointerTarget:MonoBehaviour,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public Action Click,StartDrag;public Action<Vector2> EndDrag,Drag;
        bool dragged;
        public void OnPointerClick(PointerEventData e){if(!dragged)Click?.Invoke();dragged=false;}
        public void OnBeginDrag(PointerEventData e){dragged=true;StartDrag?.Invoke();}
        public void OnDrag(PointerEventData e){Drag?.Invoke(e.position);}
        public void OnEndDrag(PointerEventData e){EndDrag?.Invoke(e.position);}
    }
    public sealed class SafeCanvas:MonoBehaviour
    {
        public RectTransform Safe;Rect last;int w,h;
        void Update(){if(last==Screen.safeArea&&w==Screen.width&&h==Screen.height)return;last=Screen.safeArea;w=Screen.width;h=Screen.height;Safe.anchorMin=new Vector2(last.x/w,last.y/h);Safe.anchorMax=new Vector2(last.xMax/w,last.yMax/h);Safe.offsetMin=Safe.offsetMax=Vector2.zero;}
    }
    public static class FlatUI
    {
        public static Color Hex(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        public static TMP_FontAsset Font;
        public static RectTransform Root(Color background)
        {
            if(!Font)Font=TMP_FontAsset.CreateFontAsset(Resources.Load<Font>("DejaVuSans"));
            var go=new GameObject("GameCanvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(720,1280);scaler.matchWidthOrHeight=0.5f;
            var fill=Panel(go.transform,"Backdrop",0,0,720,1280,background);fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.offsetMin=fill.offsetMax=Vector2.zero;
            var safe=new GameObject("SafeArea",typeof(RectTransform)).GetComponent<RectTransform>();safe.SetParent(go.transform,false);safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;go.AddComponent<SafeCanvas>().Safe=safe;
            var root=new GameObject("PortraitContent",typeof(RectTransform),typeof(AspectRatioFitter)).GetComponent<RectTransform>();root.SetParent(safe,false);root.GetComponent<AspectRatioFitter>().aspectMode=AspectRatioFitter.AspectMode.FitInParent;root.GetComponent<AspectRatioFitter>().aspectRatio=720f/1280f;
            if(!UnityEngine.Object.FindFirstObjectByType<EventSystem>())new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            return root;
        }
        public static RectTransform Rect(Transform parent,string name,float x,float y,float width,float height)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=new Vector2(x/720f,1-(y+height)/1280f);r.anchorMax=new Vector2((x+width)/720f,1-y/1280f);r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        public static RectTransform Panel(Transform parent,string name,float x,float y,float width,float height,Color color)
        {var r=Rect(parent,name,x,y,width,height);var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;return r;}
        public static TextMeshProUGUI Text(Transform parent,string name,string text,float x,float y,float width,float height,int size,Color color,TextAlignmentOptions align=TextAlignmentOptions.Left)
        {
            var r=Rect(parent,name,x,y,width,height);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=Font;t.text=text;t.color=color;t.fontSize=size;t.enableAutoSizing=true;t.fontSizeMin=size*0.72f;t.fontSizeMax=size;t.alignment=align;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;return t;
        }
        public static UnityEngine.UI.Button Button(Transform parent,string name,string title,float x,float y,float width,float height,Color background,Color foreground,Action click)
        {
            var r=Panel(parent,name,x,y,width,height,background);r.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=r.GetComponent<UnityEngine.UI.Image>();b.onClick.AddListener(()=>click());
            var text=Text(r,"Label",title,26,12,668,1256,24,foreground,TextAlignmentOptions.Center);return b;
        }
        public static PointerTarget Hit(RectTransform parent,Action click,Action start,Action<Vector2> end)
        {parent.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;var h=parent.gameObject.AddComponent<PointerTarget>();h.Click=click;h.StartDrag=start;h.EndDrag=end;return h;}
        public static bool CellAt(RectTransform grid,Vector2 screen,int columns,int rows,out int x,out int y)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(grid,screen,null,out var p);var rect=grid.rect;
            x=Mathf.FloorToInt((p.x-rect.xMin)/rect.width*columns);y=Mathf.FloorToInt((rect.yMax-p.y)/rect.height*rows);return x>=0&&y>=0&&x<columns&&y<rows;
        }
        public static string Arg(string key)
        {var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:null;}
        public static void Capture(string game)
        {
            if(!Debug.isDebugBuild&&!Application.isEditor)return;
            string directory=Arg("-capture-dir");if(directory==null)return;Directory.CreateDirectory(directory);ScreenCapture.CaptureScreenshot(Path.Combine(directory,game+"-"+DateTime.UtcNow.ToString("HHmmssfff")+".png"));
        }
    }
}
