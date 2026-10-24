using System;
using System.Collections.Generic;
using System.Linq;

namespace CashflowChains.City {
 // Derived public access, not purchased land or a new economic resource.
 // Every displayed local edge serves construction or a real property route.
 [Serializable] public struct CityStreetEdge : IEquatable<CityStreetEdge> {
  public int X,Y,DX,DY;
  public CityStreetEdge(int x,int y,int dx,int dy){X=x;Y=y;DX=dx;DY=dy;}
  public bool Equals(CityStreetEdge e)=>X==e.X&&Y==e.Y&&DX==e.DX&&DY==e.DY;
  public override bool Equals(object o)=>o is CityStreetEdge e&&Equals(e);
  public override int GetHashCode()=>((X*397+Y)*397+DX)*397+DY;
 }
 public sealed class CityStreets {
  readonly HashSet<CityStreetEdge> edges=new HashSet<CityStreetEdge>();
  public CityStreetEdge[] LocalEdges=>edges.OrderBy(e=>e.Y).ThenBy(e=>e.X).ThenBy(e=>e.DX).ToArray();
  public CityStreets(IEnumerable<CityProperty> properties){
   var plots=properties.OrderBy(p=>p.Id).SelectMany(p=>p.PendingX<0?new[]{p}:new[]{p,new CityProperty{Id=p.Id,X=p.PendingX,Y=p.PendingY}}).ToArray();
   if(plots.Length==0)return;
   // A single access stem joins the first property to the regional highway.
   Connect(plots[0].X+1,0,plots[0].X+1,plots[0].Y+1);
   foreach(var p in plots){Add(p.X,p.Y+1,p.X+1,p.Y+1);foreach(var other in plots)Connect(p.X+1,p.Y+1,other.X+1,other.Y+1);}
  }
  void Add(int x,int y,int bx,int by){if(y==0&&by==0)return;if(bx<x||by<y){int t=x;x=bx;bx=t;t=y;y=by;by=t;}edges.Add(new CityStreetEdge(x,y,bx-x,by-y));}
  void Connect(int x,int y,int bx,int by){while(x!=bx){int next=x+Math.Sign(bx-x);Add(x,y,next,y);x=next;}while(y!=by){int next=y+Math.Sign(by-y);Add(x,y,x,next);y=next;}}
  public bool HasRoad(int x,int y,int bx,int by){if(Math.Abs(bx-x)+Math.Abs(by-y)!=1)return false;if(y==0&&by==0)return true;if(bx<x||by<y){int t=x;x=bx;bx=t;t=y;y=by;by=t;}return edges.Contains(new CityStreetEdge(x,y,bx-x,by-y));}
  public int Degree(int x,int y)=>(HasRoad(x,y,x-1,y)?1:0)+(HasRoad(x,y,x+1,y)?1:0)+(HasRoad(x,y,x,y-1)?1:0)+(HasRoad(x,y,x,y+1)?1:0);
 }
}
