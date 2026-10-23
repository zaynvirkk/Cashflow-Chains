using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using CashflowChains.Economy;
using CashflowChains.Empire;
public sealed class CityProjectionTests {
 // Closed-business fixtures use298Cash to pay80 for housing and18 to restore six timber;production starts at the original100Cash/eight timber. The real200Cash empty start is tested separately.
 BalanceConfig c;
 [SetUp]public void Setup(){c=JsonUtility.FromJson<BalanceConfig>(File.ReadAllText(Path.Combine(Application.dataPath,"_Game/Data/CC2/balance-v1.json")));c.Validate();}
 EmpireEngine Open(){c.City.StarterCash=298;var e=new EmpireEngine(c,new EmpireAccount{Economy=new EconomyState{AccountId="next-day-projection"}},_=>true);e.OpenCity(90);e.BuildCity("fixture-house","home",1,1,91);e.FinishCityBuild("fixture-house",94);e.BuyCityMaterials("fixture-timber","WOOD",6,95);e.BuildCity("cafe","cafe",2,1,101);e.FinishCityBuild("cafe",104);return e;}
 [Test]public void NextDayForecastUsesClosingStockRatherThanTodaysOpeningInventory(){var e=Open();long now=105;for(int day=1;day<=5;day++){e.StartCityDay(now);now+=c.City.DaySeconds(day);e.SettleCityDay(day,now);now++;}e.StartCityDay(now);Assert.AreEqual(1,e.Snapshot.Economy.Amount("BEANS"));string before=JsonUtility.ToJson(e.Snapshot);var next=e.ForecastCity();Assert.AreEqual(7,next.Day);Assert.AreEqual(2,next.ImportBills);Assert.AreEqual(0,next.Resources.Single(r=>r.ItemId=="BEANS").Initial);Assert.AreEqual(before,JsonUtility.ToJson(e.Snapshot));e.SettleCityDay(6,now+c.City.DaySeconds(6));Assert.AreEqual(next.Net,e.ForecastCity().Net);}

 [Test]public void InvestmentQuoteShowsOnlyCurrentlyOwnedStockAndDoesNotSpendProjectedIncome(){var e=Open();e.StartCityDay(105);var before=JsonUtility.ToJson(e.Snapshot);var quote=e.QuoteCityInvestment("home",0,1);Assert.AreEqual(100,quote.Owned.Single(c=>c.ItemId=="CASH").Quantity);Assert.AreEqual(8,quote.Owned.Single(c=>c.ItemId=="WOOD").Quantity);Assert.AreEqual(before,JsonUtility.ToJson(e.Snapshot));}
 [Test]public void LegacyOwnedPropertyAndPendingProjectMigrateWithoutResettingCurrency(){var e=new EmpireEngine(c,new EmpireAccount{Economy=new EconomyState{AccountId="legacy-city-migration"}},_=>true);for(int i=0;i<3;i++)e.RecordCertified(i,CashflowChains.Domain.Campaign.Get(i).Solution,100+i);e.StartConstruction("old-home","home",200);e.OpenCity(201);Assert.AreEqual(380,e.Snapshot.Economy.Amount("CASH"));Assert.AreEqual(1,e.Snapshot.City.Jobs.Count);Assert.AreEqual("old-home",e.Snapshot.City.Jobs[0].Id);e.FinishCityBuild("old-home",230);Assert.AreEqual(1,e.Snapshot.City.Properties.Single(p=>p.Kind=="home").Level);Assert.Throws<System.InvalidOperationException>(()=>e.CollectAll("legacy-output",300));}
}
