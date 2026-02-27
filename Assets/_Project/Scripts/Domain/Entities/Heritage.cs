using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Data;

namespace CatCatGo.Domain.Entities
{
    public class Heritage
    {
        public HeritageRoute Route;
        public int Level;

        public Heritage(HeritageRoute route = HeritageRoute.SKULL, int level = 0)
        {
            Route = route;
            Level = level;
        }

        public static bool IsUnlocked(TalentGrade talentGrade)
        {
            return talentGrade == TalentGrade.HERO;
        }

        public int GetUpgradeCost()
        {
            return HeritageTable.GetUpgradeCost(Level);
        }

        public ResourceType GetRequiredBookType()
        {
            return HeritageTable.GetBookType(Route);
        }

        public Result<HeritageUpgradeResult> Upgrade(int availableBooks)
        {
            int cost = GetUpgradeCost();
            if (availableBooks < cost)
                return Result.Fail<HeritageUpgradeResult>("Not enough books");

            Level += 1;
            return Result.Ok(new HeritageUpgradeResult { Cost = cost, NewLevel = Level });
        }

        public float GetSkillMultiplier(IHeritageSynergyProvider skill)
        {
            bool isSynergy = skill.HasHeritageSynergy(Route);
            return HeritageTable.GetSkillMultiplier(Route, Level, isSynergy);
        }

        public Stats GetPassiveBonus()
        {
            var perLevel = HeritageTable.GetPassivePerLevel(Route);
            return perLevel.Multiply(Level);
        }

        public void ChangeRoute(HeritageRoute newRoute)
        {
            Route = newRoute;
            Level = 0;
        }
    }

    public class HeritageUpgradeResult
    {
        public int Cost;
        public int NewLevel;
    }

    public interface IHeritageSynergyProvider
    {
        bool HasHeritageSynergy(HeritageRoute route);
    }
}
