using System;
using System.Linq;
using CashflowChains.Economy;
namespace CashflowChains.City {
 [Serializable] public sealed class CityBusinessDefinition {
  public string Kind,Family,Art;public int BuildCash,BuildSeconds,UnlockPopulation,UnlockDay,Population,Rent,FixedBills,Power,Sales,Price,Storage,Tourists,ImportCapacity,ExportCapacity;
  public int BaseStaff,MaxStaff,DailyWage;public ItemAmount[] BuildMaterials=new ItemAmount[0];
  public ItemAmount[] Inputs=new ItemAmount[0],Outputs=new ItemAmount[0];
 }
 [Serializable] public sealed class CityGoodsDefinition {public string Id,Label;public int ImportPrice,ExportPrice;}
 [Serializable] public sealed class CityCatalog {
  public int Revision=1,StarterCash=200,InitialSize=4,MaxSize=12,DayCapSeconds=300,OfflineCapSeconds=28800,GridCapacity=20,GridUnitCost=1,TaxBasisPoints=1000,CompetitorWeight=50,BaseStorage=40;
  public CityBusinessDefinition[] Businesses;public CityGoodsDefinition[] Goods;
  public CityBusinessDefinition Business(string kind)=>Businesses.FirstOrDefault(b=>b.Kind==kind)??throw new ArgumentException("Unknown city business");
  public CityGoodsDefinition Good(string id)=>Goods.FirstOrDefault(g=>g.Id==id)??throw new ArgumentException("Unknown city cargo");
  public int DaySeconds(int day)=>Math.Min(DayCapSeconds,day==1?5:day==2?8:day==3?12:day==4?20:day==5?30:day==6?45:day==7?60:day==8?90:day==9?120:day<20?180:300);
  public void Validate(BalanceConfig balance){if((Revision!=1&&Revision!=2&&Revision!=3)||StarterCash<0||InitialSize!=4||MaxSize<6||MaxSize>16||DayCapSeconds<120||DayCapSeconds>600||OfflineCapSeconds<1||OfflineCapSeconds>28800||GridCapacity<1||GridUnitCost<1||TaxBasisPoints<0||TaxBasisPoints>3000||CompetitorWeight<1||BaseStorage<10||Businesses==null||Goods==null||Businesses.Select(b=>b.Kind).Distinct().Count()!=Businesses.Length||Goods.Select(g=>g.Id).Distinct().Count()!=Goods.Length)throw new ArgumentException("Invalid city catalog");foreach(var g in Goods){balance.Item(g.Id);if(g.ImportPrice<1||g.ExportPrice<0||g.ExportPrice>=g.ImportPrice)throw new ArgumentException("Unprofitable or invalid market spread");}foreach(var b in Businesses){CashflowChains.Domain.Pieces.Get(b.Art??b.Kind);if(Revision>=2&&b.Kind=="home"&&b.Population!=4)throw new ArgumentException("One household has four residents");if(b.BuildCash<1||b.BuildSeconds<1||b.UnlockDay<1||b.UnlockPopulation<0||b.FixedBills<0||b.Power<0||b.Price<0||b.Inputs==null||b.Outputs==null||b.BuildMaterials==null||b.BaseStaff<0||b.MaxStaff<b.BaseStaff||b.DailyWage<0)throw new ArgumentException("Invalid city business definition");foreach(var amount in b.Inputs.Concat(b.Outputs).Concat(b.BuildMaterials))if(amount.Quantity<1||amount.Quantity>1000)throw new ArgumentException("Invalid city recipe");else balance.Item(amount.ItemId);}}
 }
}
