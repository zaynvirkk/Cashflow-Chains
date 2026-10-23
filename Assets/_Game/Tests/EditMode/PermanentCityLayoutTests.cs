using LocalGame;
using NUnit.Framework;
using UnityEngine;
public sealed class PermanentCityLayoutTests {
 [TestCase(992)] [TestCase(1280)] [TestCase(1600)] [TestCase(1800)]
 public void PortraitCityClockMapForecastAndControlsNeverCoverEachOther(int height){
  var go=new GameObject("City layout fixture",typeof(RectTransform),typeof(ResponsiveStage));
  try{var rect=go.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(720,height);var stage=go.GetComponent<ResponsiveStage>();stage.View="city-world";
   var header=stage.Layout("City identity backing",new Rect(18,12,684,176));var clock=stage.Layout("Text",new Rect(34,146,638,34));var map=stage.Layout("Permanent city map",new Rect(18,209,684,805));var forecast=stage.Layout("City day forecast",new Rect(18,1026,684,158));var camera=stage.Layout("FIT CITY",new Rect(119,934,165,64));var action=stage.Layout("BUILD & DEVELOP",new Rect(366,1202,330,78));
   Assert.LessOrEqual(clock.yMax,header.yMax);Assert.Less(header.yMax,map.yMin);Assert.Less(map.yMax,forecast.yMin);Assert.Less(forecast.yMax,action.yMin);Assert.Less(camera.yMax,map.yMax);Assert.LessOrEqual(action.yMax,height);Assert.Greater(map.height,0);
  }finally{Object.DestroyImmediate(go);}
 }
}
