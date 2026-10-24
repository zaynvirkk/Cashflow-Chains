using System;
using CashflowChains.City;
using LocalGame;
using UnityEngine;
using UnityEngine.UI;
namespace CashflowChains.Presentation {
 public sealed partial class MobileGameApp {
  string PropertyLabel(CityProperty p)=>empireConfig.City.Business(p.Kind).Population>0?CityHousing.Label(p.Level):WorldLabel(p.Kind);
  RectTransform PropertyArt(Transform parent,CityProperty p,float x,float y,float width,float height){
   var d=empireConfig.City.Business(p.Kind);var group=FlatUI.Rect(parent,"Owned property art "+p.Id,x,y,width,height);bool housing=d.Population>0;int level=Math.Max(1,p.Level),copies=housing&&level<=3?CityHousing.Units(level):1;string art=p.AcquiredFrom>0&&level==1?CityMetropolis.Building(p.AcquiredFrom).Art:housing?CityHousing.Art(level):d.Art??p.Kind;float span=720f/copies;
   for(int i=0;i<copies;i++){var image=Studio.Image(group,art,i*span,0,span,1280);image.GetComponent<Image>().preserveAspect=true;}return group;
  }
 }
}
