using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;

namespace CatCatGo.Services
{
    public class PetManager
    {
        public Stats GetTotalPassiveBonus(List<Pet> pets)
        {
            float rate = PetTable.InactiveBonusRate;
            var total = Stats.Zero;
            foreach (var pet in pets)
            {
                var bonus = Stats.Create(
                    atk: (int)Math.Floor(pet.GetGlobalBonus().Atk * rate),
                    maxHp: (int)Math.Floor(pet.GetGlobalBonus().MaxHp * rate));
                total = total.Add(bonus);
            }
            return total;
        }

        public Pet HatchEgg(SeededRandom rng)
        {
            var template = PetTable.GetRandomTemplate(rng);
            return new Pet(
                $"pet_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{rng.NextInt(0, 9999)}",
                template.Name,
                template.Tier,
                PetGrade.COMMON,
                template.MaxGrade,
                1,
                template.BasePassiveBonus);
        }

        public Result<FeedResult> FeedPet(Pet pet, int foodAmount)
        {
            return pet.Feed(foodAmount);
        }

        public Result<UpgradeGradeResult> TryUpgradeGrade(Pet pet, List<Pet> duplicates)
        {
            var samePets = duplicates.Where(d => d.Name == pet.Name && d != pet).ToList();
            if (samePets.Count == 0)
                return Result.Fail<UpgradeGradeResult>("No duplicate pets available");

            return pet.UpgradeGrade();
        }

        public Pet SelectBestPet(Player player)
        {
            if (player.OwnedPets.Count == 0) return null;

            var tierOrder = new Dictionary<PetTier, int>
            {
                { PetTier.S, 3 },
                { PetTier.A, 2 },
                { PetTier.B, 1 },
            };

            var sorted = player.OwnedPets.OrderByDescending(p =>
            {
                tierOrder.TryGetValue(p.Tier, out int tierVal);
                return tierVal;
            })
            .ThenByDescending(p => p.GetGradeIndex())
            .ThenByDescending(p => p.Level)
            .ToList();

            return sorted[0];
        }

        public void AutoFeedAll(Player player, int totalFood)
        {
            if (player.OwnedPets.Count == 0 || totalFood <= 0) return;

            var sorted = player.OwnedPets
                .OrderByDescending(p => p == player.ActivePet ? 1 : 0)
                .ThenByDescending(p => p.GetGradeIndex())
                .ToList();

            int remaining = totalFood;
            int activeShare = (int)Math.Floor(totalFood * 0.6);

            if (sorted.Count > 0 && sorted[0] == player.ActivePet)
            {
                int feed = Math.Min(activeShare, remaining);
                sorted[0].Feed(feed);
                remaining -= feed;
                sorted.RemoveAt(0);
            }

            if (sorted.Count > 0 && remaining > 0)
            {
                int perPet = (int)Math.Floor((double)remaining / sorted.Count);
                foreach (var pet in sorted)
                {
                    if (perPet > 0) pet.Feed(perPet);
                }
            }
        }
    }
}
