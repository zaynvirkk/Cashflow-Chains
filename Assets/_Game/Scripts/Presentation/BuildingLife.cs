using UnityEngine;using UnityEngine.UI;
namespace LocalGame {
 public sealed class BuildingLife:MaskableGraphic {
  public string Kind;public bool Active;float clock;
  void Update(){if(Studio.Profile?.ReducedMotion??false)return;clock+=Time.unscaledDeltaTime;SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(Studio.Profile?.ReducedMotion??false)return;var r=rectTransform.rect;float w=r.width,h=r.height;
   if(Kind=="park"){for(int i=0;i<5;i++){float t=Mathf.Repeat(clock*.7f+i*.2f,1);var p=new Vector2(r.xMin+w*(.5f+Mathf.Sin(i*2.4f)*t*.1f),r.yMin+h*(.44f+Mathf.Sin(t*Mathf.PI)*.18f));CraftedPanel.Diamond(vh,p,w*.014f,new Color(.55f,.94f,1,(1-t)*.8f));}return;}
   if(Kind=="cafe"||Kind=="bakery"||Active&&(Kind=="factory"||Kind=="recycler"||Kind=="data_center")){for(int i=0;i<3;i++){float t=Mathf.Repeat(clock*.4f+i/3f,1);float x=.55f+Mathf.Sin(t*4+i)*.025f,y=.82f+t*.2f;float radius=w*(.024f+t*.025f);var p=new Vector2(r.xMin+w*x,r.yMin+h*y);Color c=new Color(.9f,.96f,1,(1-t)*.26f);for(int k=0;k<3;k++)CraftedPanel.Diamond(vh,p+new Vector2(k*radius*.6f,Mathf.Sin(k*2)*radius*.3f),radius,c);}}
   if(Active){float intensity=.18f+.12f*Mathf.Sin(clock*2.4f);CraftedPanel.Polygon(vh,new Rect(r.xMin+w*.36f,r.yMin+h*.21f,w*.08f,h*.09f),new Color(1,.87f,.4f,intensity),1);CraftedPanel.Polygon(vh,new Rect(r.xMin+w*.56f,r.yMin+h*.21f,w*.08f,h*.09f),new Color(1,.87f,.4f,intensity),1);}
  }
 }
}
