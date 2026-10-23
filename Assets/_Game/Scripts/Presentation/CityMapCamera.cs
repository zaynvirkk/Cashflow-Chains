using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
namespace CashflowChains.Presentation {
 public sealed class CityMapCamera:MonoBehaviour {
  public const float MaximumZoom=1.6f;
  public float FitZoom=>Mathf.Min(Viewport.rect.width/(FocusSize.x>0?FocusSize.x:Content.rect.width),Viewport.rect.height/(FocusSize.y>0?FocusSize.y:Content.rect.height))*.94f;
  public float MinimumZoom=>Mathf.Clamp(FitZoom*.65f,.3f,MaximumZoom);
  public RectTransform Viewport,Content;public float Zoom=1;public Vector2 FocusSize,FocusOffset;public bool FollowFit=true;Vector2 previousViewport;public Action<float,Vector2> Changed;Vector2 previous;float pinch;bool dragging;
  public void Apply(){Zoom=Mathf.Clamp(Zoom,MinimumZoom,MaximumZoom);Content.localScale=Vector3.one*Zoom;var limit=new Vector2((Content.rect.width*Zoom+Viewport.rect.width)/2-80,(Content.rect.height*Zoom+Viewport.rect.height)/2-80);var p=Content.anchoredPosition;Content.anchoredPosition=new Vector2(Mathf.Clamp(p.x,-limit.x,limit.x),Mathf.Clamp(p.y,-limit.y,limit.y));}
  public void Scale(float factor){FollowFit=false;Zoom*=factor;Apply();Changed?.Invoke(Zoom,Content.anchoredPosition);}
  public void Fit(){FollowFit=true;Canvas.ForceUpdateCanvases();Zoom=FitZoom;Content.anchoredPosition=-FocusOffset*Zoom;Apply();Changed?.Invoke(Zoom,Content.anchoredPosition);}
  public void Begin(Vector2 screen){FollowFit=false;RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport,screen,null,out previous);dragging=true;}
  public void Drag(Vector2 screen){if(!dragging)return;RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport,screen,null,out var local);Content.anchoredPosition+=local-previous;previous=local;Apply();}
  public void End(){dragging=false;Changed?.Invoke(Zoom,Content.anchoredPosition);}
  void Start(){previousViewport=Viewport.rect.size;Apply();}
  void Update(){if(!Viewport||!Content)return;if(Viewport.rect.size!=previousViewport){previousViewport=Viewport.rect.size;if(FollowFit)Fit();else{Apply();Changed?.Invoke(Zoom,Content.anchoredPosition);}}var mouse=Mouse.current;if(mouse!=null&&RectTransformUtility.RectangleContainsScreenPoint(Viewport,mouse.position.ReadValue())){float scroll=mouse.scroll.ReadValue().y;if(Mathf.Abs(scroll)>.1f)Scale(Mathf.Exp(scroll*.001f));}var touch=Touchscreen.current;var fingers=touch?.touches.Where(t=>t.press.isPressed).Take(2).ToArray();if(fingers!=null&&fingers.Length==2){float distance=Vector2.Distance(fingers[0].position.ReadValue(),fingers[1].position.ReadValue());if(pinch>0&&distance>0)Scale(distance/pinch);pinch=distance;}else pinch=0;}
 }
 public sealed class CityLandGateVisibility:MonoBehaviour {
  public RectTransform Viewport;CanvasGroup group;readonly Vector3[] corners=new Vector3[4];
  void LateUpdate(){if(!Viewport)return;if(!group)group=gameObject.AddComponent<CanvasGroup>();((RectTransform)transform).GetWorldCorners(corners);bool visible=true;var area=Viewport.rect;foreach(var corner in corners){var p=Viewport.InverseTransformPoint(corner);if(p.x<area.xMin+2||p.x>area.xMax-2||p.y<area.yMin+2||p.y>area.yMax-2)visible=false;}group.alpha=visible?1:0;group.interactable=group.blocksRaycasts=visible;}
 }

}
