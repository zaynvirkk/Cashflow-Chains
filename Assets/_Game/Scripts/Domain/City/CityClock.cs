using System;
using System.Linq;
namespace CashflowChains.Empire {
 public sealed partial class EmpireEngine {
  // Only a trusted clock calls this. Each bounded event commits independently;
  // an interrupted replay resumes from the next uncommitted event.
  public int AdvanceCityTo(long now){lock(gate){if(state.City==null)return 0;if(now<state.Economy.LastTrustedAt)throw new ArgumentException("Clock moved backwards");long floor=Math.Max(1,now-config.City.OfflineCapSeconds);int events=0;while(events<512){var city=state.City;
    if(city.Trading!=null&&city.Trading.FinishAt<floor){int oldDay=city.Day;string key="city-offline-cap:"+oldDay;Command(key,oldDay.ToString(),"city-clock","offline-cap",Math.Max(floor,state.Economy.LastTrustedAt),(a,t)=>{a.City.Trading=null;a.City.Day++;a.City.LastClosedAt=floor;});events++;continue;}
    var job=city.Jobs.OrderBy(j=>j.FinishAt).FirstOrDefault();long buildAt=job?.FinishAt??long.MaxValue,dayAt=city.Trading?.FinishAt??long.MaxValue;long next=Math.Min(buildAt,dayAt);
    if(next<=now){long stamp=Math.Max(next,state.Economy.LastTrustedAt);if(buildAt<=dayAt)FinishCityBuild(job.Id,stamp);else SettleCityDay(city.Day,stamp);events++;continue;}
    if(city.Trading==null&&city.Properties.Any(p=>p.Kind=="cafe"&&p.Level>0)){long start=Math.Max(floor,Math.Max(city.LastClosedAt,state.Economy.LastTrustedAt));StartCityDay(start);events++;continue;}
    break;
   }return events;}}
 }
}
