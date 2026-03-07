using System;
using CatCatGo.Domain.Data;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Content
{
    public class GoblinMiner
    {
        private static GoblinMinerConfig Cfg => DungeonDataTable.GoblinMiner;

        public int OreCount;

        public GoblinMiner(int oreCount = 0)
        {
            OreCount = oreCount;
        }

        public Result<MineResult> Mine(int pickaxeCount)
        {
            if (pickaxeCount < 1)
                return Result.Fail<MineResult>("No pickaxes");

            OreCount += Cfg.OrePerMine;
            return Result.Ok(new MineResult { OreGained = Cfg.OrePerMine });
        }

        public bool CanUseCart()
        {
            return OreCount >= Cfg.CartThreshold;
        }

        public Result<CartResult> UseCart(SeededRandom rng)
        {
            if (!CanUseCart())
                return Result.Fail<CartResult>($"Need {Cfg.CartThreshold} ore (have {OreCount})");

            OreCount -= Cfg.CartThreshold;

            var cr = Cfg.CartReward;
            int goldReward = rng.NextInt(cr.GoldMin, cr.GoldMax);
            int stoneReward = rng.NextInt(cr.StoneMin, cr.StoneMax);
            var reward = Reward.FromResources(
                new ResourceReward(ResourceType.GOLD, goldReward),
                new ResourceReward(ResourceType.EQUIPMENT_STONE, stoneReward));

            return Result.Ok(new CartResult { Reward = reward });
        }

        public float GetProgress()
        {
            return Math.Min(1f, (float)OreCount / Cfg.CartThreshold);
        }

        public int GetOreNeeded()
        {
            return Math.Max(0, Cfg.CartThreshold - OreCount);
        }
    }

    public class MineResult
    {
        public int OreGained;
    }

    public class CartResult
    {
        public Reward Reward;
    }
}
