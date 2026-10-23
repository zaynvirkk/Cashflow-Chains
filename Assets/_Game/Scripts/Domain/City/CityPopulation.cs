using System;
using System.Collections.Generic;
using System.Linq;
namespace CashflowChains.City {
 public static class CityPopulation {
  public static int GeneratedPower(CityCatalog c,IEnumerable<CityProperty> properties)=>properties.Where(p=>p.Open&&p.Level>0).Sum(p=>(int)(c.Business(p.Kind).Outputs.Where(o=>o.ItemId=="ENERGY").Sum(o=>o.Quantity*p.Level)*(long)CityWorkforce.Productivity(c.Business(p.Kind),p)/10000));
  public static int Capacity(CityCatalog c,IEnumerable<CityProperty> properties)=>properties.Where(p=>p.Open&&p.Level>0).Sum(p=>CityHousing.Units(p.Level)*c.Business(p.Kind).Population);
  public static int Jobs(CityCatalog c,IEnumerable<CityProperty> properties)=>properties.Where(p=>p.Open&&p.Level>0).Sum(p=>CityWorkforce.Staff(c.Business(p.Kind),p));
  public static int Residents(CityCatalog c,IEnumerable<CityProperty> source,int gridTier,bool legacy=false,int day=1)=>CityHousing.Occupancy(c,source,gridTier,legacy,day).Sum(h=>h.Residents);
  public static int Workers(CityCatalog c,IEnumerable<CityProperty> source,int gridTier,int day=1)=>CityHousing.Occupancy(c,source,gridTier,false,day).Sum(h=>h.Workers);
  public static int Adults(CityCatalog c,IEnumerable<CityProperty> source,int gridTier)=>CityHousing.Occupancy(c,source,gridTier).Sum(h=>h.Adults);
 }
}
