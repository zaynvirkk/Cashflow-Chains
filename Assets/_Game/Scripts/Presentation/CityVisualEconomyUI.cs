using System;
using System.Linq;
using CashflowChains.City;
using LocalGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CashflowChains.Presentation {
 public sealed partial class MobileGameApp {
  void DrawHighwayAccess(RectTransform parent,CityState city,float cell){
   var sign=new GameObject("Regional highway sign",typeof(RectTransform)).GetComponent<RectTransform>();sign.SetParent(parent,false);sign.anchorMin=sign.anchorMax=sign.pivot=new Vector2(0,1);sign.sizeDelta=new Vector2(cell*1.9f,35);sign.anchoredPosition=new Vector2(cell*.5f+8,-cell*.5f-19);
   var button=Studio.Button(sign,"HIGHWAY  ›",0,0,720,1280,WorldHighwayAccess,false,FlatUI.Hex("#126047"));Studio.Role(button,"HIGHWAY ACCESS");
  }
  void WorldHighwayAccess(){
   var m=Studio.Modal(root,"CONNECTED TO THE REGION",PermanentCityScreen);
   Studio.Image(m,"home",75,300,220,220);Studio.Image(m,"port",425,300,220,220);
   Studio.Label(m,"NEW HOUSEHOLDS",44,530,300,55,25,Studio.Text,TextAlignmentOptions.Center,true);
   Studio.Label(m,"FUTURE TRADE",376,530,300,55,25,Studio.Text,TextAlignmentOptions.Center,true);
   Studio.Label(m,"Regional transport supports this established district.\nOnly assets you own pay you.",44,640,632,140,29,Studio.Text,TextAlignmentOptions.Center);
   Studio.Label(m,"Other owners’ homes already supply customers.\nYou can later invest in a port to expand private trade.",44,845,632,150,26,Studio.Muted,TextAlignmentOptions.Center);
   Studio.Button(m,"BACK TO MY CITY",44,1100,632,125,PermanentCityScreen,true);
  }
  void DrawCityBusinessSnapshot(RectTransform parent,CityProperty p,CityBusinessDefinition d,CityBusinessResult row,CityBuildingJob job){
   if(job!=null){
    Studio.Label(parent,"YOUR BUILDER IS ON IT",44,450,632,70,32,Studio.Text,TextAlignmentOptions.Center,true);
    var track=Studio.Box(parent,"Construction time",60,575,600,40,FlatUI.Hex("#081B38"));
    var fill=FlatUI.Panel(track,"Actual construction progress",0,0,720,1280,FlatUI.Hex("#FFE36C"));
    var remaining=Studio.Label(parent,"",44,665,632,75,32,Studio.Text,TextAlignmentOptions.Center,true);
    var progress=track.gameObject.AddComponent<CityTimeProgress>();progress.Fill=fill;progress.Label=remaining;progress.StartAt=job.StartedAt;progress.FinishAt=job.FinishAt;progress.ReadNow=()=>EmpireNow;
    Studio.Label(parent,d.Population>0?"Residents arrive after completion.":"Your other businesses keep operating.",44,760,632,75,24,Studio.Muted,TextAlignmentOptions.Center);
    if(d.Population==0)remaining.text="BUILDING";
    return;
   }
   if(d.Sales>0){
    if(d.Family=="coffee")Studio.Label(parent,"Beans + cups → coffee",44,399,632,40,23,Studio.Muted,TextAlignmentOptions.Center);
    var people=FlatUI.Rect(parent,"Customers",54,445,110,100).gameObject.AddComponent<CityPeopleGlyph>();people.raycastTarget=false;
    Studio.Image(parent,d.Art??d.Kind,552,438,110,110);
    var exchange=FlatUI.Rect(parent,"Goods out and payment in",190,445,330,100).gameObject.AddComponent<CityExchangeGlyph>();exchange.raycastTarget=false;
    Studio.Label(parent,"PAY "+CityMoney(Math.Max(1,d.Price*(long)p.PriceBasisPoints/10000)),208,433,286,44,22,FlatUI.Hex("#A4F6BA"),TextAlignmentOptions.Center,true);
    Studio.Label(parent,d.Family=="coffee"?"COFFEE":d.Family=="shopping"?"GOODS":"SERVICE",208,511,286,44,22,Studio.Text,TextAlignmentOptions.Center,true);
   }else Studio.Label(parent,d.Population>0?(CityHousing.Units(p.Level)==1?"You own this rental home.":"You own these rental homes."):CityRole(d,p.Level),44,440,632,112,27,Studio.Text,TextAlignmentOptions.Center);
   long revenue=row?.Revenue??0,cost=(row?.Bills??0)+(row?.Imports??0)+(row?.Tax??0),net=row?.Net??0;
   BusinessFigure(parent,"INCOME",CityMoney(revenue),44,Studio.Text);
   var costs=Studio.Button(parent,"",258,580,204,148,()=>WorldBusinessCosts(p.Id),false,FlatUI.Hex("#102B50"));Studio.Role(costs,"COSTS & BILLS");
   Studio.Label(costs.transform,"COSTS  ›",10,60,700,360,22,FlatUI.Hex("#FFE2A5"),TextAlignmentOptions.Center,true);
   Studio.Label(costs.transform,CityMoney(cost),10,450,700,700,33,Studio.Text,TextAlignmentOptions.Center,true);
   BusinessFigure(parent,"NET / DAY",CityMoney(net),472,net>=0?FlatUI.Hex("#A4F6BA"):FlatUI.Hex("#FFE2A5"));
   int customers=row?.Customers??0,employees=row?.Employees??0,households=row?.Households??0,residents=row?.Residents??0;
   string status=d.Population>0?households+(households==1?" household · ":" households · ")+residents+(residents==1?" resident":" residents"):d.Sales>0?customers+(customers==1?" purchase · ":" purchases · ")+employees+(employees==1?" employee":" employees"):employees+(employees==1?" employee · ":" employees · ")+(p.Open?"operating":"paused");
   Studio.Label(parent,(p.Open?"":"PAUSED · ")+status,44,745,632,42,24,Studio.Text,TextAlignmentOptions.Center,true);
   var journeys=empire.ForecastCity().Flows.Where(f=>f.Kind=="CUSTOMER"&&f.Target==p.Id).Select(f=>f.TravelMinutes).ToArray();string timing=journeys.Length==0?"Changes apply tomorrow":journeys.Min()==journeys.Max()?journeys[0]+" min walk per trip":journeys.Min()+"–"+journeys.Max()+" min walk per trip";
   Studio.Label(parent,"Next day · "+timing,44,794,632,35,20,Studio.Muted,TextAlignmentOptions.Center);
  }
  void BusinessFigure(RectTransform parent,string label,string amount,float x,Color tint){
   var box=Studio.Box(parent,label+" figure",x,580,204,148,FlatUI.Hex("#102B50"));
   Studio.Label(box,label,10,60,700,360,22,Studio.Muted,TextAlignmentOptions.Center,true);
   Studio.Label(box,amount,10,450,700,700,33,tint,TextAlignmentOptions.Center,true);
  }
  void DrawVisualCityLedger(RectTransform parent,CityDayResult r,CityState city){
   Studio.Box(parent,"Cashflow summary",24,250,672,540,FlatUI.Hex("#0A285AF5"));
   Studio.Label(parent,"NET "+CityMoney(r.Net)+" / DAY",44,270,632,90,42,r.Net>=0?FlatUI.Hex("#A4F6BA"):FlatUI.Hex("#FFE2A5"),TextAlignmentOptions.Center,true);
   string[] labels={"Sales, rent & exports","Operating bills","Imported supplies","Tax","Loan repayments","Old bills due"};long[] amounts={r.Revenue,r.Bills,r.ImportBills,r.Tax,r.LoanDue,r.PayablesDue};
   for(int i=0;i<labels.Length;i++){float y=390+i*62;Studio.Label(parent,labels[i],44,y,405,53,27,Studio.Text);Studio.Label(parent,CityMoney(amounts[i]),450,y,226,53,29,i==0?FlatUI.Hex("#A4F6BA"):Studio.Text,TextAlignmentOptions.Right,true);}
   var demand=Studio.Box(parent,"Service and capacity",24,815,672,300,FlatUI.Hex("#0A285AF5"));
   LedgerMeter(demand,"COFFEE",r.CoffeeServed,r.CoffeeDemand,"daily needs served",60,FlatUI.Hex("#E9C095"));
   LedgerMeter(demand,"POWER",Math.Min(r.PowerAvailable,r.PowerDemand),r.PowerDemand,"capacity / demand",640,FlatUI.Hex("#8ADFFF"),r.PowerAvailable+" / "+r.PowerDemand);
   Studio.Label(parent,r.Population+" local residents · "+r.Jobs+" private jobs · "+r.Season,44,1130,632,45,25,Studio.Text,TextAlignmentOptions.Center);
  }
  void LedgerMeter(RectTransform parent,string label,int value,int maximum,string caption,float y,Color color,string display=null){
   Studio.Label(parent,label,25,y,320,150,24,Studio.Text,heavy:true);Studio.Label(parent,display??value+" / "+maximum,370,y,325,150,27,Studio.Text,TextAlignmentOptions.Right,true);
   var track=Studio.Box(parent,label+" capacity",25,y+195,670,75,FlatUI.Hex("#071A36"));float ratio=maximum==0?0:Mathf.Clamp01(value/(float)maximum);if(ratio>0)FlatUI.Panel(track,label+" actual ratio",0,0,720*ratio,1280,color);
   Studio.Label(parent,caption,25,y+300,670,170,21,Studio.Muted);
  }
  void DrawCityInvestment(RectTransform parent,CityInvestmentQuote q){
   Studio.Label(parent,"NEXT DAY NET CASHFLOW",44,555,632,45,24,Studio.Muted,TextAlignmentOptions.Center,true);
   var before=Studio.Box(parent,"Current city margin",44,615,292,140,FlatUI.Hex("#102B50"));
   var after=Studio.Box(parent,"City margin after investment",384,615,292,140,FlatUI.Hex("#102B50"));
   Studio.Label(before,"NOW",10,70,700,300,22,Studio.Muted,TextAlignmentOptions.Center,true);Studio.Label(before,CityMoney(q.BeforeNet),10,430,700,700,36,Studio.Text,TextAlignmentOptions.Center,true);
   Studio.Label(after,"AFTER BUILD",10,70,700,300,22,Studio.Muted,TextAlignmentOptions.Center,true);Studio.Label(after,CityMoney(q.AfterNet),10,430,700,700,36,q.AfterNet>=q.BeforeNet?FlatUI.Hex("#A4F6BA"):FlatUI.Hex("#FFE2A5"),TextAlignmentOptions.Center,true);
   Studio.Label(parent,"→",338,650,44,60,34,Studio.Text,TextAlignmentOptions.Center,true);
   Studio.Label(parent,"After build costs · based on current demand",44,770,632,40,21,Studio.Muted,TextAlignmentOptions.Center);
   for(int i=0;i<q.Costs.Length;i++){var item=q.Costs[i];long owned=q.Owned.Single(o=>o.ItemId==item.ItemId).Quantity;float y=830+i*62;Studio.Label(parent,ItemName(item.ItemId),44,y,290,55,26,Studio.Text);Studio.Label(parent,owned+" / "+item.Quantity,345,y,331,55,28,owned>=item.Quantity?Studio.Text:FlatUI.Hex("#FFE2A5"),TextAlignmentOptions.Right,true);}
   Studio.Label(parent,EmpireDuration(q.Seconds)+" build · labor & permit included",44,1035,632,45,24,Studio.Muted,TextAlignmentOptions.Center);
  }
  void WorldBusinessCosts(int id){
   var p=empire.Snapshot.City.Properties.Single(b=>b.Id==id);var r=empire.ForecastCity().Businesses.FirstOrDefault(b=>b.PropertyId==id);var m=Studio.Modal(root,PropertyLabel(p)+" · COSTS",()=>WorldPropertyDetails(id));
   Studio.Label(m,"NEXT DAY · PER OPERATING DAY",44,235,632,65,25,Studio.Muted,TextAlignmentOptions.Center,true);
   string[] labels={"Sales or rent","Wages, power & upkeep","Imported supplies","Sales tax","Net after costs"};long[] values={r?.Revenue??0,r?.Bills??0,r?.Imports??0,r?.Tax??0,r?.Net??0};
   for(int i=0;i<labels.Length;i++){float y=370+i*115;Studio.Label(m,labels[i],44,y,410,90,27,Studio.Text);Studio.Label(m,CityMoney(values[i]),458,y,218,90,32,i==4?Studio.Accent:Studio.Text,TextAlignmentOptions.Right,true);}
   Studio.Label(m,"Loan repayments and old bills belong to the city ledger.",44,980,632,85,25,Studio.Muted,TextAlignmentOptions.Center);
   Studio.Button(m,"BACK TO BUSINESS",44,1100,632,125,()=>WorldPropertyDetails(id),true);
  }
 }
 public sealed class CityTimeProgress:MonoBehaviour {
  public RectTransform Fill;public TextMeshProUGUI Label;public long StartAt,FinishAt;public Func<long> ReadNow;
  void Update(){long now=ReadNow?.Invoke()??StartAt;float value=FinishAt<=StartAt?1:Mathf.Clamp01((float)(now-StartAt)/(FinishAt-StartAt));if(Fill){Fill.anchorMin=Vector2.zero;Fill.anchorMax=new Vector2(value,1);Fill.offsetMin=Fill.offsetMax=Vector2.zero;}if(Label)Label.text=Math.Max(0,FinishAt-now)+"s TO COMPLETE";}
 }
 public sealed class CityPeopleGlyph:MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;float unit=Mathf.Min(r.width,r.height)/10;var p=r.center;CityGameSkin.Round(vh,new Rect(p.x-unit*1.4f,p.y+unit*.4f,unit*2.8f,unit*2.8f),unit*1.4f,FlatUI.Hex("#FFD4A2"),FlatUI.Hex("#FFE7C8"));CityGameSkin.Round(vh,new Rect(p.x-unit*2.5f,p.y-unit*3.4f,unit*5,unit*3.4f),unit,FlatUI.Hex("#3A8AC6"),FlatUI.Hex("#90DDFA"));}
 }
 public sealed class CityExchangeGlyph:MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;float middle=r.center.y;Arrow(vh,new Vector2(r.xMin,middle+8),new Vector2(r.xMax,middle+8),FlatUI.Hex("#9AF0B3"));Arrow(vh,new Vector2(r.xMax,middle-13),new Vector2(r.xMin,middle-13),FlatUI.Hex("#FFE0AE"));}
  static void Arrow(VertexHelper vh,Vector2 a,Vector2 b,Color c){var d=(b-a).normalized;var side=new Vector2(-d.y,d.x);CraftedPanel.Line(vh,a,b-d*7,2,c);int i=vh.currentVertCount;vh.AddVert(b,c,Vector2.zero);vh.AddVert(b-d*10+side*5,c,Vector2.zero);vh.AddVert(b-d*10-side*5,c,Vector2.zero);vh.AddTriangle(i,i+1,i+2);}
 }
}
