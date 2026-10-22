using TMPro;using UnityEngine;
namespace LocalGame {
 public sealed class TileCaptionLayout:MonoBehaviour {
  public TextMeshProUGUI Name,Value;bool first=true,compact;
  void LateUpdate(){bool next=((RectTransform)transform).rect.width<85||FlatUI.Stage&&!FlatUI.Stage.Wide&&FlatUI.Stage.Height<1120;if(!first&&next==compact)return;first=false;compact=next;Value.enabled=!compact;var r=Name.rectTransform;float y=compact?855:800,h=compact?415:270;r.anchorMin=new Vector2(20/720f,1-(y+h)/1280);r.anchorMax=new Vector2(700/720f,1-y/1280);r.offsetMin=r.offsetMax=Vector2.zero;}
 }
}
