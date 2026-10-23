using System;
using System.IO;
using System.Linq;
using CashflowChains.Domain;
using CashflowChains.Economy;
using CashflowChains.Empire;
using NUnit.Framework;
using UnityEngine;

public sealed class DevelopmentPlanningTests
{
    BalanceConfig config;

    [SetUp]
    public void Setup()
    {
        config = JsonUtility.FromJson<BalanceConfig>(File.ReadAllText(Path.Combine(Application.dataPath, "_Game/Data/CC2/balance-v1.json")));
        config.Validate();
    }

    EmpireEngine Earned(int clears = 3)
    {
        var engine = new EmpireEngine(config, new EmpireAccount { Economy = new EconomyState { AccountId = "planning-test" } }, _ => true);
        for (int level = 0; level < clears; level++) engine.RecordCertified(level, Campaign.Get(level).Solution, 100 + level);
        return engine;
    }

    [Test]
    public void AffordableDevelopmentRecommendsTheActualUpgradeAndDoesNotSpendAnything()
    {
        var engine = Earned();
        string before = JsonUtility.ToJson(engine.Snapshot);
        var quote = DevelopmentPlanning.Inspect(config, engine.Snapshot, "home");
        Assert.AreEqual(DevelopmentAction.Upgrade, quote.Action);
        Assert.AreEqual(0m, quote.BeforeHourly);
        Assert.AreEqual(8m, quote.AfterHourly);
        Assert.AreEqual(30, quote.Seconds);
        Assert.IsTrue(quote.Requirements.All(r => r.Missing == 0));
        Assert.AreEqual(before, JsonUtility.ToJson(engine.Snapshot));
    }

    [Test]
    public void LockedAndBusyProjectsExplainTheirRealGate()
    {
        var engine = Earned();
        Assert.AreEqual(DevelopmentAction.Unlock, DevelopmentPlanning.Inspect(config, engine.Snapshot, "cafe").Action);
        engine.StartConstruction("first-project", "home", 200);
        var quote = DevelopmentPlanning.Inspect(config, engine.Snapshot, "home");
        Assert.AreEqual(DevelopmentAction.BuilderBusy, quote.Action);
        Assert.AreEqual("first-project", quote.ProjectId);
    }

    [Test]
    public void AShortageReportsOwnedRequiredAndMissingForEveryMaterial()
    {
        var engine = Earned();
        engine.StartConstruction("build", "home", 200);
        engine.FinishConstruction("finish", "build", 230);
        var quote = DevelopmentPlanning.Inspect(config, engine.Snapshot, "home");
        Assert.AreEqual(DevelopmentAction.FindResources, quote.Action);
        var steel = quote.Requirements.Single(r => r.ItemId == "STEEL");
        Assert.AreEqual(0, steel.Owned);
        Assert.AreEqual(5, steel.Required);
        Assert.AreEqual(5, steel.Missing);
        Assert.AreEqual(8m, quote.BeforeHourly);
        Assert.AreEqual(8.64m, quote.AfterHourly);
    }

    [Test]
    public void EditingAQuoteCannotModifyTheRealCostOrGrantResources()
    {
        var engine = Earned();
        var quote = DevelopmentPlanning.Inspect(config, engine.Snapshot, "home");
        quote.Requirements[0].Required = 0;
        Assert.AreEqual(100, config.Upgrade(1).Costs.Single(r => r.ItemId == "CASH").Quantity);
        Assert.AreEqual(280, engine.Snapshot.Economy.Amount("CASH"));
        engine.StartConstruction("still-real-cost", "home", 200);
        Assert.AreEqual(180, engine.Snapshot.Economy.Amount("CASH"));
    }

    [Test]
    public void ChoosingAndChangingAGoalDoesNotChangeAnyEconomicState()
    {
        var engine = Earned();
        string before = JsonUtility.ToJson(engine.Snapshot);
        var goal = DevelopmentPlanning.Choose(config, engine.Snapshot, "cafe");
        goal.Validate(config);
        Assert.IsFalse(goal.Reached(engine.Snapshot));
        Assert.AreEqual(1, goal.TargetLevel);
        goal = DevelopmentPlanning.Choose(config, engine.Snapshot, "home");
        Assert.AreEqual("home", goal.BusinessId);
        Assert.AreEqual(before, JsonUtility.ToJson(engine.Snapshot));
    }

    [Test]
    public void AGoalCompletesOnlyAfterActualConstructionAndSurvivesSerialization()
    {
        var engine = Earned();
        var goal = DevelopmentPlanning.Choose(config, engine.Snapshot, "home");
        goal = JsonUtility.FromJson<DevelopmentGoal>(JsonUtility.ToJson(goal));
        engine.StartConstruction("build", "home", 200);
        Assert.IsFalse(goal.Reached(engine.Snapshot));
        engine.FinishConstruction("finish", "build", 230);
        Assert.IsTrue(goal.Reached(engine.Snapshot));
        Assert.AreEqual(2, DevelopmentPlanning.Choose(config, engine.Snapshot, "home").TargetLevel);
    }

    [Test]
    public void UnknownOrUnauthoredGoalsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new DevelopmentGoal { BusinessId = "unknown", TargetLevel = 1 }.Validate(config));
        Assert.Throws<ArgumentException>(() => new DevelopmentGoal { BusinessId = "home", TargetLevel = 0 }.Validate(config));
        Assert.Throws<ArgumentException>(() => new DevelopmentGoal { BusinessId = "home", TargetLevel = 50 }.Validate(config));
    }

    [Test]
    public void DisplayedRateUsesTheSameCappedFormulaAsRealAccrual()
    {
        foreach (int level in Enumerable.Range(1, 5))
        foreach (int bonus in new[] { 0, 500, 10000 })
        {
            var business = new OwnedBusiness { Id = "home", Level = level, ProducedAt = 100 };
            var actual = ProductionPolicy.Quote(config, business, 3700, bonus);
            decimal displayed = ProductionPolicy.ScaledHourly(config, "home", level, bonus) / 10000m;
            Assert.AreEqual(displayed, actual.Amount + actual.Carry / 36000000m);
        }
    }
}
