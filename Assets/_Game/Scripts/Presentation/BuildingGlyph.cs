using UnityEngine;
using UnityEngine.UI;
using CashflowChains.Domain;

namespace CashflowChains.Presentation
{
    public sealed class BuildingGlyph:MaskableGraphic
    {
        public string Kind;public int Rotation;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rect=rectTransform.rect;var p=Pieces.Get(Kind);
            void Box(float x,float y,float w,float h,Color c)
            {int i=vh.currentVertCount;Vector2 P(float a,float b)=>new Vector2(rect.xMin+a*rect.width,rect.yMin+b*rect.height);vh.AddVert(P(x,y),c,Vector2.zero);vh.AddVert(P(x+w,y),c,Vector2.zero);vh.AddVert(P(x+w,y+h),c,Vector2.zero);vh.AddVert(P(x,y+h),c,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
            Color dim=new Color(color.r,color.g,color.b,0.2f);
            Box(.25f,.28f,.5f,.5f,dim);Box(.25f,.75f,.5f,.025f,color);Box(.25f,.28f,.025f,.5f,color);Box(.725f,.28f,.025f,.5f,color);Box(.25f,.28f,.5f,.025f,color);
            if(Kind=="home"||Kind=="office")for(int x=0;x<2;x++)for(int y=0;y<2;y++)Box(.34f+x*.2f,.4f+y*.2f,.12f,.12f,color);
            else if(Kind=="cafe"){Box(.36f,.4f,.25f,.24f,color);Box(.61f,.48f,.08f,.08f,color);Box(.35f,.35f,.34f,.025f,color);}
            else if(Kind=="farm"){for(int y=0;y<3;y++){Box(.34f,.39f+y*.12f,.32f,.04f,color);Box(.42f,.43f+y*.12f,.04f,.05f,color);}}
            else if(Kind=="bank"){for(int x=0;x<3;x++)Box(.34f+x*.13f,.4f,.07f,.23f,color);Box(.31f,.66f,.4f,.03f,color);}
            else {Box(.34f,.4f,.12f,.27f,color);Box(.53f,.4f,.13f,.12f,color);Box(.53f,.58f,.13f,.09f,color);}
            int input=Pieces.Ports(p.Inputs,Rotation),output=Pieces.Ports(p.Outputs,Rotation);
            for(int d=0;d<4;d++)
            {
                bool incoming=(input&(1<<d))!=0,outgoing=(output&(1<<d))!=0;if(!incoming&&!outgoing)continue;
                Color c=outgoing?new Color(.72f,.95f,.4f):new Color(.98f,.74f,.33f);
                if(d==0){Box(.47f,.79f,.06f,.15f,c);Box(.42f,outgoing?.91f:.79f,.16f,.04f,c);}
                if(d==1){Box(.78f,.5f,.15f,.05f,c);Box(outgoing?.9f:.78f,.45f,.04f,.15f,c);}
                if(d==2){Box(.47f,.15f,.06f,.12f,c);Box(.42f,outgoing?.15f:.23f,.16f,.04f,c);}
                if(d==3){Box(.08f,.5f,.15f,.05f,c);Box(outgoing?.08f:.2f,.45f,.04f,.15f,c);}
            }
        }
    }
}
