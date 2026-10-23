using TMPro;using UnityEngine;
namespace LocalGame {
 public sealed class TileCaptionLayout:MonoBehaviour {
  public TextMeshProUGUI Name,Value;bool first=true,compact,narrow;string fullName;
  void LateUpdate(){float width=((RectTransform)transform).rect.width;bool next=width<85,small=width<60;if(!first&&next==compact&&small==narrow)return;if(first)fullName=Name.text;first=false;compact=next;narrow=small;Name.text=compact?CompactName(fullName,small):fullName;Value.enabled=!compact;var r=Name.rectTransform;float y=compact?855:800,h=compact?415:270;r.anchorMin=new Vector2(20/720f,1-(y+h)/1280);r.anchorMax=new Vector2(700/720f,1-y/1280);r.offsetMin=r.offsetMax=Vector2.zero;}
  static string CompactName(string name,bool small){switch(name){case "RESIDENTS":return small?"HOMES":"PEOPLE";case "WAREHOUSE":return "DEPOT";case "LOGISTICS":return "FLEET";case "GREENHOUSE":return small?"FARM":"GROWER";case "UNIVERSITY":return small?"UNI":"CAMPUS";case "FREIGHT YARD":return small?"YARD":"FREIGHT";case "RECYCLER":return small?"REUSE":"RECYCLE";case "EXCHANGE":return small?"TRADE":"TRADING";case "FACTORY":return small?"WORKS":name;default:return name;}}
 }
}
