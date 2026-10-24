using System;
using System.Collections.Generic;
using System.Linq;
namespace CashflowChains.City {
 public static class CityMetropolis {
  public const int Size=8;
  static readonly string[][] blocks={
   new[]{"port","warehouse","factory","factory","office","hotel","warehouse","warehouse"},
   new[]{"grocer","apartment","office","restaurant","cafe","bar","hotel","apartment"},
   new[]{"restaurant","home","apartment","cafe","restaurant","office","hotel","apartment"},
   new[]{"grocer","apartment","park","home","cafe","restaurant","bar","office"},
   new[]{"club","home","restaurant","cafe","office","hotel","apartment","park"},
   new[]{"grocer","office","bar","grocer","club","home","restaurant","apartment"},
   new[]{"apartment","home","hotel","park","cafe","grocer","office","vacant"},
   new[]{"home","apartment","office","restaurant","bar","hotel","warehouse","vacant"}
  };
  public static readonly CityPublicBuilding[] Buildings=Create();
  static CityPublicBuilding[] Create(){var result=new List<CityPublicBuilding>();
   foreach(var old in CityRegion.Buildings.Where(b=>b.Id>=1100&&b.Id<=1112)){int n=old.Id-1100;result.Add(new CityPublicBuilding{Id=old.Id,X=n<8?n:-1,Y=n<8?-1:n-7,Name=old.Name,Art=old.Art,Role=old.Role,Kind="office",Level=1});}
   for(int y=0;y<Size;y++)for(int x=0;x<Size;x++){string type=blocks[y][x];if(type=="vacant")continue;string kind=type=="apartment"?"home":type=="restaurant"||type=="bar"?"cafe":type=="club"?"cinema":type;string art=type=="restaurant"||type=="bar"||type=="club"||type=="hotel"||type=="home"||type=="apartment"||type=="office"||type=="grocer"?"urban-"+type:type;
    result.Add(new CityPublicBuilding{Id=12000+y*Size+x,X=x,Y=y,Kind=kind,Level=type=="apartment"?5:1,Housing=kind=="home",Art=art,Name=type=="home"?"Townhouse":type=="apartment"?"Apartments":type=="grocer"?"Groceries":type=="club"?"Nightclub":char.ToUpperInvariant(type[0])+type.Substring(1),Role="Established business · another owner"});}
   void Offer(int x,int y,string name,int price,int day){var b=result.Single(p=>p.X==x&&p.Y==y);b.Name=name;b.OfferPrice=price;b.AvailableDay=day;}
   Offer(3,3,"Willow Rental",80,1);Offer(4,3,"Corner Cafe",100,1);Offer(3,5,"Market Street Grocer",180,1);Offer(1,4,"Garden Rental",120,3);Offer(7,0,"Eastside Storage",160,4);Offer(5,4,"Grand Avenue Hotel",480,6);Offer(4,4,"Exchange Offices",600,8);
   return result.ToArray();
  }
  public static CityPublicBuilding Building(int id)=>Buildings.FirstOrDefault(b=>b.Id==id);
  public static CityPublicBuilding At(int x,int y)=>Buildings.FirstOrDefault(b=>b.X==x&&b.Y==y);
  public static bool Vacant(int x,int y)=>(x>=Size||y>=Size)||x==7&&(y==6||y==7);
  public static IEnumerable<(int X,int Y)> Vacancies(CityState city){for(int y=0;y<city.Height;y++)for(int x=0;x<city.Width;x++)if(Vacant(x,y)&&!city.Properties.Any(p=>p.X==x&&p.Y==y||p.PendingX==x&&p.PendingY==y))yield return (x,y);}
  public static IEnumerable<CityPublicBuilding> Available(CityState city)=>Buildings.Where(b=>b.OfferPrice>0&&city.Day>=b.AvailableDay&&!city.Properties.Any(p=>p.X==b.X&&p.Y==b.Y));
  public static CityHouseholdOccupancy[] Households(int day,IEnumerable<CityProperty> owned){var p=(owned??Enumerable.Empty<CityProperty>()).Where(b=>b.Level>0).ToArray();return Buildings.Where(b=>b.Housing&&!p.Any(o=>o.X==b.X&&o.Y==b.Y)).Select(b=>new CityHouseholdOccupancy{PropertyId=b.Id,Units=CityHousing.Units(b.Level),Occupied=CityHousing.Units(b.Level),Families=Enumerable.Range(0,CityHousing.Units(b.Level)).Select(unit=>CityResidents.Household(b.Id,unit,day)).ToArray()}).ToArray();}
 }
}
