using System;
using System.Collections.Generic;
using System.Linq;
namespace CashflowChains.City {
 public static class CityPopulation {
  public static int GeneratedPower(CityCatalog c,IEnumerable<CityProperty> properties)=>properties.Where(p=>p.Open&&p.Level>0).Sum(p=>(int)(c.Business(p.Kind).Outputs.Where(o=>o.ItemId=="ENERGY").Sum(o=>o.Quantity*p.Level)*(long)CityWorkforce.Productivity(c.Business(p.Kind),p)/10000));
  public static int Capacity(CityCatalog c,IEnumerable<CityProperty> properties)=>properties.Where(p=>p.Open&&p.Level>0).Sum(p=>c.Business(p.Kind).Population*p.Level);
  public static int Jobs(CityCatalog c,IEnumerable<CityProperty> properties)=>properties.Where(p=>p.Open&&p.Level>0).Sum(p=>CityWorkforce.Staff(c.Business(p.Kind),p));
  public static int Residents(CityCatalog c,IEnumerable<CityProperty> source,int gridTier){var properties=source.ToArray();int capacity=Capacity(c,properties),jobs=Jobs(c,properties);int generated=GeneratedPower(c,properties);int infrastructure=Math.Max(12,(c.GridCapacity*gridTier+generated)*6);return Math.Min(capacity,Math.Min(12+jobs*6,infrastructure));}
 }
}
