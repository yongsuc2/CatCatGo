using System;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Content;

namespace CatCatGo.Domain.Meta
{
    public class OfflineRewardCalculator
    {
        private const int MaxOfflineHoursValue = 12;
        private const int StaminaPerHour = 60;

        public Reward Calculate(long lastOnlineTimestamp, Travel travel)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long elapsedMs = now - lastOnlineTimestamp;
            double elapsedHours = Math.Min((double)elapsedMs / (1000 * 60 * 60), MaxOfflineHoursValue);

            if (elapsedHours < 0.05)
                return Reward.Empty();

            int staminaUsed = (int)Math.Floor(elapsedHours * StaminaPerHour);
            int goldEarned = travel.CalculateGold(staminaUsed);

            return Reward.FromResources(
                new ResourceReward(ResourceType.GOLD, goldEarned),
                new ResourceReward(ResourceType.STAMINA, (int)Math.Floor(elapsedHours * 10)));
        }

        public int GetMaxOfflineHours()
        {
            return MaxOfflineHoursValue;
        }

        public Reward PreviewReward(float hours, Travel travel)
        {
            double clamped = Math.Min(hours, MaxOfflineHoursValue);
            int staminaUsed = (int)Math.Floor(clamped * StaminaPerHour);
            int goldEarned = travel.CalculateGold(staminaUsed);

            return Reward.FromResources(
                new ResourceReward(ResourceType.GOLD, goldEarned));
        }
    }
}
