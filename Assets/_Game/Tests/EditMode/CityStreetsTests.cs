using System;
using System.Collections.Generic;
using System.Linq;
using CashflowChains.City;
using CashflowChains.Domain;
using NUnit.Framework;

public sealed class CityStreetsTests {
 [Test] public void EmptyCityHasOnlyTheRegionalHighway(){var streets=new CityStreets(new CityProperty[0]);Assert.IsEmpty(streets.LocalEdges);Assert.IsTrue(streets.HasRoad(-8,0,-7,0));Assert.IsFalse(streets.HasRoad(1,1,2,1));}
 [Test] public void FirstConstructionHasAccessWithoutServicingTheWholeGrid(){var s=new CityStreets(new[]{new CityProperty{Id=1,X=1,Y=1,Level=0}});Assert.AreEqual(3,s.LocalEdges.Length);Assert.IsTrue(s.HasRoad(2,0,2,1));Assert.IsTrue(s.HasRoad(2,1,2,2));Assert.IsTrue(s.HasRoad(1,2,2,2));Assert.IsFalse(s.HasRoad(4,0,4,1));}
 [Test] public void EveryShownEconomicRouteHasRoadAndRetainsModeledWalkingDistance(){
  var random=new Random(20261024);
  for(int sample=0;sample<80;sample++){
   var locations=Enumerable.Range(0,144).OrderBy(_=>random.Next()).Take(1+sample%30).ToArray();var properties=locations.Select((n,i)=>new CityProperty{Id=i+1,X=n%12,Y=n/12}).ToArray();var streets=new CityStreets(properties);
   foreach(var a in properties)foreach(var b in properties){int x=a.X+1,y=a.Y+1,distance=0;while(x!=b.X+1){int next=x+Math.Sign(b.X+1-x);Assert.IsTrue(streets.HasRoad(x,y,next,y));x=next;distance++;}while(y!=b.Y+1){int next=y+Math.Sign(b.Y+1-y);Assert.IsTrue(streets.HasRoad(x,y,x,next));y=next;distance++;}Assert.AreEqual(Math.Abs(a.X-b.X)+Math.Abs(a.Y-b.Y),distance);}
   var reached=new HashSet<string>{(properties[0].X+1)+":0"};bool changed=true;while(changed){changed=false;foreach(var e in streets.LocalEdges){string a=e.X+":"+e.Y,b=(e.X+e.DX)+":"+(e.Y+e.DY);if(reached.Contains(a))changed|=reached.Add(b);if(reached.Contains(b))changed|=reached.Add(a);}}
   foreach(var p in properties)Assert.IsTrue(reached.Contains((p.X+1)+":"+(p.Y+1)));Assert.AreEqual(streets.LocalEdges.Length,streets.LocalEdges.Distinct().Count());
  }
 }
 [Test] public void PendingMovesKeepBothSitesAccessible(){var s=new CityStreets(new[]{new CityProperty{Id=1,X=1,Y=1,PendingX=3,PendingY=3}});Assert.IsTrue(s.HasRoad(1,2,2,2));Assert.IsTrue(s.HasRoad(3,4,4,4));Assert.IsTrue(s.HasRoad(4,3,4,4));}
 [Test] public void HighwayTrafficNeverUsesUnbuiltLocalRoadsAndKeepsCarBodiesApart(){var traffic=new TrafficSimulation(1584,528,12,4,true);double before=traffic.Cars[0].X;for(int i=0;i<6000;i++){traffic.Advance(1d/30);Assert.IsFalse(traffic.HasOverlap());Assert.IsTrue(traffic.Cars.All(c=>c.Axis==1&&Math.Abs(c.Y-528)<6));}Assert.AreNotEqual(before,traffic.Cars[0].X);}
}
