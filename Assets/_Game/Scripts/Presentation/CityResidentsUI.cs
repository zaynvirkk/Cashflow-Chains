using System;
using System.Linq;
using CashflowChains.City;
using LocalGame;
using TMPro;
using UnityEngine;
namespace CashflowChains.Presentation {
 public sealed partial class MobileGameApp {
  void WorldResidents(){Base("city-residents");var a=empire.Snapshot;var r=empire.ForecastCity();Studio.Header(root,"", "Your people",r.Population+" residents · "+r.Households+" occupied homes",PermanentCityScreen);
   if(r.Population==0){Studio.Image(root,"home",190,280,340,350);Studio.Label(root,"A city starts with a home.",38,650,644,90,40,Studio.Text,TextAlignmentOptions.Center,true);Studio.Label(root,"Build your first house. Residents arrive after construction;empty land creates no people or income.",58,770,604,160,29,Studio.Text,TextAlignmentOptions.Center);Studio.Button(root,"BUILD FIRST HOME",38,1100,644,110,()=>WorldBuildPicker(1,1,0),true);return;}
   var people=CityHousing.Occupancy(empireConfig.City,a.City.Properties,a.City.GridTier,false,a.City.Day).SelectMany(h=>h.Families).SelectMany(f=>f).ToArray();int[] count={people.Count(p=>p.Age<13),people.Count(p=>p.Age>=13&&p.Age<18),people.Count(p=>p.WorkingAge),people.Count(p=>p.Senior)};string[] label={"CHILDREN","TEENS","WORKING AGE","RETIRED"};
   for(int i=0;i<4;i++){var card=Studio.Box(root,"Age group "+i,38+i%2*330,265+i/2*155,314,140,FlatUI.Hex(i==2?"#155447":"#10384D"));Studio.Label(card,count[i].ToString(),20,80,680,600,52,Studio.Text,TextAlignmentOptions.Center,true);Studio.Label(card,label[i],20,800,680,320,22,Studio.Accent,TextAlignmentOptions.Center,true);}
   Studio.Label(root,r.Women+" women · "+r.Men+" men · "+r.NonBinary+" non-binary",38,610,644,55,27,Studio.Text,TextAlignmentOptions.Center);Studio.Label(root,"Different people,different needs.",38,700,644,80,35,Studio.Text,TextAlignmentOptions.Center,true);Studio.Label(root,"Age,retirement,household budgets and personal interests affect shopping,coffee and entertainment. Children create no coffee orders;retirees do not fill local jobs. Gender is identity,not a price or wage multiplier.",48,810,624,235,28,Studio.Text,TextAlignmentOptions.Center);Studio.Button(root,"INSPECT A HOME",38,1120,644,100,()=>WorldHouseholds(a.City.Properties.First(p=>empireConfig.City.Business(p.Kind).Population>0&&p.Level>0).Id,0),true);Probe();
  }
  void WorldHouseholds(int propertyId,int page){var a=empire.Snapshot;var p=a.City.Properties.Single(b=>b.Id==propertyId);var h=CityHousing.Occupancy(empireConfig.City,a.City.Properties,a.City.GridTier,false,a.City.Day).FirstOrDefault(row=>row.PropertyId==p.Id);var m=Studio.Modal(root,PropertyLabel(p)+" · HOUSEHOLDS",()=>WorldPropertyDetails(propertyId));
   if(h==null||h.Occupied==0){Studio.Label(m,"This home is vacant.\n\nNobody lives here or pays rent yet. Develop jobs,utilities and a useful neighborhood to attract another household.",44,280,632,580,31,Studio.Text,TextAlignmentOptions.Center);return;}
   page=Math.Max(0,Math.Min(h.Occupied-1,page));var family=h.Families[page];Studio.Label(m,"HOME "+(page+1)+" / "+h.Units+" · "+family.Length+" RESIDENTS",44,225,632,70,29,Studio.Accent,TextAlignmentOptions.Center,true);
   for(int i=0;i<family.Length;i++){var person=family[i];float y=330+i*142;var card=Studio.Box(m,"Resident "+person.Id,44,y,632,128,FlatUI.Hex("#10394C"));string role=person.Senior?"Retired":person.WorkingAge?"Working age":person.Age>=13?"Teen":"Child";Studio.Label(card,person.Age+" YEARS · "+person.Gender,24,105,672,410,27,Studio.Text,heavy:true);Studio.Label(card,role+" · "+(person.LikesCoffee?"Coffee fan":person.Adult?"Other drinks":"No coffee orders"),24,665,672,400,23,Studio.Accent);}
   long means=family.Sum(person=>(long)person.DailyBudget),rent=empireConfig.City.Business(p.Kind).Rent;Studio.Label(m,"Daily household budget "+CityMoney(means)+"\nRent "+CityMoney(rent)+" · up to "+CityMoney(Math.Max(0,means-rent))+" left for purchases",44,930,632,130,27,Studio.Text,TextAlignmentOptions.Center);
   if(h.Occupied>1){Studio.Button(m,"PREVIOUS HOME",44,1120,300,90,()=>WorldHouseholds(propertyId,(page+h.Occupied-1)%h.Occupied));Studio.Button(m,"NEXT HOME",360,1120,316,90,()=>WorldHouseholds(propertyId,(page+1)%h.Occupied),true);}else Studio.Button(m,"MY CITY",44,1120,632,90,PermanentCityScreen,true);
  }
 }
}
