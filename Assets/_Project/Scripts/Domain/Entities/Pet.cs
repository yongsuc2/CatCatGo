using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Domain.Entities
{
    public class Pet
    {
        private static readonly PetGrade[] PET_GRADE_ORDER = new[]
        {
            PetGrade.COMMON,
            PetGrade.RARE,
            PetGrade.EPIC,
            PetGrade.LEGENDARY,
            PetGrade.IMMORTAL,
        };

        private const int EXP_PER_FOOD = 10;
        private const int EXP_PER_LEVEL = 100;
        private const int STAT_PER_LEVEL = 2;

        public string Id { get; }
        public string Name { get; }
        public PetTier Tier { get; }
        public PetGrade Grade { get; set; }
        public int Level { get; set; }
        public Stats BasePassiveBonus { get; }
        public int Exp { get; set; }

        public Pet(string id, string name, PetTier tier, PetGrade grade, int level = 1, Stats basePassiveBonus = default, int exp = 0)
        {
            Id = id;
            Name = name;
            Tier = tier;
            Grade = grade;
            Level = level;
            BasePassiveBonus = basePassiveBonus;
            Exp = exp;
        }

        public Result<FeedResult> Feed(int foodAmount)
        {
            if (foodAmount <= 0)
            {
                return Result.Fail<FeedResult>("No food to use");
            }

            int oldLevel = Level;
            Exp += foodAmount * EXP_PER_FOOD;

            while (Exp >= GetExpToNextLevel())
            {
                Exp -= GetExpToNextLevel();
                Level += 1;
            }

            return Result.Ok(new FeedResult { LevelsGained = Level - oldLevel });
        }

        public int GetExpToNextLevel()
        {
            return EXP_PER_LEVEL + (Level - 1) * 20;
        }

        public Result<UpgradeGradeResult> UpgradeGrade()
        {
            int idx = System.Array.IndexOf(PET_GRADE_ORDER, Grade);
            if (idx >= PET_GRADE_ORDER.Length - 1)
            {
                return Result.Fail<UpgradeGradeResult>("Already at max grade");
            }

            Grade = PET_GRADE_ORDER[idx + 1];
            return Result.Ok(new UpgradeGradeResult { NewGrade = Grade });
        }

        public Stats GetGlobalBonus()
        {
            var levelBonus = Stats.Create(
                atk: Level * STAT_PER_LEVEL,
                maxHp: Level * STAT_PER_LEVEL * 2
            );
            return BasePassiveBonus.Add(levelBonus);
        }

        public int GetGradeIndex()
        {
            return System.Array.IndexOf(PET_GRADE_ORDER, Grade);
        }

        public bool IsMaxGrade()
        {
            return Grade == PetGrade.IMMORTAL;
        }
    }

    public struct FeedResult
    {
        public int LevelsGained;
    }

    public struct UpgradeGradeResult
    {
        public PetGrade NewGrade;
    }
}
