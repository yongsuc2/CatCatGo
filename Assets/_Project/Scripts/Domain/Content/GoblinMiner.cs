using System;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Content
{
    public class GoblinMiner
    {
        private const int OrePerMine = 1;
        private const int CartThreshold = 30;

        public int OreCount;

        public GoblinMiner(int oreCount = 0)
        {
            OreCount = oreCount;
        }

        public Result<MineResult> Mine(int pickaxeCount)
        {
            if (pickaxeCount < 1)
                return Result.Fail<MineResult>("No pickaxes");

            OreCount += OrePerMine;
            return Result.Ok(new MineResult { OreGained = OrePerMine });
        }

        public bool CanUseCart()
        {
            return OreCount >= CartThreshold;
        }

        public Result<CartResult> UseCart(SeededRandom rng)
        {
            if (!CanUseCart())
                return Result.Fail<CartResult>($"Need {CartThreshold} ore (have {OreCount})");

            OreCount -= CartThreshold;

            int goldReward = rng.NextInt(200, 500);
            int stoneReward = rng.NextInt(1, 3);
            var reward = Reward.FromResources(
                new ResourceReward(ResourceType.GOLD, goldReward),
                new ResourceReward(ResourceType.EQUIPMENT_STONE, stoneReward));

            return Result.Ok(new CartResult { Reward = reward });
        }

        public float GetProgress()
        {
            return Math.Min(1f, (float)OreCount / CartThreshold);
        }

        public int GetOreNeeded()
        {
            return Math.Max(0, CartThreshold - OreCount);
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
