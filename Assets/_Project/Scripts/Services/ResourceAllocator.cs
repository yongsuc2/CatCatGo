using System;
using System.Collections.Generic;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Entities;

namespace CatCatGo.Services
{
    public struct GoldAllocationPlan
    {
        public int AtkAmount;
        public int HpAmount;
        public int DefAmount;
        public int HeritageAmount;
    }

    public class ResourceAllocator
    {
        public GoldAllocationPlan AllocateGold(Player player)
        {
            float totalGold = player.Resources.Gold;
            var talent = player.Talent;

            int atkDeficit = Math.Max(0, talent.HpLevel - talent.AtkLevel);
            int hpDeficit = Math.Max(0, talent.AtkLevel - talent.HpLevel - 5);

            float atkRatio = 0.6f;
            float hpRatio = 0.3f;
            float defRatio = 0.05f;
            float heritageRatio = 0.05f;

            if (atkDeficit > 3)
            {
                atkRatio = 0.7f;
                hpRatio = 0.2f;
            }
            if (hpDeficit > 3)
            {
                hpRatio = 0.5f;
                atkRatio = 0.4f;
            }

            if (player.IsHeritageUnlocked())
            {
                heritageRatio = 0.1f;
                atkRatio -= 0.05f;
            }

            return new GoldAllocationPlan
            {
                AtkAmount = (int)Math.Floor(totalGold * atkRatio),
                HpAmount = (int)Math.Floor(totalGold * hpRatio),
                DefAmount = (int)Math.Floor(totalGold * defRatio),
                HeritageAmount = (int)Math.Floor(totalGold * heritageRatio),
            };
        }

        public bool ShouldSpendGems(Player player, string purpose)
        {
            float gems = player.Resources.Gems;
            int pityTarget = 180 * 298;

            switch (purpose)
            {
                case "gacha":
                    return gems >= 2980;
                case "arena_retry":
                    return gems > pityTarget && gems >= 50;
                case "stamina":
                    return false;
                default:
                    return false;
            }
        }

        public List<TalentAutoUpgradeResult> AutoUpgradeTalent(Player player)
        {
            var plan = AllocateGold(player);
            var results = new List<TalentAutoUpgradeResult>();

            var allocations = new (StatType stat, int budget)[]
            {
                (StatType.ATK, plan.AtkAmount),
                (StatType.HP, plan.HpAmount),
                (StatType.DEF, plan.DefAmount),
            };

            foreach (var (stat, budget) in allocations)
            {
                int spent = 0;
                while (true)
                {
                    int cost = player.Talent.GetUpgradeCost(stat);
                    if (spent + cost > budget) break;
                    if (player.Resources.Gold < cost) break;

                    var result = player.Talent.Upgrade(stat, (int)player.Resources.Gold);
                    if (result.IsFail()) break;

                    player.Resources.Spend(ResourceType.GOLD, result.Data.Cost);
                    spent += result.Data.Cost;
                    results.Add(new TalentAutoUpgradeResult { Upgraded = true, Stat = stat, Cost = result.Data.Cost });
                }
            }

            return results;
        }
    }

    public struct TalentAutoUpgradeResult
    {
        public bool Upgraded;
        public StatType Stat;
        public int Cost;
    }
}
