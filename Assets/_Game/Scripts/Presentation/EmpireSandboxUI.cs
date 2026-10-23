using System;
using System.IO;
using System.Linq;
using CashflowChains.Economy;
using CashflowChains.Empire;
using LocalGame;
using TMPro;
using UnityEngine;

namespace CashflowChains.Presentation {
 public sealed partial class MobileGameApp {
  bool empireSandbox,empireWritable=true;EmpireEngine empire;BalanceConfig empireConfig;AtomicSave empireDisk;int empirePage;long empireCashReward,empireWoodReward,empireChestReward;
  long EmpireNow=>DateTimeOffset.UtcNow.ToUnixTimeSeconds();
  void InitializeEmpireSandbox(string directory){
   if(!empireSandbox)return;empireConfig=JsonUtility.FromJson<BalanceConfig>(Resources.Load<TextAsset>("CC2/balance-v1").text);empireConfig.Validate();empireDisk=new AtomicSave(Path.Combine(directory,"cc2-local-empire-v3.json"));
   var account=empireDisk.Read<EmpireAccount>(a=>a.Validate(empireConfig),out string recovery);empireWritable=!recovery.StartsWith("Save unreadable");
   account=account??new EmpireAccount{Economy=new EconomyState{AccountId="sandbox-"+Guid.NewGuid().ToString("N")}};
   if(data.Goal!=null){try{data.Goal.Validate(empireConfig);}catch(ArgumentException){data.Goal=null;}}
   empire=new EmpireEngine(empireConfig,account,a=>{if(!empireWritable)return false;try{empireDisk.Write(a);return true;}catch(Exception){empireWritable=false;return false;}});
   var banner=FlatUI.Panel(root.parent,"Empire sandbox watermark",0,1252,720,28,new Color(.015f,.03f,.06f,.97f));var canvas=banner.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=200;Studio.Label(banner,"CC2 LOCAL EMPIRE SANDBOX · NO CLOUD OR PURCHASES",0,0,720,1280,12,Color.white,TextAlignmentOptions.Center).raycastTarget=false;banner.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
  }
  void RecordEmpireRound(){empireCashReward=empireWoodReward=empireChestReward=0;if(!empireSandbox||run.Stars(result.Credits)<1)return;try{var before=empire.Snapshot.Economy;empire.RecordCertified(run.Mission.Index,run.Board,EmpireNow);var after=empire.Snapshot.Economy;empireCashReward=after.Amount("CASH")-before.Amount("CASH");empireWoodReward=after.Amount("WOOD")-before.Amount("WOOD");empireChestReward=after.Amount("CHEST:bronze")-before.Amount("CHEST:bronze");}catch(Exception e){message="Empire reward preserved for retry: "+e.Message;}WriteEmpireProbe();}
  string EmpireRewardText=>empireCashReward>0?"+"+empireCashReward+" CASH · +"+empireWoodReward+" WOOD"+(empireChestReward>0?" · SUPPLY CHEST EARNED":""):"FIRST-CLEAR REWARD ALREADY SAVED";
  void WriteEmpireProbe(){string dir=FlatUI.Arg("-capture-dir");if(!empireSandbox||empire==null||dir==null)return;Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"empire-state.json"),JsonUtility.ToJson(empire.Snapshot,true));}
  void EmpireAction(Action action,Action refresh=null){bool failed=false;try{action();sound.Cue("cash");message="";}catch(Exception e){sound.Cue("deny");message=e.Message;failed=true;}WriteEmpireProbe();Action next=refresh??EmpireScreen;next();if(failed){var m=Studio.Modal(root,"NOT READY YET",next);Studio.Label(m,message,44,300,632,600,31,Studio.Text,TextAlignmentOptions.Center);Studio.Button(m,"BACK TO MY CITY",44,1090,632,135,next,true);}}
  string EmpireDuration(long seconds)=>seconds<=0?"READY":seconds<60?seconds+"s":seconds<3600?(seconds/60)+"m "+(seconds%60)+"s":(seconds/3600)+"h "+(seconds%3600/60)+"m";
  string EmpireCost(UpgradeDefinition upgrade)=>string.Join(" · ",upgrade.Costs.Select(x=>x.Quantity+" "+x.ItemId));
  void ContinueEmpirePuzzle(){if(run.Finished&&run.Stars(run.Preview().Credits)>0&&data.Profile.Selected<71&&data.Profile.CanSelect(data.Profile.Selected+1))StartLevel(data.Profile.Selected+1);else Board();}
  void EmpireScreen(){
   if(!empireSandbox)return;Base("empire");var a=empire.Snapshot;long now=EmpireNow;int clears=a.Economy.Receipts.Count(x=>x.CommandId.StartsWith("certified-first-clear:"));
   Studio.Header(root,"YOUR CHAINS BUILD SOMETHING LASTING","My permanent city",a.Economy.Amount("CASH").ToString("N0")+" CASH  ·  "+a.Businesses.Count(x=>x.Level>0)+" BUSINESSES",ContinueEmpirePuzzle);
   DrawDevelopmentGoal();
   for(int i=0;i<6;i++){
    int index=empirePage*6+i;if(index>=empireConfig.Businesses.Length)break;var def=empireConfig.Businesses[index];var owned=a.Businesses.Find(x=>x.Id==def.Id);var project=a.Construction.Find(x=>x.BusinessId==def.Id);bool locked=clears<def.UnlockClears;float x=38+i%2*330,y=290+i/2*235;
    Studio.Box(root,"Empire plot "+def.Id,x,y,314,229,FlatUI.Hex(locked?"#05263FEB":"#003B4AEF"));var art=Studio.Image(root,def.Recipe,x+4,y+6,132,137);art.GetComponent<UnityEngine.UI.Image>().preserveAspect=true;if(locked)art.GetComponent<UnityEngine.UI.Image>().color=new Color(.5f,.6f,.65f,.6f);
    Studio.Label(root,Domain.Pieces.Get(def.Recipe).Label,x+139,y+16,166,45,23,locked?Studio.Muted:Studio.Accent,heavy:true);Studio.Label(root,locked?"AT "+def.UnlockClears+" CITIES":project!=null?"BUILDING":owned==null||owned.Level==0?"NEW FRANCHISE":"LEVEL "+owned.Level,x+139,y+62,166,45,21,Studio.Text);
    Studio.Label(root,owned!=null&&owned.Level>0?def.BaseHourly*(10000L+Math.Min((owned.Level-1)*800+EmpireBenefits.Bonus(empireConfig,a,"production-bp"),empireConfig.ProductionBonusCapBp))/10000m+" "+def.Output+" / HOUR":def.Rarity.ToUpperInvariant()+" · "+def.Role.ToUpperInvariant(),x+10,y+112,294,38,20,Studio.Muted);
    var button=Studio.Button(root,locked?"SEE UNLOCK":project!=null?"VIEW CONSTRUCTION":owned!=null&&owned.Level>0?"DEVELOP":"BUILD HERE",x+8,y+145,298,80,()=>EmpireBusiness(def.Id),!locked);button.interactable=true;
   }
   Studio.Button(root,"‹",38,1010,86,80,()=>{empirePage=(empirePage+1)%2;EmpireScreen();});Studio.Label(root,(empirePage+1)+" / 2",132,1023,100,45,22,Studio.Muted,TextAlignmentOptions.Center);Studio.Button(root,"›",244,1010,86,80,()=>{empirePage=(empirePage+1)%2;EmpireScreen();});Studio.Button(root,"BUILDER "+a.Construction.Count+" / "+a.Builders,350,1010,332,80,EmpireQueue);
   long available=a.Businesses.Where(b=>b.Level>0).Sum(b=>ProductionPolicy.Quote(empireConfig,b,Math.Max(now,b.ProducedAt),EmpireBenefits.Bonus(empireConfig,a,"production-bp"),EmpireBenefits.Bonus(empireConfig,a,"storage-bp")).Amount);var collect=Studio.Button(root,"COLLECT OUTPUT · "+available,38,1100,644,82,()=>EmpireAction(()=>empire.CollectAll(Guid.NewGuid().ToString("N"),EmpireNow)),true);var liveCollect=collect.gameObject.AddComponent<EmpireCollectState>();liveCollect.Read=()=>{var current=empire.Snapshot;long at=EmpireNow;return current.Businesses.Where(b=>b.Level>0).Sum(b=>{var ready=current.Construction.FirstOrDefault(p=>p.BusinessId==b.Id&&p.FinishAt<at);long through=ready!=null?ready.FinishAt:at;return ProductionPolicy.Quote(empireConfig,b,Math.Max(through,b.ProducedAt),EmpireBenefits.Bonus(empireConfig,current,"production-bp"),EmpireBenefits.Bonus(empireConfig,current,"storage-bp")).Amount;});};liveCollect.CanSave=()=>empireWritable;
   Studio.Button(root,"BOARD",38,1198,200,80,ContinueEmpirePuzzle);Studio.Button(root,"DISPATCH",248,1198,208,80,EmpireDispatch);Studio.Button(root,"RESEARCH",466,1198,216,80,()=>EmpireResearch(0,0));WriteEmpireProbe();
  }
  void EmpireBusiness(string id)=>RenderDevelopmentBusiness(id);
  void EmpireSources(string id)=>OpenDevelopmentSources(id);
  void EmpireQueue(){var a=empire.Snapshot;var m=Studio.Modal(root,"CONSTRUCTION KEEPS WORKING",EmpireScreen);if(a.Construction.Count==0){Studio.Label(m,"Your builder is ready.\nChoose a franchise on the permanent city.\n\nYou can always keep solving puzzles while construction runs.",44,300,632,420,32,Studio.Text,TextAlignmentOptions.Center);Studio.Button(m,"CHOOSE A BUSINESS",44,1090,632,130,EmpireScreen,true);return;}
   var p=a.Construction[0];Studio.Image(m,p.BusinessId,210,220,300,300);Studio.Label(m,Domain.Pieces.Get(p.BusinessId).Label+" · LEVEL "+p.TargetLevel,44,550,632,100,35,Studio.Accent,TextAlignmentOptions.Center,true);var timer=Studio.Label(m,"",44,680,632,120,44,Studio.Text,TextAlignmentOptions.Center,true);var live=timer.gameObject.AddComponent<EmpireCountdown>();live.FinishAt=p.FinishAt;var finish=Studio.Button(m,"COMPLETE CONSTRUCTION",44,930,632,115,()=>EmpireAction(()=>empire.FinishConstruction(Guid.NewGuid().ToString("N"),p.Id,EmpireNow)),true);live.FinishButton=finish;if(a.Economy.Amount("CONSTRUCTION_SPEEDUP_MIN")>0){Studio.Label(m,"Uses one minute token; unused seconds are consumed.",44,802,632,40,18,Studio.Muted,TextAlignmentOptions.Center);Studio.Button(m,"USE 1 BUILDER MINUTE",44,850,632,65,()=>EmpireAction(()=>empire.SpendSpeedup(Guid.NewGuid().ToString("N"),p.Id,"CONSTRUCTION_SPEEDUP_MIN",1,EmpireNow),EmpireQueue));}Studio.Button(m,"KEEP PLAYING · NO WAIT REQUIRED",44,1090,632,130,ContinueEmpirePuzzle);
  }
 }
 public sealed class EmpireCountdown:MonoBehaviour {
  public long FinishAt;public UnityEngine.UI.Button FinishButton;
  void Update(){long seconds=Math.Max(0,FinishAt-DateTimeOffset.UtcNow.ToUnixTimeSeconds());GetComponent<TextMeshProUGUI>().text=seconds==0?"READY TO COMPLETE":seconds/60+"m "+seconds%60+"s";if(FinishButton)FinishButton.interactable=seconds==0;}
 }
 public sealed class EmpireCollectState:MonoBehaviour {
  public Func<long> Read;public Func<bool> CanSave;long last=-1;float next;
  void Update(){if(Read==null||Time.unscaledTime<next)return;next=Time.unscaledTime+.25f;long amount=Read();var button=GetComponent<UnityEngine.UI.Button>();button.interactable=(CanSave?.Invoke()??false)&&amount>0;if(amount==last)return;last=amount;GetComponentInChildren<TextMeshProUGUI>().text="COLLECT OUTPUT · "+amount;}
 }
}
