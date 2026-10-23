using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using CashflowChains.City;
using CashflowChains.Economy;
using CashflowChains.Empire;
public sealed class CityClockTests {
 // Closed-business fixtures use298Cash to pay80 for housing and18 to restore six timber;production starts at the original100Cash/eight timber. The real200Cash empty start is tested separately.
 BalanceConfig c;
 [SetUp]public void Setup(){c=JsonUtility.FromJson<BalanceConfig>(File.ReadAllText(Path.Combine(Application.dataPath,"_Game/Data/CC2/balance-v1.json")));c.Validate();}
 EmpireEngine Open(){c.City.StarterCash=298;var e=new EmpireEngine(c,new EmpireAccount{Economy=new EconomyState{AccountId="city-clock"}},_=>true);e.OpenCity(90);e.BuildCity("fixture-house","home",1,1,91);e.FinishCityBuild("fixture-house",94);e.BuyCityMaterials("fixture-timber","WOOD",6,95);e.BuildCity("cafe","cafe",2,1,101);return e;}
 [Test]public void ClockCompletesTheActualBuilderAndFirstQuickDayWithoutASkip(){var e=Open();e.AdvanceCityTo(103);Assert.AreEqual(0,e.Snapshot.City.Properties.Last().Level);e.AdvanceCityTo(104);Assert.AreEqual(1,e.Snapshot.City.Properties.Last().Level);Assert.AreEqual(109,e.Snapshot.City.Trading.FinishAt);e.AdvanceCityTo(108);Assert.AreEqual(1,e.Snapshot.City.Day);e.AdvanceCityTo(109);Assert.AreEqual(2,e.Snapshot.City.Day);Assert.Greater(e.Snapshot.Economy.Amount("CASH"),100);}
 [Test]public void ResumeProcessesBoundedEarnedDaysAndRepeatingTheSameTimeDoesNotRegrant(){var e=Open();e.AdvanceCityTo(2000);var before=e.Snapshot;Assert.Greater(before.City.Day,5);Assert.AreEqual(0,e.AdvanceCityTo(2000));Assert.AreEqual(before.Economy.Amount("CASH"),e.Snapshot.Economy.Amount("CASH"));Assert.AreEqual(before.Economy.Revision,e.Snapshot.Economy.Revision);}
 [Test]public void LongAbsenceCapsSimulationAndRetainsTheSameCity(){var e=Open();e.AdvanceCityTo(109);e.AdvanceCityTo(100000);var a=e.Snapshot;Assert.Less(a.City.Day,120);Assert.AreEqual(2,a.City.Properties.Count);Assert.AreEqual(1,a.Economy.Receipts.Count(r=>r.CommandId.StartsWith("city-offline-cap:")));Assert.GreaterOrEqual(a.City.Trading.StartAt,100000-c.City.OfflineCapSeconds);Assert.Throws<ArgumentException>(()=>e.AdvanceCityTo(a.Economy.LastTrustedAt-1));}
 [Test]public void EmptyHousingCannotCreateUnlimitedRentOrCoffeeDemand(){var cty=c.City;var properties=Enumerable.Range(1,10).Select(i=>new CityProperty{Id=i,Kind="home",Level=1,X=i}).ToArray();var p=new CityOperatingPlan{Day=1,StartAt=100,FinishAt=105,CashAvailable=0,Properties=properties,Inventory=new ItemAmount[0]};var r=CityDayModel.Forecast(cty,p);Assert.AreEqual(40,r.HousingCapacity);Assert.AreEqual(4,r.Population);Assert.AreEqual(0,r.Jobs);Assert.AreEqual(2,r.CoffeeDemand);Assert.Less(r.Revenue,10*cty.Business("home").Rent);}
}
