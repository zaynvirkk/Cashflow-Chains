using System;
using System.Linq;
using CashflowChains.Domain;
using NUnit.Framework;
public sealed class TrafficSimulationTests {
 [Test] public void CarsKeepTheirFullSilhouettesApartAcrossAllBoardSizes(){for(int size=4;size<=7;size++){var traffic=new TrafficSimulation(644,644,size,size);for(int tick=0;tick<6000;tick++){traffic.Advance(1d/30);Assert.False(traffic.HasOverlap(),"size "+size+" tick "+tick);}Assert.Greater(traffic.Cars.Count,0);}}
 [Test] public void RedLightsAndIntersectionsDoNotFreezeTheWholeCity(){var t=new TrafficSimulation(644,644,6,6);var start=t.Cars.Select(c=>(c.X,c.Y)).ToArray();t.Advance(60);Assert.Greater(t.Cars.Where((c,i)=>Math.Abs(c.X-start[i].X)+Math.Abs(c.Y-start[i].Y)>20).Count(),t.Cars.Count/2);Assert.False(t.HasOverlap());}
 [Test] public void ResizeCreatesASafeLayoutAndRejectsInvalidDimensions(){Assert.Throws<ArgumentException>(()=>new TrafficSimulation(20,20,7,7));var t=new TrafficSimulation(400,400,7,7);t.Advance(10);Assert.False(t.HasOverlap());Assert.True(t.Cars.All(c=>c.X>=0&&c.Y>=0&&c.X<=400&&c.Y<=400));}
}
