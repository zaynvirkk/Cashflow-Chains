using System;
using System.Linq;
using CashflowChains.Economy;

namespace CashflowChains.Empire
{
    public enum DevelopmentAction { Unlock, TierLimit, BuilderBusy, Upgrade, FindResources }

    public sealed class DevelopmentRequirement
    {
        public string ItemId;
        public long Required, Owned, Missing;
    }

    public sealed class DevelopmentQuote
    {
        public string BusinessId, Output, ProjectId;
        public int Level, NextLevel, Clears, UnlockClears;
        public long Seconds, Storage;
        public decimal BeforeHourly, AfterHourly;
        public bool Affordable;
        public DevelopmentAction Action;
        public DevelopmentRequirement[] Requirements;
    }

    // A chosen goal is a preference, never proof of achievement or an award source.
    [Serializable]
    public sealed class DevelopmentGoal
    {
        public string BusinessId;
        public int TargetLevel;

        public void Validate(BalanceConfig config)
        {
            config.Business(BusinessId);
            config.Upgrade(TargetLevel);
        }

        public bool Reached(EmpireAccount account) =>
            account.Businesses.Any(b => b.Id == BusinessId && b.Level >= TargetLevel);

        public DevelopmentGoal Copy() => new DevelopmentGoal { BusinessId = BusinessId, TargetLevel = TargetLevel };
    }

    public static class DevelopmentPlanning
    {
        public static DevelopmentQuote Inspect(BalanceConfig config, EmpireAccount account, string businessId)
        {
            var definition = config.Business(businessId);
            var business = account.Businesses.FirstOrDefault(b => b.Id == businessId);
            int level = business?.Level ?? 0;
            var upgrade = config.Upgrades.FirstOrDefault(u => u.Level == level + 1);
            int clears = account.Economy.Receipts.Count(r => r.CommandId.StartsWith("certified-first-clear:"));
            var requirements = upgrade == null ? new DevelopmentRequirement[0] : upgrade.Costs.Select(cost =>
            {
                long owned = account.Economy.Amount(cost.ItemId);
                return new DevelopmentRequirement
                {
                    ItemId = cost.ItemId, Required = cost.Quantity, Owned = owned,
                    Missing = Math.Max(0, cost.Quantity - owned)
                };
            }).ToArray();
            bool affordable = upgrade != null && requirements.All(r => r.Missing == 0);
            var action = clears < definition.UnlockClears ? DevelopmentAction.Unlock
                : upgrade == null ? DevelopmentAction.TierLimit
                : account.Construction.Count >= account.Builders ? DevelopmentAction.BuilderBusy
                : affordable ? DevelopmentAction.Upgrade : DevelopmentAction.FindResources;
            int production = EmpireBenefits.Bonus(config, account, "production-bp");
            return new DevelopmentQuote
            {
                BusinessId = businessId, Output = definition.Output, Level = level,
                NextLevel = upgrade?.Level ?? 0, Clears = clears, UnlockClears = definition.UnlockClears,
                ProjectId = account.Construction.FirstOrDefault(p => p.BusinessId == businessId)?.Id,
                Seconds = upgrade == null ? 0 : EconomyMath.Duration(upgrade.Seconds, EmpireBenefits.Bonus(config, account, "build-time-bp")),
                BeforeHourly = ProductionPolicy.ScaledHourly(config, businessId, level, production) / 10000m,
                AfterHourly = ProductionPolicy.ScaledHourly(config, businessId, upgrade?.Level ?? level, production) / 10000m,
                Storage = EconomyMath.WithBonus(definition.Storage, EmpireBenefits.Bonus(config, account, "storage-bp"), 10000),
                Affordable = affordable, Action = action, Requirements = requirements
            };
        }

        public static DevelopmentGoal Choose(BalanceConfig config, EmpireAccount account, string businessId)
        {
            var quote = Inspect(config, account, businessId);
            if (quote.NextLevel == 0) throw new InvalidOperationException("This business has reached its authored tier limit");
            return new DevelopmentGoal { BusinessId = businessId, TargetLevel = quote.NextLevel };
        }
    }
}
