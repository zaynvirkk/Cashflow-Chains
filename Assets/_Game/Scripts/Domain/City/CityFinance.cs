using System;
using System.Linq;
using CashflowChains.Economy;
namespace CashflowChains.City {
 [Serializable] public sealed class CityLoan {
  public string Id;public long Principal,Fee,Remaining,Installment,Arrears,Repaid;public int StartDay,TermDays;
  public CityLoan Copy()=>(CityLoan)MemberwiseClone();
  public long Due(int day)=>day<=StartDay||Remaining==0?0:Math.Min(Remaining,checked(Installment+Arrears));
  public void Validate(EconomyState e){if(TermDays!=10||string.IsNullOrEmpty(Id)||Principal<1||Fee!=Principal/10||Remaining<0||Remaining>Principal+Fee||Installment!=(Principal+Fee+TermDays-1)/TermDays||Arrears<0||Arrears>Remaining||Repaid<0||Remaining+Repaid!=Principal+Fee||TermDays!=10||StartDay<1||!e.Receipts.Any(r=>r.CommandId==Id)||e.Ledger.Where(l=>l.CorrelationId==Id&&l.ItemId=="CASH").Sum(l=>l.Delta)!=Principal)throw new ArgumentException("Invalid city loan");}
 }
 public static class CityWorkforce {
  public static int Staff(CityBusinessDefinition d,CityProperty p)=>p.Staff<0?d.BaseStaff:p.Staff;
  public static int Productivity(CityBusinessDefinition d,CityProperty p)=>d.BaseStaff==0?10000:Math.Min(20000,Staff(d,p)*10000/d.BaseStaff);
 }
}
namespace CashflowChains.Empire {
 public sealed partial class EmpireEngine {
  public CashflowChains.Economy.CommandReceipt FinanceCity(string id,long principal,long now)=>Command(id,principal.ToString(),"city-finance","loan-principal",now,(a,t)=>{var c=RequiredCity(a);if(c.Day<4||c.Loans.Any(l=>l.Remaining>0)||principal!=100&&principal!=300&&principal!=600||principal==600&&c.Population(config.City)<72)throw new InvalidOperationException("Financing unlocks on day4; repay existing borrowing first. The largest line needs72 residents.");long fee=principal/10;t.Grant("CASH",principal);c.Loans.Add(new CashflowChains.City.CityLoan{Id=id,Principal=principal,Fee=fee,Remaining=principal+fee,Installment=(principal+fee+9)/10,StartDay=c.Day,TermDays=10});});
  public CashflowChains.Economy.CommandReceipt RepayCityLoan(string id,string loanId,long amount,long now)=>Command(id,loanId+":"+amount,"city-finance","early-repayment",now,(a,t)=>{var loan=RequiredCity(a).Loans.Find(l=>l.Id==loanId)??throw new ArgumentException("Unknown borrowing");if(amount<1||amount>loan.Remaining)throw new ArgumentException("Choose an amount within the remaining balance");t.Spend("CASH",amount);loan.Remaining-=amount;loan.Repaid+=amount;loan.Arrears=Math.Min(loan.Arrears,loan.Remaining);});
  public CashflowChains.Economy.CommandReceipt StaffCityBusiness(string id,int propertyId,int staff,long now)=>Command(id,propertyId+":"+staff,"city-hiring","staff-change",now,(a,t)=>{var c=RequiredCity(a);var p=c.Properties.Find(b=>b.Id==propertyId)??throw new ArgumentException("Unknown business");var d=config.City.Business(p.Kind);if(staff<0||staff>d.MaxStaff||d.BaseStaff==0)throw new ArgumentException("Invalid staffing level");int current=CashflowChains.City.CityWorkforce.Staff(d,p);int other=c.Properties.Where(b=>b.Id!=p.Id&&b.Open).Sum(b=>CashflowChains.City.CityWorkforce.Staff(config.City.Business(b.Kind),b));if(other+staff>c.Population(config.City)/2)throw new InvalidOperationException("Not enough local workers. Develop housing before hiring more staff.");if(staff>current)t.Spend("CASH",(staff-current)*20L);p.Staff=staff;});
  public CashflowChains.Economy.CommandReceipt MarketCityBusiness(string id,int propertyId,int budget,long now)=>Command(id,propertyId+":"+budget,"city-marketing","campaign-budget",now,(a,t)=>{var c=RequiredCity(a);var p=c.Properties.Find(b=>b.Id==propertyId)??throw new ArgumentException("Unknown business");if(c.Day<3||config.City.Business(p.Kind).Sales==0||budget!=0&&budget!=5&&budget!=15)throw new InvalidOperationException("Choose an available daily campaign after day3");p.MarketingBudget=budget;});
  public CashflowChains.Economy.CommandReceipt BuyCityMaterials(string id,string item,int quantity,long now)=>Command(id,item+":"+quantity,"city-materials","published-market-price",now,(a,t)=>{RequiredCity(a);var good=config.City.Good(item);if(quantity<1||quantity>20)throw new ArgumentException("Buy1–20 units per order");t.Spend("CASH",checked(good.ImportPrice*(long)quantity));t.Grant(item,quantity);});
  public CashflowChains.Economy.CommandReceipt AgreeCityPartner(string id,bool sponsorship,long now)=>Command(id,sponsorship?"coffee-fair":"local-roaster","city-agreements","sign-agreement",now,(a,t)=>{var c=RequiredCity(a);if(c.Day<(sponsorship?7:5))throw new InvalidOperationException("This agreement is not unlocked yet");if(sponsorship){if(c.SponsorEndsDay>=c.Day)throw new InvalidOperationException("Current sponsorship is still active");c.Sponsor="coffee-fair";c.SponsorEndsDay=c.Day+2;}else{if(c.PartnerEndsDay>=c.Day)throw new InvalidOperationException("Current supplier agreement is still active");c.Partner="local-roaster";c.PartnerEndsDay=c.Day+2;}});
 }
}
