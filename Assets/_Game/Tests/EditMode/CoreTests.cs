using System;
using System.Linq;
using NUnit.Framework;
using CashflowChains.Domain;

public class CoreTests
{
    [Test] public void EveryLaboratoryBoardHasAFreeLegalWitness()
    {
        for(int i=0;i<3;i++)
        {
            var b=Board.Tutorial(i);b.Move(2,1,1);if(i>0)b.Move(3,1,2);
            if(i==2){b.Nodes.Add(new Node(4,"home",3,1));b.Nodes.Add(new Node(5,"cafe",4,1));b.Nodes.Add(new Node(6,"farm",4,2,3){InitialEnergy=1});}
            b.Validate();Assert.GreaterOrEqual(Resolver.Run(b).Credits,i==0?480:i==1?800:1200);
        }
    }
    [Test] public void TutorialHasVisibleFiniteSuppliesAndExactSevenSalesBeats()
    {
        var b = Board.Tutorial(0); Assert.True(b.Move(2,1,1));
        var r = Resolver.Run(b);
        Assert.AreEqual(1120, r.Credits); Assert.AreEqual(8, r.Beats);
        Assert.AreEqual(14,r.Consumed[0]); Assert.AreEqual(14,r.Consumed[1]);
        Assert.True(r.Conserved()); Assert.AreEqual(7,r.Events.Count(e=>e.Credits>0));
    }
    [Test] public void MissingGoodsMeansNoSales()
    {
        var b=Board.Tutorial(0); b.Nodes[1].InitialGoods=0; b.Move(2,1,1);
        Assert.AreEqual(0,Resolver.Run(b).Credits);
    }
    [Test] public void RotationImmediatelyBreaksPathAndUndoRestoresIt()
    {
        var s=new Session(); s.Move(2,1,1); var score=s.Preview().Credits;
        s.Rotate(1); Assert.AreEqual(0,s.Preview().Credits);
        Assert.True(s.Undo()); Assert.AreEqual(score,s.Preview().Credits);
    }
    [Test] public void InvalidMoveAndPreviewNeverMutateBoard()
    {
        var b=Board.Tutorial(0); var before=b.Fingerprint();
        Assert.False(b.Move(1,2,1)); Assert.False(b.Move(1,-1,0)); Resolver.Run(b);
        Assert.AreEqual(before,b.Fingerprint());
    }
    [Test] public void BanksApplyOnceEvenWhenSeveralTouchOutlet()
    {
        var b=Board.Tutorial(0); b.Budget=1000;b.Move(2,1,1);
        b.Nodes.Add(new Node(3,"bank",1,0)); b.Nodes.Add(new Node(4,"bank",1,2));
        Assert.AreEqual(1400,Resolver.Run(b).Credits);
    }
    [Test] public void WarehouseCannotCreateGoodsInCycle()
    {
        var b=new Board { Width=4, Height=4, Budget=10000 };
        b.Nodes.Add(new Node(1,"warehouse",1,1));b.Nodes.Add(new Node(2,"warehouse",2,1,2));
        var r=Resolver.Run(b); Assert.AreEqual(0,r.Credits);
        Assert.AreEqual(0,r.Generated.Sum());Assert.True(r.Conserved());
    }
    [Test] public void StockTraversesARealFourNodeCycleWithoutMultiplication()
    {
        var b=new Board{Width=4,Height=4,Budget=1000};
        b.Nodes.Add(new Node(1,"warehouse",1,1,3){InitialGoods=5});b.Nodes.Add(new Node(2,"warehouse",2,1,0));
        b.Nodes.Add(new Node(3,"warehouse",2,2,1));b.Nodes.Add(new Node(4,"warehouse",1,2,2));
        var r=Resolver.Run(b);Assert.AreEqual(5,r.Remaining[1]);Assert.AreEqual(0,r.Credits);
        Assert.AreEqual(4,r.Events.Count(e=>e.Reason=="shipment"));Assert.True(r.Conserved());
    }
    [Test] public void StableNodeIdentitySurvivesContainerOrderChanges()
    {
        var b=Board.Tutorial(1);b.Move(2,1,1);b.Move(3,1,2);
        var before=Resolver.Run(b).Trace();b.Nodes.Reverse();Assert.AreEqual(before,Resolver.Run(b).Trace());
    }
    [Test] public void OneBankOnlyAmplifiesOneOutlet()
    {
        var b=Board.Tutorial(0);b.Budget=10000;b.Move(2,1,1);
        b.Nodes.Add(new Node(3,"bank",1,2));b.Nodes.Add(new Node(4,"cafe",2,2,2){InitialGoods=14});b.Nodes.Add(new Node(5,"home",3,2,2));
        Assert.AreEqual(2520,Resolver.Run(b).Credits);
    }
    [Test] public void ExactlyOnceActivationAndFirstClearReward()
    {
        var s=new Session();s.Move(2,1,1);
        Assert.True(s.Activate("first"));Assert.False(s.Activate("second"));
        Assert.AreEqual(500,s.Snapshot.Wallet);
        s.Edit();Assert.True(s.Activate("third"));Assert.AreEqual(500,s.Snapshot.Wallet);
    }
    [Test] public void ResumeRestoresBoardAndRewardIdentity()
    {
        var s=new Session();s.Move(2,1,1);s.Activate("first");
        var copy=Session.Restore(s.Export());Assert.AreEqual(s.Export().Board.Fingerprint(),copy.Export().Board.Fingerprint());
        Assert.False(copy.Activate("first"));Assert.AreEqual(500,copy.Snapshot.Wallet);
    }
    [Test] public void RejectInvalidDefinitionsBudgetsAndMultipliers()
    {
        var b=Board.Tutorial(0);b.Nodes[0].Kind="unknown";
        Assert.Throws<ArgumentException>(()=>Resolver.Run(b));
        b=Board.Tutorial(0);b.Nodes[0].InitialGoods=int.MaxValue;
        Assert.Throws<ArgumentException>(()=>Resolver.Run(b));
        Assert.Throws<ArgumentOutOfRangeException>(()=>Resolver.Run(Board.Tutorial(0),80001));
        var s=new Session();Assert.False(s.Place("bank",0,0));
    }
    [Test] public void FactoryConversionRecordsBothConsumptionAndProduction()
    {
        var b=new Board {Width=4,Height=4,Budget=10000};
        b.Nodes.Add(new Node(1,"factory",1,1){InitialGoods=2,InitialEnergy=2});
        var r=Resolver.Run(b);Assert.AreEqual(2,r.Consumed[1]);Assert.AreEqual(6,r.Generated[1]);Assert.True(r.Conserved());
    }
    [Test] public void TenThousandSeededBoardsConserveAndReplay()
    {
        var rng=new Random(104729);string[] kinds={"home","cafe","farm","warehouse","factory","power","bank","grocer"};
        for(int t=0;t<10000;t++)
        {
            var b=new Board{Width=4,Height=4,Budget=100000};
            for(int cell=0;cell<16;cell++) if(rng.Next(3)==0)
                b.Nodes.Add(new Node(cell+1,kinds[rng.Next(kinds.Length)],cell%4,cell/4,rng.Next(4)) {InitialGoods=rng.Next(5),InitialEnergy=rng.Next(3)});
            var r=Resolver.Run(b);var replay=Resolver.Run(b);
            Assert.True(r.Conserved(),"seed case "+t);Assert.GreaterOrEqual(r.Credits,0);
            Assert.AreEqual(r.Trace(),replay.Trace(),"seed case "+t);
            // Independent ledger arithmetic, not just the engine's convenience assertion.
            for(int k=0;k<3;k++) Assert.AreEqual(r.Initial[k]+r.Generated[k],r.Consumed[k]+r.Spilled[k]+r.Remaining[k]);
            Assert.LessOrEqual(r.Events.Count,10000);
        }
    }
}
