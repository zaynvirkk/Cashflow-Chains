using System;
using System.Linq;
namespace CashflowChains.City {
 public sealed class CityResident {
  public int Id,Age,DailyBudget;public string Gender;public bool LikesCoffee,LikesCinema;
  public bool Adult=>Age>=18;public bool WorkingAge=>Age>=18&&Age<65;public bool Senior=>Age>=65;
 }
 public static class CityResidents {
  // NPC profiles are independent of the player's identity. Gender never changes
  // prices,wages or buying power;age,means and individual interests drive needs.
  static readonly int[][] Ages={new[]{32,29,7,11},new[]{24,26},new[]{68,72},new[]{38},new[]{35,16,5},new[]{21,23,25,27}};
  public static CityResident[] Household(int property,int unit,int day,bool legacy=false,bool frozenAges=false){int type=legacy?0:((property-1)+unit)%Ages.Length;return Ages[type].Select((baseAge,i)=>{
   int id=property*10000+unit*100+i,age=baseAge+(frozenAges?0:Math.Max(0,day-1)/365);uint identity=unchecked((uint)id*2654435761u);string gender=identity%20==0?"Non-binary":identity%2==0?"Woman":"Man";int budget=age<18?0:age>=65?34:40+(int)(identity%3)*8;
   return new CityResident{Id=id,Age=age,Gender=gender,DailyBudget=budget,LikesCoffee=age>=18&&(type==0||identity%3!=0),LikesCinema=age>=13&&identity%3!=1};}).ToArray();}
 }
}
