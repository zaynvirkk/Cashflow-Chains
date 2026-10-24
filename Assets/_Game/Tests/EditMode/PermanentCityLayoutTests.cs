using LocalGame;
using NUnit.Framework;
using UnityEngine;
public sealed class PermanentCityLayoutTests {
 [TestCase(992)] [TestCase(1280)] [TestCase(1600)] [TestCase(1800)]
 public void PortraitCityClockMapForecastAndControlsNeverCoverEachOther(int height){
  var go=new GameObject("City layout fixture",typeof(RectTransform),typeof(ResponsiveStage));
  try{var rect=go.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(720,height);var stage=go.GetComponent<ResponsiveStage>();stage.View="city-world";
   var header=stage.Layout("City identity backing",new Rect(0,0,720,184));var clock=stage.Layout("City day strip",new Rect(20,152,680,34));var map=stage.Layout("Permanent city map",new Rect(18,209,684,805));var forecast=stage.Layout("City day forecast",new Rect(18,1026,684,158));var camera=stage.Layout("FIT CITY",new Rect(119,934,165,64));var action=stage.Layout("BUILD & DEVELOP",new Rect(366,1202,330,78));
   Assert.LessOrEqual(clock.yMax,header.yMax);Assert.Less(header.yMax,map.yMin);Assert.Less(map.yMax,forecast.yMin);Assert.Less(forecast.yMax,action.yMin);Assert.Less(map.yMax,camera.yMin);Assert.Less(camera.yMax,forecast.yMin);Assert.LessOrEqual(action.yMax,height);Assert.Greater(map.height,0);
  }finally{Object.DestroyImmediate(go);}
 }
 [Test] public void BareCivicRectCanReceiveAnInvisibleHitTargetWithoutLosingItsChildren(){
  var go=new GameObject("New civic plot",typeof(RectTransform));int taps=0;
  try{var target=FlatUI.Hit(go.GetComponent<RectTransform>(),()=>taps++,()=>{},_=>{});var image=go.GetComponent<UnityEngine.UI.Image>();Assert.NotNull(image);Assert.AreEqual(0,image.color.a);Assert.IsTrue(image.raycastTarget);target.Click();Assert.AreEqual(1,taps);}
  finally{Object.DestroyImmediate(go);}
 }
 [Test] public void ExistingParcelGraphicRemainsTheOnlyGraphicAndReceivesTaps(){
  var go=new GameObject("Drawn city parcel",typeof(RectTransform),typeof(CashflowChains.Presentation.MetroLot));int taps=0;
  try{var original=go.GetComponent<CashflowChains.Presentation.MetroLot>();var target=FlatUI.Hit(go.GetComponent<RectTransform>(),()=>taps++,()=>{},_=>{});Assert.IsTrue(original.raycastTarget);Assert.IsNull(go.GetComponent<UnityEngine.UI.Image>());Assert.AreEqual(1,go.GetComponents<UnityEngine.UI.Graphic>().Length);target.Click();Assert.AreEqual(1,taps);}
  finally{Object.DestroyImmediate(go);}
 }
 [Test] public void BodyFontWeightsKeepTheirOwnAtlasMaterials(){
  var go=new GameObject("Real UI typography fixture",typeof(RectTransform));bool previous=Studio.Velvet;Studio.Velvet=false;
  try{var regular=Studio.Label(go.transform,"Household",0,0,600,100,24);var bold=Studio.Label(go.transform,"Household",0,110,600,100,24,heavy:true);regular.font.TryAddCharacters("Household");bold.font.TryAddCharacters("Household");Assert.AreNotSame(regular.font,bold.font);Assert.AreNotSame(regular.fontSharedMaterial,bold.fontSharedMaterial);Assert.AreSame(regular.font.atlasTexture,regular.fontSharedMaterial.GetTexture("_MainTex"));Assert.AreSame(bold.font.atlasTexture,bold.fontSharedMaterial.GetTexture("_MainTex"));}
  finally{Studio.Velvet=previous;Object.DestroyImmediate(go);}
 }

 [TestCase(4)] [TestCase(12)] public void CityZoomIsBoundedAndFitStillShowsAllOwnedLand(int side){
  var view=new GameObject("Phone viewport",typeof(RectTransform));var content=new GameObject("Owned land",typeof(RectTransform));
  try{var v=view.GetComponent<RectTransform>();v.sizeDelta=new Vector2(720,1104);var c=content.GetComponent<RectTransform>();c.sizeDelta=new Vector2((side+2)*132,(side+2)*132);var camera=view.AddComponent<CashflowChains.Presentation.CityMapCamera>();camera.Viewport=v;camera.Content=c;camera.FocusSize=new Vector2(side*132,side*132);camera.Scale(10000);Assert.AreEqual(CashflowChains.Presentation.CityMapCamera.MaximumZoom,camera.Zoom);camera.Scale(.000001f);Assert.AreEqual(camera.MinimumZoom,camera.Zoom);camera.Fit();Assert.LessOrEqual(camera.FocusSize.x*camera.Zoom,v.rect.width);Assert.LessOrEqual(camera.FocusSize.y*camera.Zoom,v.rect.height);Assert.LessOrEqual(camera.Zoom,CashflowChains.Presentation.CityMapCamera.MaximumZoom);Assert.GreaterOrEqual(camera.Zoom,camera.MinimumZoom);}
  finally{Object.DestroyImmediate(content);Object.DestroyImmediate(view);}
 }

}
