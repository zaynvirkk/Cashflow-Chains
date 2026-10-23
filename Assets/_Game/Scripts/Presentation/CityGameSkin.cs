using UnityEngine;
using UnityEngine.UI;
namespace LocalGame {
 public static class CityGameSkin {
  public static void Draw(VertexHelper vh,Rect r,Color fill,bool primary,float heat){
   if(fill.a<.03f)return;float radius=Mathf.Min(22,r.height*.23f);for(int i=4;i>=1;i--)Round(vh,new Rect(r.x-i*.7f,r.y-4-i*.8f,r.width+i*1.4f,r.height+i*1.1f),radius+i,new Color(.005f,.02f,.035f,.045f),new Color(.005f,.02f,.035f,.045f));
   bool gold=primary&&fill.r>.7f&&fill.g>.5f&&fill.b<.5f;var edge=gold?FlatUI.Hex("#A96900"):Color.Lerp(fill,FlatUI.Hex("#4D8E9D"),.42f);Round(vh,r,radius,edge,edge);
   var inside=new Rect(r.x+2,r.y+(primary?6:2),r.width-4,r.height-(primary?8:4));Color top=gold?FlatUI.Hex("#FFF17B"):Color.Lerp(fill,FlatUI.Hex("#598394"),.17f),bottom=gold?FlatUI.Hex("#FFBF20"):fill;top.a=bottom.a=fill.a;Round(vh,inside,radius-2,bottom,Color.Lerp(top,Color.white,heat*.08f));
   if(primary){var glint=new Rect(inside.x+5,inside.yMax-5,inside.width-10,1.2f);Round(vh,glint,.5f,new Color(1,1,.9f,.65f),new Color(1,1,.9f,.65f));}
  }
  public static void Round(VertexHelper vh,Rect r,float radius,Color bottom,Color top){radius=Mathf.Clamp(radius,0,Mathf.Min(r.width,r.height)/2);int start=vh.currentVertCount;vh.AddVert(r.center,Color.Lerp(bottom,top,.5f),Vector2.zero);
   for(int corner=0;corner<4;corner++){Vector2 center=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);for(int i=0;i<=6;i++){float angle=(corner*90+i*15)*Mathf.Deg2Rad;Vector2 p=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;vh.AddVert(p,Color.Lerp(bottom,top,Mathf.Clamp01((p.y-r.yMin)/Mathf.Max(1,r.height))),Vector2.zero);}}
   for(int i=0;i<28;i++)vh.AddTriangle(start,start+1+i,start+1+(i+1)%28);
  }
 }
}
