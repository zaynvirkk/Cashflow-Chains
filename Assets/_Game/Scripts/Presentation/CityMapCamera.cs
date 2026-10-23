using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
namespace CashflowChains.Presentation {
 public sealed class CityMapCamera:MonoBehaviour {
  public RectTransform Viewport,Content;public float Zoom=1;public Vector2 FocusSize,FocusOffset;public bool FollowFit=true;Vector2 previousViewport;public Action<float,Vector2> Changed;Vector2 previous;float pinch;bool dragging;
  public void Apply(){Zoom=Mathf.Clamp(Zoom,.3f,1.8f);Content.localScale=Vector3.one*Zoom;var limit=new Vector2((Content.rect.width*Zoom+Viewport.rect.width)/2-80,(Content.rect.height*Zoom+Viewport.rect.height)/2-80);var p=Content.anchoredPosition;Content.anchoredPosition=new Vector2(Mathf.Clamp(p.x,-limit.x,limit.x),Mathf.Clamp(p.y,-limit.y,limit.y));}
  public void Scale(float factor){FollowFit=false;Zoom*=factor;Apply();Changed?.Invoke(Zoom,Content.anchoredPosition);}
  public void Fit(){FollowFit=true;Canvas.ForceUpdateCanvases();var size=FocusSize.x>0?FocusSize:Content.rect.size;Zoom=Mathf.Min(Viewport.rect.width/size.x,Viewport.rect.height/size.y)*.94f;Content.anchoredPosition=-FocusOffset*Zoom;Apply();Changed?.Invoke(Zoom,Content.anchoredPosition);}
  public void Begin(Vector2 screen){FollowFit=false;RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport,screen,null,out previous);dragging=true;}
  public void Drag(Vector2 screen){if(!dragging)return;RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport,screen,null,out var local);Content.anchoredPosition+=local-previous;previous=local;Apply();}
  public void End(){dragging=false;Changed?.Invoke(Zoom,Content.anchoredPosition);}
  void Start(){previousViewport=Viewport.rect.size;Apply();}
  void Update(){if(!Viewport||!Content)return;if(Viewport.rect.size!=previousViewport){previousViewport=Viewport.rect.size;if(FollowFit)Fit();}var mouse=Mouse.current;if(mouse!=null&&RectTransformUtility.RectangleContainsScreenPoint(Viewport,mouse.position.ReadValue())){float scroll=mouse.scroll.ReadValue().y;if(Mathf.Abs(scroll)>.1f)Scale(Mathf.Exp(scroll*.001f));}var touch=Touchscreen.current;var fingers=touch?.touches.Where(t=>t.press.isPressed).Take(2).ToArray();if(fingers!=null&&fingers.Length==2){float distance=Vector2.Distance(fingers[0].position.ReadValue(),fingers[1].position.ReadValue());if(pinch>0&&distance>0)Scale(distance/pinch);pinch=distance;}else pinch=0;}
 }
}
