using System;
using System.Collections.Generic;
using System.Linq;

namespace CashflowChains.City {
 public sealed class CityPublicBuilding {
  public int Id,X,Y;public string Name,Art,Role;public bool Housing;
  public CityProperty Footprint()=>new CityProperty{Id=Id,X=X,Y=Y,Kind=Housing?"home":"office",Level=Housing?5:1,Staff=0};
 }
 public sealed class CityMarketDay {
  public string Name,Description;public int ExtraImportCapacity,ExtraShoppingNeed;
 }
 public static class CityRegion {
  public const int Version=1,PublicEmployment=24,RegionalHousingDemand=6;
  public static readonly CityPublicBuilding[] Buildings={
   new CityPublicBuilding{Id=1000,X=-2,Y=1,Name="WESTSIDE HOMES",Art="apartment",Housing=true,Role="Other owners · six occupied homes"},
   new CityPublicBuilding{Id=1001,X=-2,Y=3,Name="RIVERSIDE HOMES",Art="apartment",Housing=true,Role="Other owners · six occupied homes"},
   new CityPublicBuilding{Id=1002,X=-1,Y=-2,Name="CENTRAL HOMES",Art="apartment",Housing=true,Role="Other owners · six occupied homes"},
   new CityPublicBuilding{Id=1100,X=0,Y=-2,Name="POLICE",Art="civic-police",Role="Municipal public safety · outside your portfolio"},
   new CityPublicBuilding{Id=1101,X=1,Y=-2,Name="FIRE STATION",Art="civic-fire",Role="Municipal fire service · outside your portfolio"},
   new CityPublicBuilding{Id=1102,X=2,Y=-2,Name="HOSPITAL",Art="civic-hospital",Role="Regional healthcare · outside your portfolio"},
   new CityPublicBuilding{Id=1103,X=3,Y=-2,Name="COURTHOUSE",Art="civic-court",Role="Civic legal administration · outside your portfolio"},
   new CityPublicBuilding{Id=1104,X=-2,Y=-1,Name="AUCTION HOUSE",Art="civic-auction",Role="Independent auction venue · outside your portfolio"},
   new CityPublicBuilding{Id=1105,X=-3,Y=2,Name="WATER UTILITY",Art="civic-water",Role="Municipal water infrastructure · outside your portfolio"},
   new CityPublicBuilding{Id=1106,X=-3,Y=0,Name="TRANSIT DEPOT",Art="civic-transit",Role="Regional transport · outside your portfolio"},
   new CityPublicBuilding{Id=1107,X=-2,Y=0,Name="WHOLESALE",Art="civic-wholesale",Role="Published material market · paid supplies"},
   new CityPublicBuilding{Id=1108,X=-3,Y=3,Name="POWER UTILITY",Art="power",Role="Metered electricity · your own usage is billed"},
   new CityPublicBuilding{Id=1109,X=-3,Y=1,Name="INDUSTRIAL YARD",Art="factory",Role="Regional industry · outside your portfolio"},
   new CityPublicBuilding{Id=1110,X=-1,Y=-1,Name="SCHOOL",Art="office",Role="Public education · outside your portfolio"},
   new CityPublicBuilding{Id=1111,X=-3,Y=-1,Name="CITY HALL",Art="bank",Role="Municipal administration · outside your portfolio"},
   new CityPublicBuilding{Id=1112,X=-3,Y=-2,Name="RECYCLING",Art="warehouse",Role="Public waste and recycling · outside your portfolio"},
   new CityPublicBuilding{Id=1113,X=-2,Y=-2,Name="RIVAL CAFE",Art="cafe",Role="Independent coffee shop · competes for existing demand"},
   new CityPublicBuilding{Id=1114,X=-1,Y=0,Name="CITY MARKET",Art="grocer",Role="Other-owned groceries · competes for household spending"}
  };
  public static CityPublicBuilding Building(int id)=>Buildings.FirstOrDefault(b=>b.Id==id);
  public static CityProperty Property(int id)=>Building(id)?.Footprint();
  public static IEnumerable<CityProperty> Footprints=>Buildings.Select(b=>b.Footprint());
  public static CityHouseholdOccupancy[] Households(int day)=>Buildings.Where(b=>b.Housing).Select(b=>new CityHouseholdOccupancy{PropertyId=b.Id,Units=6,Occupied=6,Families=Enumerable.Range(0,6).Select(unit=>CityResidents.Household(b.Id,unit,day)).ToArray()}).ToArray();
  public static int Population(int day)=>Households(day).Sum(h=>h.Residents);
  public static int AvailableWorkers(int day)=>Math.Max(0,Households(day).Sum(h=>h.Workers)-PublicEmployment);
  public static CityMarketDay Market(int day)=>day%3==1?new CityMarketDay{Name="WHOLESALE DELIVERY",Description="Regional deliveries: +4 import capacity today.",ExtraImportCapacity=4}:day%3==2?new CityMarketDay{Name="NEIGHBORHOOD MARKET",Description="One extra grocery need per household;budgets still limit spending.",ExtraShoppingNeed=1}:new CityMarketDay{Name="ORDINARY TRADING",Description="Standard demand and published supply prices."};
  public static bool Unlocked(CityState city,CityBusinessDefinition d,int population){if(city.Region==0)return city.Day>=d.UnlockDay&&population>=d.UnlockPopulation;if(d.Kind=="cafe"&&city.CareerStage<2)return false;if(d.Kind=="grocer")return population>=d.UnlockPopulation;return city.Day>=d.UnlockDay&&population>=d.UnlockPopulation;}
 }
 public static class CityCareer {
  public static readonly string[] Names={"Your first investment","Your first positive cashflow","Two income streams","Earn $100 from operations","Develop an owned asset","Reach $100 daily cashflow","Established investor"};
  public static readonly string[] Directions={"Compare a rental property. Only assets you own pay you.","Let your completed asset trade through a real operating day.","Operate two businesses with positive daily margins.","Earn $100 in positive daily cashflow,after costs.","Upgrade one of your own properties to level2.","Develop a portfolio forecasting at least $100 net per day.","Diversify,improve margins and expand your portfolio."};
  public static void Advance(CityState city,CityDayResult report){
   bool[] met={city.Properties.Any(p=>p.Level>0),city.EarnedCashflow>0,report!=null&&report.Businesses.Count(b=>b.Net>0)>=2,city.EarnedCashflow>=100,city.Properties.Any(p=>p.Level>=2),report!=null&&report.Net>=100};
   while(city.CareerStage<met.Length&&met[city.CareerStage])city.CareerStage++;
  }
 }
}
