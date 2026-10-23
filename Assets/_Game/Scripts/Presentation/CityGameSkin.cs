using UnityEngine;
using UnityEngine.UI;
namespace LocalGame {
 public static class CityGameSkin {
  public static void Draw(VertexHelper vh,Rect r,Color fill,bool primary,float heat){
   if(fill.a<.03f)return;float radius=Mathf.Min(primary?18:20,r.height*.20f);bool gold=primary&&fill.r>.7f&&fill.g>.5f&&fill.b<.5f;
   Round(vh,new Rect(r.x-2,r.y-5,r.width+4,r.height+5),radius+2,new Color(.01f,.025f,.07f,.65f),new Color(.01f,.025f,.07f,.65f));
   Color dark=gold?FlatUI.Hex("#A65D09"):FlatUI.Hex("#071936");Round(vh,r,radius,dark,dark);
   var rim=new Rect(r.x+1,r.y+4,r.width-2,r.height-5);Round(vh,rim,radius-1,gold?FlatUI.Hex("#F3A911"):FlatUI.Hex("#2166A0"),gold?FlatUI.Hex("#FFF6B4"):FlatUI.Hex("#78C6F4"));
   float inset=Mathf.Min(4,r.height*.05f),lip=primary?Mathf.Min(9,r.height*.16f):Mathf.Min(5,r.height*.06f);var face=new Rect(r.x+inset,r.y+lip,r.width-inset*2,r.height-lip-inset);Color bottom=gold?FlatUI.Hex("#FFA917"):Color.Lerp(fill,FlatUI.Hex("#103471"),.5f),top=gold?FlatUI.Hex("#FFE96B"):Color.Lerp(fill,FlatUI.Hex("#3679BB"),r.height<160?.45f:.25f);bottom.a=top.a=fill.a;
   Round(vh,face,Mathf.Max(3,radius-4),bottom,Color.Lerp(top,Color.white,heat*.12f));
   var shine=new Rect(face.x+3,face.yMax-face.height*.36f,face.width-6,face.height*.34f);Round(vh,shine,Mathf.Max(2,radius-6),new Color(1,1,1,0),new Color(1,1,1,gold?.22f:r.height<160?.065f:.03f));
   if(r.width>110){var line=new Rect(face.x+radius*.5f,face.yMax-2,face.width-radius,1.1f);Round(vh,line,.4f,new Color(1,1,1,gold?.72f:.22f),new Color(1,1,1,gold?.72f:.22f));}
  }
  public static void Round(VertexHelper vh,Rect r,float radius,Color bottom,Color top){radius=Mathf.Clamp(radius,0,Mathf.Min(r.width,r.height)/2);int start=vh.currentVertCount;vh.AddVert(r.center,Color.Lerp(bottom,top,.5f),Vector2.zero);
   for(int corner=0;corner<4;corner++){Vector2 center=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);for(int i=0;i<=6;i++){float angle=(corner*90+i*15)*Mathf.Deg2Rad;Vector2 p=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;vh.AddVert(p,Color.Lerp(bottom,top,Mathf.Clamp01((p.y-r.yMin)/Mathf.Max(1,r.height))),Vector2.zero);}}
   for(int i=0;i<28;i++)vh.AddTriangle(start,start+1+i,start+1+(i+1)%28);
  }
 }
 public sealed class CityMoneyGlyph:MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;float h=r.height*.66f;var back=new Rect(r.x+3,r.center.y-h/2-3,r.width-3,h);CityGameSkin.Round(vh,back,3,FlatUI.Hex("#095E3B"),FlatUI.Hex("#23A75A"));var face=new Rect(r.x,r.center.y-h/2+3,r.width-3,h);CityGameSkin.Round(vh,face,3,FlatUI.Hex("#25B95A"),FlatUI.Hex("#8CF487"));var inside=new Rect(face.x+3,face.y+3,face.width-6,face.height-6);CityGameSkin.Round(vh,inside,2,FlatUI.Hex("#15954A"),FlatUI.Hex("#3BC66B"));CraftedPanel.Diamond(vh,face.center,Mathf.Min(face.width*.13f,h*.3f),FlatUI.Hex("#CCFFB8"));}
 }

}
