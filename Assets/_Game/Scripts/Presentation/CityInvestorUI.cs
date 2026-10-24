using System;
using System.Linq;
using CashflowChains.City;
using LocalGame;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CashflowChains.Presentation {
 public sealed partial class MobileGameApp {
  void DrawPublicDistrict(RectTransform parent,CityState city,float cell){
   foreach(var building in CityRegion.Buildings){var b=building;var tile=new GameObject("Public building "+b.Id,typeof(RectTransform)).GetComponent<RectTransform>();tile.SetParent(parent,false);tile.anchorMin=tile.anchorMax=tile.pivot=new Vector2(0,1);tile.sizeDelta=new Vector2(cell-24,cell-24);tile.anchoredPosition=new Vector2((b.X+.5f)*cell+12,-(b.Y+.5f)*cell-12);
    Studio.Image(tile,"V3/plot",0,0,720,1280,new Color(.73f,.88f,1,.88f));var art=Studio.Image(tile,b.Art,15,0,690,1000);art.GetComponent<Image>().preserveAspect=true;
    if(b.Art=="factory"||b.Art=="power"){var life=FlatUI.Rect(art,"Public industry activity",0,0,720,1280).gameObject.AddComponent<BuildingLife>();life.Kind=b.Art;life.Active=true;life.raycastTarget=false;}
    string shortName=b.Housing?"HOMES":b.Id==1101?"FIRE":b.Id==1104?"AUCTION":b.Id==1105?"WATER":b.Id==1106?"TRANSIT":b.Id==1108?"POWER":b.Id==1109?"INDUSTRY":b.Id==1114?"MARKET":b.Name;
    Studio.Box(tile,"Other ownership plate",0,860,720,420,FlatUI.Hex("#102B48"));Studio.Label(tile,shortName,10,860,700,240,13,Studio.Text,TextAlignmentOptions.Center,true);Studio.Label(tile,b.Housing||b.Id==1104||b.Id==1107||b.Id==1109||b.Id==1113||b.Id==1114?"OTHER OWNER":"CITY",10,1100,700,180,9,FlatUI.Hex("#A7D5EC"),TextAlignmentOptions.Center,true);
    var hit=FlatUI.Hit(tile,()=>WorldPublicBuilding(b.Id),()=>worldCamera.Begin(Pointer.current?.position.ReadValue()??Vector2.zero),p=>worldCamera.End());hit.Drag=p=>worldCamera.Drag(p);
   }
  }
  void DrawInvestorGuide(RectTransform parent,CityState city){
   var guide=Studio.Button(parent,"",18,1120,684,110,WorldCareer,false,FlatUI.Hex("#0A285A"));Studio.Role(guide,"MY PROGRESSION");Studio.Label(guide.transform,(city.CareerStage>=6?"CAREER: ":"NEXT: ")+CityCareer.Names[city.CareerStage].ToUpperInvariant(),15,80,690,700,23,Studio.Text,TextAlignmentOptions.Center,true);Studio.Label(guide.transform,city.CareerStage+" / 6 MILESTONES  ›",15,840,690,330,16,FlatUI.Hex("#A4F6BA"),TextAlignmentOptions.Center,true);
  }
  void WorldCareer(){
   if(empire.Snapshot.City.Region>=2){MetropolisCareer();return;}var city=empire.Snapshot.City;var m=Studio.Modal(root,"YOUR INVESTOR JOURNEY",PermanentCityScreen);
   for(int i=0;i<6;i++){int step=i;float y=225+i*115;var box=Studio.Box(m,"Career step "+i,44,y,632,100,FlatUI.Hex(i<city.CareerStage?"#154F48":i==city.CareerStage?"#235F91":"#102B50"));Studio.Label(box,(i+1).ToString(),15,110,75,950,32,i<city.CareerStage?Studio.Accent:Studio.Text,TextAlignmentOptions.Center,true);Studio.Label(box,CityCareer.Names[i],112,110,590,950,26,i<=city.CareerStage?Studio.Text:Studio.Muted,heavy:i==city.CareerStage);}
   Studio.Label(m,CityCareer.Directions[city.CareerStage],44,950,632,110,26,Studio.Text,TextAlignmentOptions.Center);
   Studio.Button(m,city.CareerStage==0?"COMPARE MY FIRST INVESTMENT":"BACK TO MY PORTFOLIO",44,1100,632,125,()=>{if(city.CareerStage==0)WorldBuildPicker(1,1,0);else PermanentCityScreen();},true);
  }
  void WorldPublicBuilding(int id){
   var b=CityRegion.Building(id,empire.Snapshot.City.Region);if(b==null)return;var m=Studio.Modal(root,b.Name,PermanentCityScreen);Studio.Image(m,b.Art,220,200,280,280);Studio.Label(m,b.Housing?"OTHER OWNER · NO RENT FOR YOU":"PUBLIC / OTHER OWNER · NOT YOUR INCOME",44,505,632,70,25,FlatUI.Hex("#A7D5EC"),TextAlignmentOptions.Center,true);
   string detail=b.Housing?CityRegion.Households(empire.Snapshot.City.Day,empire.Snapshot.City.Region,empire.Snapshot.City.Properties).Single(h=>h.PropertyId==id).Residents+" residents in existing homes.\nThese households shop in the district and can use your businesses.":b.Role+".\nOnly businesses you own contribute to your private cashflow.";
   if(id==1106)detail="Regional buses and the highway already connect this district.\nEight daily regional visitor opportunities can serve your future hotels;you do not need to own a port first.";
   if(id==1107)detail="A working supply market is already here.\nYour businesses can import needed goods at published prices. You pay for actual stock and deliveries.";
   Studio.Label(m,detail,44,640,632,230,30,Studio.Text,TextAlignmentOptions.Center);
   if(b.Housing)Studio.Button(m,"MEET THESE HOUSEHOLDS",44,955,632,100,()=>WorldHouseholds(id,0));
   if(id==1107)Studio.Button(m,"BUY MATERIALS",44,955,632,100,WorldWholesale);
   if(id==1108)Studio.Button(m,"MY METERED CONNECTION",44,955,632,100,WorldUtilities);
   Studio.Button(m,"BACK TO MY PORTFOLIO",44,1100,632,125,PermanentCityScreen,true);
  }
  void WorldWholesale(){
   var m=Studio.Modal(root,"REGIONAL WHOLESALE",()=>WorldPublicBuilding(1107));var market=CityRegion.Market(empire.Snapshot.City.Day);Studio.Label(m,market.Name,44,220,632,80,30,Studio.Text,TextAlignmentOptions.Center,true);Studio.Label(m,market.Description,44,315,632,130,27,Studio.Muted,TextAlignmentOptions.Center);
   string[] goods={"WOOD","STEEL","BEANS","CUPS","PRODUCE"};for(int i=0;i<goods.Length;i++){string item=goods[i];var d=empireConfig.City.Good(item);Studio.Button(m,ItemName(item)+" · "+CityMoney(d.ImportPrice)+" / UNIT",44,485+i*115,632,90,()=>WorldMaterial(item,1,WorldWholesale));}
   Studio.Label(m,"Your cash buys actual inventory.\nImports used by operating businesses are billed automatically.",44,1080,632,130,25,Studio.Muted,TextAlignmentOptions.Center);
  }
  void WorldMarketCalendar(){
   var city=empire.Snapshot.City;var m=Studio.Modal(root,"DISTRICT MARKET CALENDAR",PermanentCityScreen);
   for(int i=0;i<3;i++){int day=city.Day+i;var market=CityRegion.Market(day);float y=245+i*255;var box=Studio.Box(m,"Real market day "+day,44,y,632,230,FlatUI.Hex(i==0?"#185877":"#102B50"));Studio.Label(box,(i==0?"TODAY":"DAY "+day)+" · "+market.Name,20,50,680,350,25,Studio.Text,TextAlignmentOptions.Center,true);Studio.Label(box,market.Description,24,490,672,690,25,Studio.Muted,TextAlignmentOptions.Center);}
   Studio.Button(m,"BACK TO MY PORTFOLIO",44,1100,632,125,PermanentCityScreen,true);
  }
  void WorldInvestorHelp(){
   var m=Studio.Modal(root,"YOUR PLACE IN THE CITY",WorldDesk);string[] art={"apartment","home","civic-wholesale"};string[] text={"The city already exists. Other owners’ homes,public services and industry support daily life.","You own a private portfolio. Only your rent and business sales earn you cash;your bills reduce it.","Compare investments. Supplies,workers,distance,prices and demand change their margins."};
   for(int i=0;i<3;i++){float y=245+i*260;Studio.Image(m,art[i],44,y,180,200);Studio.Label(m,text[i],250,y,426,215,28,Studio.Text);}
   Studio.Button(m,"MY INVESTOR JOURNEY",44,1100,632,125,WorldCareer,true);
  }
 }
}
