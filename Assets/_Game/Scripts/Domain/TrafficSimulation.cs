using System;
using System.Collections.Generic;

namespace CashflowChains.Domain {
 // Decorative traffic only. It never creates economic events or rewards.
 [Serializable] public sealed class StreetCar {
  public int Id,Axis,Lane,Direction;
  public double X,Y,Speed,Distance;
  public double HalfX=>Axis==0?TrafficSimulation.HalfWidth:TrafficSimulation.HalfLength;
  public double HalfY=>Axis==0?TrafficSimulation.HalfLength:TrafficSimulation.HalfWidth;
 }
 public sealed class TrafficSimulation {
  public const double HalfLength=9.5,HalfWidth=4.4,LaneOffset=5.5;
  public readonly double Width,Height;
  public readonly int Columns,Rows;
  readonly bool highwayOnly;
  public readonly List<StreetCar> Cars=new List<StreetCar>();
  public double Time{get;private set;}
  public TrafficSimulation(double width,double height,int columns,int rows,bool highwayOnly=false){
   if(width<columns*40||height<rows*40||columns<1||rows<1)throw new ArgumentException("Invalid road dimensions");Width=width;Height=height;Columns=columns;Rows=rows;
   this.highwayOnly=highwayOnly;
   if(highwayOnly){for(int direction=-1;direction<=1;direction+=2)for(int i=0;i<3;i++){var car=new StreetCar{Id=Cars.Count,Axis=1,Lane=rows,Direction=direction,Distance=(i+.25)*width/3,Speed=32};Position(car,car.Distance,out car.X,out car.Y);Cars.Add(car);}if(HasOverlap())throw new ArgumentException("Highway spawn overlaps");return;}
   for(int axis=0;axis<2;axis++)for(int lane=1;lane<(axis==0?columns:rows);lane++)for(int direction=-1;direction<=1;direction+=2){double length=axis==0?height:width;int segments=axis==0?rows:columns;var car=new StreetCar{Id=Cars.Count,Axis=axis,Lane=lane,Direction=direction,Distance=(.5+(lane+axis)%segments)*length/segments,Speed=20+(lane*7+axis*11)%16};Position(car,car.Distance,out car.X,out car.Y);Cars.Add(car);}
   if(HasOverlap())throw new ArgumentException("Traffic spawn overlaps");
  }
  public static bool Overlaps(StreetCar a,StreetCar b)=>Math.Abs(a.X-b.X)<a.HalfX+b.HalfX+.5&&Math.Abs(a.Y-b.Y)<a.HalfY+b.HalfY+.5;
  public bool HasOverlap(){for(int i=0;i<Cars.Count;i++)for(int j=i+1;j<Cars.Count;j++)if(Overlaps(Cars[i],Cars[j]))return true;return false;}
  void Position(StreetCar c,double distance,out double x,out double y){if(c.Axis==0){x=c.Lane*Width/Columns+c.Direction*LaneOffset;y=distance;}else{x=distance;y=c.Lane*Height/Rows-c.Direction*LaneOffset;}}
  bool Red(int axis){if(highwayOnly)return false;double phase=Time%9;return axis==0?phase>=3.5:phase<4.5||phase>=8;}
  public void Advance(double seconds){if(seconds<0||seconds>60)throw new ArgumentException("Invalid traffic step");while(seconds>0){double dt=Math.Min(seconds,.02);seconds-=dt;Time+=dt;foreach(var car in Cars){double length=car.Axis==0?Height:Width,step=length/(car.Axis==0?Rows:Columns);double next=car.Distance+car.Direction*car.Speed*dt;bool wrapped=next<0||next>=length;if(wrapped)next=(next%length+length)%length;
     bool stop=false;if(Red(car.Axis)&&!wrapped){for(int intersection=1;intersection<(car.Axis==0?Rows:Columns);intersection++){double line=intersection*step-car.Direction*22;double before=(line-car.Distance)*car.Direction,after=(line-next)*car.Direction;if(before>=0&&after<=0){stop=true;break;}}}if(stop)continue;
     Position(car,next,out double x,out double y);bool free=true;foreach(var other in Cars){if(other.Id==car.Id)continue;double minX=wrapped?x:Math.Min(x,car.X),maxX=wrapped?x:Math.Max(x,car.X),minY=wrapped?y:Math.Min(y,car.Y),maxY=wrapped?y:Math.Max(y,car.Y);if(maxX+car.HalfX+.5>other.X-other.HalfX&&minX-car.HalfX-.5<other.X+other.HalfX&&maxY+car.HalfY+.5>other.Y-other.HalfY&&minY-car.HalfY-.5<other.Y+other.HalfY){free=false;break;}}
     if(free){car.Distance=next;car.X=x;car.Y=y;}
    }}
  }
 }
}
