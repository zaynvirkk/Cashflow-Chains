using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using CashflowChains.City;
using CashflowChains.Economy;
using CashflowChains.Empire;
public sealed class CityOperatingIntegrityTests {
 BalanceConfig c;
 [SetUp]public void Setup(){c=JsonUtility.FromJson<BalanceConfig>(File.ReadAllText(Path.Combine(Application.dataPath,"_Game/Data/CC2/balance-v1.json")));c.Validate();}
 CityOperatingPlan Plan(params CityProperty[] properties)=>new CityOperatingPlan{Day=1,StartAt=100,FinishAt=200,CashAvailable=1000,Properties=properties,Inventory=new ItemAmount[0]};
 [Test]public void GridChargesOnlyDeliveredPowerAndAllocationCannotMultiplyTheBill(){
  var homes=Enumerable.Range(1,5).Select(id=>new CityProperty{Id=id,Kind="home",Level=5,X=id}).ToArray();
  var shortage=CityDayModel.Forecast(c.City,Plan(homes));Assert.AreEqual(25,shortage.PowerDemand);Assert.AreEqual(20,shortage.PowerAvailable);
  long baseline=homes.Sum(p=>Math.Max(2,c.City.Business("home").FixedBills)*p.Level);Assert.AreEqual(20,shortage.Bills-baseline);
  var plan=Plan(homes);plan.Day=22;Assert.AreEqual(30,CityDayModel.Forecast(c.City,plan).Bills-baseline);
 }
 [Test]public void UnstaffedPowerPlantCannotGenerateElectricityOrSupportMoreResidents(){
  var home=new CityProperty{Id=1,Kind="home",Level=5};var plant=new CityProperty{Id=2,Kind="power",Level=1,X=1,Staff=0};
  var stopped=CityDayModel.Forecast(c.City,Plan(home,plant));Assert.AreEqual(20,stopped.PowerAvailable);
  plant.Staff=1;Assert.AreEqual(60,CityDayModel.Forecast(c.City,Plan(home,plant)).PowerAvailable);
  Assert.AreEqual(CityPopulation.GeneratedPower(c.City,new[]{home,plant}),40);
 }
 [Test]public void FactoryDeliveriesCannotLeaveBeforeTheirInputArrives(){
  var plan=Plan(new CityProperty{Id=1,Kind="factory",X=11,Y=11,Level=1},new CityProperty{Id=2,Kind="cafe",X=10,Y=11,Level=1},new CityProperty{Id=3,Kind="home",X=9,Y=11,Level=1});
  var r=CityDayModel.Forecast(c.City,plan);var timber=r.Flows.Single(f=>f.Kind=="WOOD");var cups=r.Flows.First(f=>f.Kind=="CUPS"&&f.Source==1);
  Assert.GreaterOrEqual(cups.Minute,timber.Minute+timber.TravelMinutes);
 }
 [Test]public void InvestmentForecastDeductsBuildCashBeforePurchasingOperatingInputs(){
  var e=new EmpireEngine(c,new EmpireAccount{Economy=new EconomyState{AccountId="working-capital"}},_=>true);e.OpenCity(100);e.BuildCity("cafe","cafe",2,1,101);e.FinishCityBuild("cafe",104);
  long now=105;for(int i=1;i<=3;i++){e.StartCityDay(now);now+=c.City.DaySeconds(i);e.SettleCityDay(i,now);now++;}
  e.BuyCityMaterials("timber","WOOD",8,now);e.BuyCityMaterials("steel","STEEL",4,now);
  var quote=e.QuoteCityInvestment("warehouse",0,1);Assert.True(quote.Affordable);var before=JsonUtility.ToJson(e.Snapshot);
  e.BuildCity("warehouse","warehouse",0,1,now);e.FinishCityBuild("warehouse",now+8);
  Assert.AreEqual(e.ForecastCity().Net,quote.AfterNet);Assert.AreNotEqual(before,JsonUtility.ToJson(e.Snapshot));
 }
 [Test]public void HiringReservesWorkersAlreadyCommittedToUnfinishedBusinesses(){
  var e=new EmpireEngine(c,new EmpireAccount{Economy=new EconomyState{AccountId="workers-reserved"}},_=>true);e.OpenCity(100);e.BuildCity("cafe","cafe",2,1,101);e.FinishCityBuild("cafe",104);
  long now=105;while(e.Snapshot.City.Day<4){e.StartCityDay(now);now+=c.City.DaySeconds(e.Snapshot.City.Day);e.SettleCityDay(e.Snapshot.City.Day,now);now++;}
  e.BuildCity("farm","farm",0,1,now);Assert.Throws<InvalidOperationException>(()=>e.StaffCityBusiness("overhire",2,6,now+1));
 }
}
