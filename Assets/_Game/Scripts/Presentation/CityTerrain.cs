using LocalGame;
using UnityEngine;
using UnityEngine.UI;
namespace CashflowChains.Presentation {
 public sealed class CityTerrain:MaskableGraphic {
  public int Columns,Rows;public float Cell;public CityRoads Traffic;
  void Update(){if(!(Studio.Profile?.ReducedMotion??false))SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;Color asphalt=FlatUI.Hex("#1E3441"),walk=FlatUI.Hex("#4B6B62"),paint=new Color(1,.87f,.6f,.5f);float left=r.xMin+Cell/2,top=r.yMax-Cell/2;
   for(int x=0;x<=Columns;x++){float at=left+x*Cell;CraftedPanel.Line(vh,new Vector2(at,top+16),new Vector2(at,top-Rows*Cell-16),28,walk);CraftedPanel.Line(vh,new Vector2(at,top+15),new Vector2(at,top-Rows*Cell-15),25,asphalt);}
   for(int y=0;y<=Rows;y++){float at=top-y*Cell;CraftedPanel.Line(vh,new Vector2(left-16,at),new Vector2(left+Columns*Cell+16,at),28,walk);CraftedPanel.Line(vh,new Vector2(left-15,at),new Vector2(left+Columns*Cell+15,at),25,asphalt);}
   for(int x=1;x<Columns;x++)for(int y=1;y<Rows;y++){Vector2 p=new Vector2(left+x*Cell,top-y*Cell);for(int side=-1;side<=1;side+=2)for(int stripe=-2;stripe<=2;stripe++){CraftedPanel.Line(vh,p+new Vector2(side*18,stripe*3),p+new Vector2(side*23,stripe*3),1.1f,new Color(.92f,.95f,.88f,.7f));CraftedPanel.Line(vh,p+new Vector2(stripe*3,side*18),p+new Vector2(stripe*3,side*23),1.1f,new Color(.92f,.95f,.88f,.7f));}double phase=(Traffic?.SimulationTime??0)%9;CraftedPanel.Diamond(vh,p+new Vector2(-18,-18),2.5f,FlatUI.Hex(phase<3.5?"#8BFFC3":"#FF857B"));CraftedPanel.Diamond(vh,p+new Vector2(18,18),2.5f,FlatUI.Hex(phase>=4.5&&phase<8?"#8BFFC3":"#FF857B"));}
  }
 }
}
