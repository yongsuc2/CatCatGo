using System;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;

namespace CatCatGo.Services
{
    public class PetManager
    {
        public Pet HatchEgg(SeededRandom rng, PetGrade grade = PetGrade.COMMON, string idPrefix = "pet")
        {
            var template = PetTable.GetRandomTemplate(rng);
            return new Pet(
                $"{idPrefix}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{rng.NextInt(0, 9999)}",
                template.Name,
                template.Tier,
                grade,
                template.MaxGrade,
                1,
                template.BasePassiveBonus);
        }

    }
}
