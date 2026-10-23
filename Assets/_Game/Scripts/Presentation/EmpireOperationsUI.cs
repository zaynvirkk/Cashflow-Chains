using System;
using System.Linq;
using CashflowChains.Domain;
using CashflowChains.Economy;
using CashflowChains.Empire;
using LocalGame;
using TMPro;
using UnityEngine;

namespace CashflowChains.Presentation {
 public sealed partial class MobileGameApp {
  int dispatchPage,researchTree,researchPage;
  string ItemName(string id){
   if(id.StartsWith("BIZ_FRAG:"))return Pieces.Get(id.Substring(9)).Label+" FRAGMENTS";
   switch(id){
    case "CONSTRUCTION_SPEEDUP_MIN":return "BUILDER MINUTE";
    case "RESEARCH_SPEEDUP_MIN":return "RESEARCH MINUTE";
    case "DISPATCH_SPEEDUP_MIN":return "DELIVERY MINUTE";
    case "GENERAL_SPEEDUP_MIN":return "FLEXIBLE MINUTE";
    case "RECRUIT_TICKET":return "RECRUITMENT TICKET";
    case "RESEARCH_DOCS":return "RESEARCH DOCUMENTS";
    default:return id.Replace('_',' ');
   }
  }
  string BundleText(ItemAmount[] bundle)=>string.Join("\n",bundle.Select(x=>"+"+x.Quantity+" "+ItemName(x.ItemId)));
  string ResearchBenefit(ResearchDefinition n)=>n.BonusBp/100m+"% "+(n.Bonus=="production-bp"?"MORE PRODUCTION":n.Bonus=="storage-bp"?"MORE OUTPUT STORAGE":n.Bonus=="dispatch-time-bp"?"SHORTER DISPATCHES":n.Bonus=="dispatch-yield-bp"?"MORE DISPATCH REWARDS":n.Bonus=="build-time-bp"?"SHORTER CONSTRUCTION":"SHORTER RESEARCH");
  void EmpireDispatch(){
   if(!empireSandbox)return;Base("empire-dispatch");var a=empire.Snapshot;var active=a.Dispatches.Where(x=>!x.Claimed).ToArray();Studio.Header(root,"A CLEVER CHAIN CAN KEEP WORKING","Delivery routes",active.Length+" / "+EmpireBenefits.DispatchSlots(empireConfig,a)+" ACTIVE · "+a.Dispatches.Count(x=>x.Claimed)+" RETURNS",EmpireScreen);
   Studio.Label(root,message!=""?message:"Guaranteed rewards. Your saved layout is verified before each dispatch.",38,210,644,68,24,Studio.Text);
   if(!a.Businesses.Any(x=>x.Level>0)){Studio.Image(root,"warehouse",200,320,320,330);Studio.Label(root,"Complete your first permanent business to give your chains another job.",60,710,600,180,32,Studio.Text,TextAlignmentOptions.Center);Studio.Button(root,"BUILD MY FIRST BUSINESS",38,1070,644,110,EmpireScreen,true);}
   else if(active.Length==0){Studio.Image(root,"logistics",200,300,320,330);Studio.Label(root,"Your delivery fleet is ready.\nUse a successful city layout, pay its dispatch fee and earn the listed materials when it returns.",60,700,600,240,30,Studio.Text,TextAlignmentOptions.Center);Studio.Button(root,"CHOOSE A DELIVERY",38,1070,644,110,()=>EmpireDispatchCatalog(0),true);}
   else{
    for(int i=0;i<active.Length;i++){var d=active[i];var def=empireConfig.Operations.Dispatch(d.Template);float y=320+i*290;Studio.Box(root,"Delivery "+d.Id,38,y,644,265,FlatUI.Hex("#003747EE"));Studio.Image(root,def.RequiredRecipe,52,y+25,170,170);Studio.Label(root,def.Title.ToUpperInvariant(),235,y+24,430,70,27,Studio.Accent,heavy:true);var timer=Studio.Label(root,"",235,y+102,430,65,30,Studio.Text);timer.gameObject.AddComponent<EmpireCountdown>().FinishAt=d.FinishAt;Studio.Button(root,"VIEW RETURN",235,y+178,430,80,()=>EmpireDispatchDetail(d.Id),true);}
    Studio.Button(root,"CHOOSE ANOTHER DELIVERY",38,1070,644,110,()=>EmpireDispatchCatalog(0),true);
   }
   Studio.Button(root,"MY CITY",38,1198,310,80,EmpireScreen);Studio.Button(root,"RESEARCH",366,1198,316,80,()=>EmpireResearch(0,0));WriteEmpireProbe();
  }
  void EmpireDispatchCatalog(int page){dispatchPage=page;Base("dispatch-catalog");var a=empire.Snapshot;Studio.Header(root,"ALWAYS ANOTHER USEFUL JOB","Dispatch board",a.Economy.Amount("CASH")+" CASH · "+empireConfig.Operations.DailyDispatchStarts+" STARTS / UTC DAY",EmpireDispatch);var defs=empireConfig.Operations.Dispatches;int pages=(defs.Length+4)/5;
   for(int i=0;i<5;i++){int index=page*5+i;if(index>=defs.Length)break;var d=defs[index];float y=280+i*151;Studio.Image(root,d.RequiredRecipe,38,y,102,112);Studio.Label(root,d.Title.ToUpperInvariant(),152,y,360,46,25,Studio.Accent,heavy:true);Studio.Label(root,"Needs "+Pieces.Get(d.RequiredRecipe).Label+" · "+EmpireDuration(d.Seconds)+"\n"+string.Join(" · ",d.Costs.Select(x=>x.Quantity+" "+ItemName(x.ItemId))),152,y+48,360,80,23,Studio.Text);Studio.Button(root,"DETAILS",530,y+8,152,110,()=>EmpireDispatchOffer(d.Id));}
   Studio.Button(root,"‹",38,1060,100,82,()=>EmpireDispatchCatalog((page+pages-1)%pages));Studio.Label(root,(page+1)+" / "+pages,170,1070,380,60,27,Studio.Muted,TextAlignmentOptions.Center);Studio.Button(root,"›",582,1060,100,82,()=>EmpireDispatchCatalog((page+1)%pages));Studio.Button(root,"MY ACTIVE DELIVERIES",38,1175,644,85,EmpireDispatch,true);
  }
  Board DispatchPlan(string recipe){var receipts=empire.Snapshot.Economy.Receipts;var plans=data.CompletedPlans.Select(b=>b.Copy()).ToList();foreach(var d in empire.Snapshot.Dispatches)if(!plans.Any(b=>b.Fingerprint()==d.Layout.Fingerprint()))plans.Add(d.Layout.Copy());if(data.Profile.Stars[run.Board.Challenge]>0&&run.Stars(run.Preview().Credits)>0)plans.Add(run.Board.Copy());return VerifiedLayouts.Find(plans,recipe,level=>receipts.Any(r=>r.CommandId=="certified-first-clear:"+level));}
  void EmpireDispatchOffer(string id,Action returnTo=null){var d=empireConfig.Operations.Dispatch(id);var a=empire.Snapshot;var plan=DispatchPlan(d.RequiredRecipe);int clears=a.Economy.Receipts.Count(x=>x.CommandId.StartsWith("certified-first-clear:"));var m=Studio.Modal(root,d.Title.ToUpperInvariant(),returnTo??(()=>EmpireDispatchCatalog(dispatchPage)));Studio.Image(m,d.RequiredRecipe,250,180,220,210);Studio.Label(m,"DISPATCH COST\n"+string.Join(" · ",d.Costs.Select(x=>x.Quantity+" "+ItemName(x.ItemId)))+"\n\nRETURN TIME · "+EmpireDuration(EconomyMath.Duration(d.Seconds,EmpireBenefits.Bonus(empireConfig,a,"dispatch-time-bp"))),44,420,632,210,27,Studio.Text,TextAlignmentOptions.Center);
   var rewards=d.Rewards.Select(x=>new ItemAmount(x.ItemId,EconomyMath.WithBonus(x.Quantity,EmpireBenefits.Bonus(empireConfig,a,"dispatch-yield-bp"),10000))).ToArray();Studio.Label(m,"GUARANTEED RETURN\n"+BundleText(rewards),44,650,632,300,25,Studio.Accent,TextAlignmentOptions.Center,true);Studio.Label(m,clears<d.UnlockClears?"Unlocks at "+d.UnlockClears+" city wins. Current: "+clears:plan==null?"Finish a city with a contributing "+Pieces.Get(d.RequiredRecipe).Label+" to save a delivery plan.":"Uses your saved city "+(plan.Challenge+1)+" plan. Keep editing your next puzzle while this chain works.",44,970,632,100,23,Studio.Text,TextAlignmentOptions.Center);
   Studio.Button(m,clears<d.UnlockClears||plan==null?"PLAY A CITY TO PREPARE THIS ROUTE":"START DELIVERY",44,1100,632,120,()=>{if(clears<d.UnlockClears||plan==null){ContinueEmpirePuzzle();return;}EmpireAction(()=>empire.StartDispatch(Guid.NewGuid().ToString("N"),id,plan,EmpireNow),returnTo??EmpireDispatch);},true);
  }
  void EmpireDispatchDetail(string id,Action returnTo=null){var a=empire.Snapshot;var d=a.Dispatches.Find(x=>x.Id==id);if(d==null||d.Claimed){EmpireDispatch();return;}var def=empireConfig.Operations.Dispatch(d.Template);var m=Studio.Modal(root,def.Title.ToUpperInvariant(),returnTo??EmpireDispatch);Studio.Image(m,def.RequiredRecipe,240,200,240,230);Studio.Label(m,"YOUR COMMITTED RETURN\n"+BundleText(d.Rewards),44,460,632,350,28,Studio.Accent,TextAlignmentOptions.Center,true);var timer=Studio.Label(m,"",44,850,632,90,38,Studio.Text,TextAlignmentOptions.Center,true);var countdown=timer.gameObject.AddComponent<EmpireCountdown>();countdown.FinishAt=d.FinishAt;var claim=Studio.Button(m,"COLLECT THIS RETURN",44,1090,632,135,()=>EmpireAction(()=>empire.ClaimDispatch(id,EmpireNow),returnTo??EmpireDispatch),true);countdown.FinishButton=claim;
   int minutes=(int)a.Economy.Amount("DISPATCH_SPEEDUP_MIN");if(minutes>0)Studio.Button(m,"USE 1 DISPATCH MINUTE · "+minutes+" OWNED",44,970,632,80,()=>EmpireAction(()=>empire.SpendSpeedup(Guid.NewGuid().ToString("N"),id,"DISPATCH_SPEEDUP_MIN",1,EmpireNow),EmpireDispatch));
  }
  void EmpireResearch(int tree,int page){researchTree=tree;researchPage=page;Base("empire-research");var a=empire.Snapshot;int clears=a.Economy.Receipts.Count(x=>x.CommandId.StartsWith("certified-first-clear:")),claims=a.Dispatches.Count(x=>x.Claimed);Studio.Header(root,"PLAN A MORE CAPABLE CITY","Research laboratory",a.Research.Count+" / 30 COMPLETED · "+a.Economy.Amount("RESEARCH_DOCS")+" DOCUMENTS",EmpireScreen);
   if(clears<empireConfig.Operations.ResearchUnlockClears||claims<empireConfig.Operations.ResearchUnlockClaims){Studio.Image(root,"power",220,310,280,290);Studio.Label(root,"A REAL OBJECTIVE, NO PURCHASE REQUIRED\n\nCity wins  "+clears+" / "+empireConfig.Operations.ResearchUnlockClears+"\nReturned dispatches  "+claims+" / "+empireConfig.Operations.ResearchUnlockClaims+"\n\nYour deliveries bring back research documents. Keep solving and dispatching while construction works.",44,660,632,360,29,Studio.Text,TextAlignmentOptions.Center);Studio.Button(root,"GO TO MY DELIVERY ROUTES",38,1100,644,100,EmpireDispatch,true);return;}
   string[] trees={"Operations","Logistics","Innovation"};for(int i=0;i<3;i++){int choice=i;Studio.Button(root,trees[i].ToUpperInvariant(),38+i*218,225,208,80,()=>EmpireResearch(choice,0),tree==i);}
   var nodes=empireConfig.Operations.Research.Where(x=>x.Tree==trees[tree]).ToArray();for(int i=0;i<5;i++){int index=page*5+i;if(index>=nodes.Length)break;var n=nodes[index];bool done=a.Research.Contains(n.Id),running=a.ResearchQueue.Any(x=>x.NodeId==n.Id);float y=330+i*143;Studio.Label(root,(done?"✓ ":"")+n.Title.ToUpperInvariant(),38,y,445,55,25,done?Studio.Accent:Studio.Text,heavy:true);Studio.Label(root,running?"RESEARCH RUNNING":done?"BONUS ACTIVE":n.BonusBp/100m+"% · "+EmpireDuration(EconomyMath.Duration(n.Seconds,EmpireBenefits.Bonus(empireConfig,a,"research-time-bp")))+(string.IsNullOrEmpty(n.Prerequisite)?"":" · NEEDS "+n.Prerequisite.ToUpperInvariant()),38,y+60,445,64,22,Studio.Muted);Studio.Button(root,running?"VIEW":"DETAILS",510,y+6,172,110,()=>EmpireResearchDetail(n.Id));}
   Studio.Button(root,"‹",38,1080,110,80,()=>EmpireResearch(tree,1-page));Studio.Label(root,(page+1)+" / 2",185,1095,350,55,25,Studio.Muted,TextAlignmentOptions.Center);Studio.Button(root,"›",572,1080,110,80,()=>EmpireResearch(tree,1-page));Studio.Button(root,"MY CITY",38,1198,310,80,EmpireScreen);Studio.Button(root,"DELIVERIES",366,1198,316,80,EmpireDispatch);WriteEmpireProbe();
  }
  void EmpireResearchDetail(string node){var a=empire.Snapshot;var n=empireConfig.Operations.Node(node);var project=a.ResearchQueue.Find(x=>x.NodeId==node);var m=Studio.Modal(root,n.Title.ToUpperInvariant(),()=>EmpireResearch(researchTree,researchPage));Studio.Image(m,n.Tree=="Operations"?"warehouse":n.Tree=="Logistics"?"logistics":"power",240,210,240,230);Studio.Label(m,ResearchBenefit(n)+"\nBonuses add within their published caps.\nCertified puzzle scoring stays unchanged.",44,470,632,200,29,Studio.Accent,TextAlignmentOptions.Center,true);
   if(project!=null){var timer=Studio.Label(m,"",44,720,632,120,42,Studio.Text,TextAlignmentOptions.Center,true);var live=timer.gameObject.AddComponent<EmpireCountdown>();live.FinishAt=project.FinishAt;live.FinishButton=Studio.Button(m,"COMPLETE RESEARCH",44,1090,632,135,()=>EmpireAction(()=>empire.FinishResearch(project.Id,EmpireNow),()=>EmpireResearch(researchTree,researchPage)),true);}
   else{Studio.Label(m,a.Research.Contains(node)?"COMPLETED · THIS BENEFIT IS ACTIVE":"RESEARCH COST\n"+string.Join(" · ",n.Costs.Select(x=>x.Quantity+" "+ItemName(x.ItemId)))+"\n\n"+(string.IsNullOrEmpty(n.Prerequisite)?"An entry research branch.":"Requires "+n.Prerequisite.ToUpperInvariant())+"\nUnlocks at "+n.UnlockClears+" city wins.\nDocuments come from renewable deliveries.",44,720,632,310,27,Studio.Text,TextAlignmentOptions.Center);Studio.Button(m,a.Research.Contains(node)?"BACK TO MY RESEARCH":"START RESEARCH",44,1090,632,135,()=>{if(a.Research.Contains(node)){EmpireResearch(researchTree,researchPage);return;}EmpireAction(()=>empire.StartResearch(Guid.NewGuid().ToString("N"),node,EmpireNow),()=>EmpireResearch(researchTree,researchPage));},true);}
  }
 }
}
