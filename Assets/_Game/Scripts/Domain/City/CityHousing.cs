using System;
using System.Collections.Generic;
using System.Linq;
namespace CashflowChains.City {
 public sealed class CityHouseholdOccupancy {
  public int PropertyId,Units,Occupied;public CityResident[][] Families=new CityResident[0][];
  public int Residents=>Families.Sum(f=>f.Length);public int Adults=>Families.Sum(f=>f.Count(p=>p.Adult));public int Children=>Residents-Adults;public int Workers=>Families.Sum(f=>f.Count(p=>p.WorkingAge));
 }
 public static class CityHousing {
  public static int Units(int level)=>level<1?0:level<5?level:6;
  public static string Label(int level)=>level<2?"HOUSE":level==2?"DUPLEX":level==3?"ROW HOMES":level==4?"SMALL APARTMENTS":"APARTMENT BLOCK";
  public static string Art(int level)=>level>=4?"apartment":"home";
  public static CityHouseholdOccupancy[] Occupancy(CityCatalog c,IEnumerable<CityProperty> source,int gridTier,bool legacy=false,int day=1,bool existingCity=false){
   var a=source.Where(p=>p.Open&&p.Level>0).OrderBy(p=>p.Id).ToArray();int supported=Math.Min((existingCity?CityRegion.RegionalHousingDemand:1)+CityPopulation.Jobs(c,a),c.GridCapacity*gridTier+CityPopulation.GeneratedPower(c,a));var rows=new List<CityHouseholdOccupancy>();
   foreach(var p in a.Where(p=>c.Business(p.Kind).Population>0)){int units=Units(p.Level),occupied=p.AcquiredFrom>0?units:Math.Min(units,Math.Max(0,supported));if(p.AcquiredFrom==0)supported-=occupied;var families=Enumerable.Range(0,occupied).Select(u=>CityResidents.Household(p.AcquiredFrom>0?p.AcquiredFrom:p.Id,u,day,legacy||u<p.LegacyFamilies,legacy)).ToArray();rows.Add(new CityHouseholdOccupancy{PropertyId=p.Id,Units=units,Occupied=occupied,Families=families});}return rows.ToArray();
  }
 }
}
