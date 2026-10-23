using System;
using System.Linq;
using CashflowChains.Domain;
using NUnit.Framework;

public sealed class TravelRulesTests {
 static Board Shop(int distance){var b=new Board{Width=7,Height=3,Budget=1000,RulesVersion=1};b.Nodes.Add(new Node(1,"home",0,1));b.Nodes.Add(new Node(2,"cafe",distance,1){InitialGoods=48});return b;}
 [Test] public void DistantResidentsStillBuyButArriveLaterAndEarnLess(){var near=Resolver.Run(Shop(1));var far=Resolver.Run(Shop(5));Assert.Greater(far.Credits,0);Assert.Greater(near.Credits,far.Credits);Assert.AreEqual(24,far.Beats);Assert.AreEqual(6,far.Events.First(e=>e.Reason=="sale").Beat);Assert.True(far.Conserved());}
 [Test] public void RotatingAHouseOrCafeDoesNotRemoveACityRoad(){var b=Shop(4);string before=Resolver.Run(b).Trace();b.Nodes[0].Rotation=3;b.Nodes[1].Rotation=2;Assert.AreEqual(before,Resolver.Run(b).Trace());}
 [Test] public void FarmHarvestsWithoutASecretEnergyTokenAndGoodsActuallyTravel(){var b=Shop(1);b.Nodes[1].InitialGoods=0;b.Nodes.Add(new Node(3,"farm",4,1));var r=Resolver.Run(b);Assert.Greater(r.Credits,0);Assert.AreEqual(0,r.Consumed[2]);var trip=r.Events.First(e=>e.Reason=="shipment"&&e.Source==3);Assert.AreEqual(3,trip.TravelSteps);Assert.AreEqual(trip.Beat+3,trip.ArrivalBeat);Assert.True(r.Events.Any(e=>e.Reason=="arrival"&&e.Source==3&&e.Beat==trip.ArrivalBeat));Assert.True(r.Conserved());}
 [Test] public void ChoosingADestinationHasARealOpportunityCostAndCannotRouteToAnIncompatibleBuilding(){var b=Shop(1);b.Nodes.Add(new Node(3,"diner",5,1){InitialGoods=48});var run=new CityRun(0);Assert.False(run.SetDestination(1,1));b.Nodes[0].TargetId=3;var r=Resolver.Run(b);Assert.True(r.Events.Where(e=>e.Reason=="shipment"&&e.Source==1).All(e=>e.Target==3));Assert.False(r.Events.Any(e=>e.Reason=="sale"&&e.Source==2));b.Nodes[0].TargetId=99;Assert.Throws<ArgumentException>(()=>b.Validate());}
 [Test] public void RuleVersionsAndDestinationsAreBoundIntoCopiesAndProofFingerprints(){var b=Shop(2);var copy=b.Copy();Assert.AreEqual(1,copy.RulesVersion);copy.Nodes[0].TargetId=2;Assert.AreNotEqual(b.Fingerprint(),copy.Fingerprint());Assert.AreEqual(2,copy.Copy().Nodes[0].TargetId);b.RulesVersion=2;Assert.Throws<ArgumentException>(()=>b.Validate());}
 [Test] public void ClosingTimeRetainsPendingShipmentsWithoutInventingSales(){var b=Shop(6);var r=Resolver.Run(b);Assert.Greater(r.InTransit[0],0);Assert.True(r.Conserved());Assert.AreEqual(r.Credits,r.Events.Sum(e=>e.Credits));Assert.True(r.Events.Where(e=>e.Reason=="sale").All(e=>e.Beat<=24));}
 [Test] public void AllVersionedCampaignWitnessesRemainLegalConservedAndSuccessful(){for(int i=0;i<104;i++){var m=Campaign.Get(i,1);var run=new CityRun(i,m.Solution.Copy());var r=run.Preview();Assert.True(r.Conserved(),"city "+i);Assert.Greater(r.Credits,0,"city "+i);Assert.AreEqual(3,run.Stars(r.Credits),"city "+i);Assert.AreEqual(1,run.Board.RulesVersion);}}
 [Test] public void LegacyProofsKeepTheirExactScoringAndVersionZeroFingerprint(){var m=Campaign.Get(0);Assert.AreEqual(0,m.Solution.RulesVersion);Assert.AreEqual(1120,Resolver.Run(m.Solution).Credits);Assert.False(m.Solution.Fingerprint().StartsWith("v1:"));}
}
