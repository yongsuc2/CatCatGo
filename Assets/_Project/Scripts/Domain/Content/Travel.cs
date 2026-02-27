using System;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Domain.Content
{
    public class Travel
    {
        private const int BaseGoldPerChapter = 50;
        private static readonly int[] AvailableMultipliers = { 3, 5, 10, 20, 50 };

        public int MaxClearedChapter;
        public int Multiplier;

        public Travel(int maxClearedChapter = 1)
        {
            MaxClearedChapter = maxClearedChapter;
            Multiplier = 3;
        }

        public Result SetMultiplier(int mult)
        {
            if (!AvailableMultipliers.Contains(mult))
                return Result.Fail("Invalid multiplier");

            Multiplier = mult;
            return Result.Ok();
        }

        public int[] GetAvailableMultipliers()
        {
            return AvailableMultipliers;
        }

        public Result<TravelRunResult> Run(int staminaToSpend, int availableStamina)
        {
            if (availableStamina < staminaToSpend || staminaToSpend <= 0)
                return Result.Fail<TravelRunResult>("Not enough stamina");

            int goldEarned = CalculateGold(staminaToSpend);
            var reward = Reward.FromResources(
                new ResourceReward(ResourceType.GOLD, goldEarned));

            return Result.Ok(new TravelRunResult { Reward = reward, StaminaSpent = staminaToSpend });
        }

        public int CalculateGold(int stamina)
        {
            return (int)Math.Floor((double)stamina * BaseGoldPerChapter * MaxClearedChapter * Multiplier / 100);
        }

        public int GetGoldPreview(int stamina)
        {
            return CalculateGold(stamina);
        }
    }

    public class TravelRunResult
    {
        public Reward Reward;
        public int StaminaSpent;
    }
}
