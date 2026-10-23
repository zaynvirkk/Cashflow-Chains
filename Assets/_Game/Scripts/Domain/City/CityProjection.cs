using System;
using System.Linq;
using CashflowChains.City;
using CashflowChains.Economy;
namespace CashflowChains.Empire {
 public sealed partial class EmpireEngine {
  // Planning-only copy. Never persisted or passed back into an economy command.
  EmpireAccount ProjectNextCityDay(EmpireAccount projection){var city=projection.City;var plan=city.Trading;if(plan==null)return projection;var report=CityDayModel.Forecast(config.City,plan);
   void Amount(string id,long value){var balance=projection.Economy.Balances.Find(b=>b.ItemId==id);if(balance==null){balance=new Balance{ItemId=id};projection.Economy.Balances.Add(balance);}if(value<0)throw new InvalidOperationException("Reserved city stock changed");balance.Earned=value;balance.Purchased=0;}
   foreach(var resource in report.Resources)Amount(resource.ItemId,checked(projection.Economy.Amount(resource.ItemId)+resource.Produced+resource.Imported-resource.Consumed-resource.Exported-resource.Spilled));
   long cash=checked(projection.Economy.Amount("CASH")+report.Revenue),due=checked(report.Bills+report.ImportBills+report.Tax+city.Payables),paid=Math.Min(cash,due);cash-=paid;city.Payables=due-paid;
   foreach(var loan in city.Loans){var original=plan.Loans.FirstOrDefault(l=>l.Id==loan.Id);long installment=original==null?0:Math.Min(loan.Remaining,original.Due(city.Day)),payment=Math.Min(cash,installment);cash-=payment;loan.Remaining-=payment;loan.Repaid+=payment;loan.Arrears=Math.Min(loan.Remaining,installment-payment);}Amount("CASH",cash);
   foreach(var job in city.Jobs.Where(j=>j.FinishAt<=plan.FinishAt)){var p=city.Properties.Find(b=>b.Id==job.PropertyId);if(p!=null)p.Level=job.TargetLevel;}
   city.Day++;city.Trading=null;return projection;
  }
 }
}
