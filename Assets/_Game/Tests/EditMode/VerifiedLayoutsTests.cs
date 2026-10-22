using System;
using System.Collections.Generic;
using NUnit.Framework;
using CashflowChains.Domain;
using LocalGame.Data;

public sealed class VerifiedLayoutsTests {
 Profile Win(int level=0){var p=new Profile();for(int i=0;i<=level;i++){var m=Campaign.Get(i);p.Award(i,3,m.Par,"won-"+i);}return p;}
 [Test] public void SavedWinningPlanSurvivesMovingToAnUnsolvedNextCity(){var p=Win(2);var plans=new List<Board>();VerifiedLayouts.Record(plans,p,Campaign.Get(2).Solution);var next=new CityRun(3);Assert.AreEqual(0,next.Preview().Credits);var picked=VerifiedLayouts.Find(plans,"home",level=>level<3);Assert.AreEqual(2,picked.Challenge);Assert.Greater(Resolver.Run(picked,Campaign.Get(2).Multiplier).Credits,0);}
 [Test] public void UnwonOrNonProducingPlansAreRejectedWithoutReplacingExistingPlan(){var p=Win();var plans=new List<Board>();VerifiedLayouts.Record(plans,p,Campaign.Get(0).Solution);string saved=plans[0].Fingerprint();Assert.Throws<ArgumentException>(()=>VerifiedLayouts.Record(plans,p,Campaign.Get(1).Solution));Assert.Throws<ArgumentException>(()=>VerifiedLayouts.Record(plans,p,Campaign.Get(0).Start));Assert.AreEqual(saved,plans[0].Fingerprint());}
 [Test] public void SavingAndReturningAPlanCannotAlterItOrAwardCurrency(){var p=Win();long coins=p.Credits;var plans=new List<Board>();var original=Campaign.Get(0).Solution;VerifiedLayouts.Record(plans,p,original);original.Nodes.Clear();var picked=VerifiedLayouts.Find(plans,"cafe",_=>true);picked.Nodes.Clear();Assert.AreEqual(2,plans[0].Nodes.Count);Assert.AreEqual(coins,p.Credits);Assert.IsNull(VerifiedLayouts.Find(plans,"factory",_=>true));Assert.IsNull(VerifiedLayouts.Find(plans,"cafe",_=>false));}
 [Test] public void AReplayReplacesOnlyItsOwnPlanAndCannotForgeABetterRecordedResult(){var p=Win(1);var plans=new List<Board>();VerifiedLayouts.Record(plans,p,Campaign.Get(0).Solution);VerifiedLayouts.Record(plans,p,Campaign.Get(1).Solution);VerifiedLayouts.Record(plans,p,Campaign.Get(0).Solution);Assert.AreEqual(2,plans.Count);p.Best[0]=0;Assert.Throws<ArgumentException>(()=>VerifiedLayouts.Validate(plans,p));}
}
