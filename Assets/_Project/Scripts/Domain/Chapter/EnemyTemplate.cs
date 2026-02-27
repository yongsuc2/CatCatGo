using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Data;

namespace CatCatGo.Domain.Chapter
{
    public class EnemyTemplate
    {
        public readonly string Id;
        public readonly string Name;
        public readonly Stats BaseStats;
        public readonly ActiveSkill[] ActiveSkills;
        public readonly PassiveSkill[] PassiveSkills;
        public readonly bool IsBoss;
        public readonly int RagePerAttack;

        public EnemyTemplate(
            string id,
            string name,
            Stats baseStats,
            ActiveSkill[] activeSkills,
            PassiveSkill[] passiveSkills,
            bool isBoss,
            int ragePerAttack)
        {
            Id = id;
            Name = name;
            BaseStats = baseStats;
            ActiveSkills = activeSkills;
            PassiveSkills = passiveSkills;
            IsBoss = isBoss;
            RagePerAttack = ragePerAttack;
        }

        public static EnemyTemplate FromData(EnemyTemplateData data)
        {
            var activeSkills = new List<ActiveSkill>();
            var passiveSkills = new List<PassiveSkill>();

            foreach (var id in data.SkillIds)
            {
                var passive = PassiveSkillRegistry.GetById(id, 1);
                if (passive != null) { passiveSkills.Add(passive); continue; }
                var active = ActiveSkillRegistry.GetById(id, 1);
                if (active != null) { activeSkills.Add(active); }
            }

            return new EnemyTemplate(
                data.Id,
                data.Name,
                data.BaseStats,
                activeSkills.ToArray(),
                passiveSkills.ToArray(),
                data.IsBoss,
                data.RagePerAttack);
        }

        public static EnemyTemplate FromId(string id)
        {
            var data = EnemyTable.GetTemplate(id);
            if (data == null) return null;
            return FromData(data);
        }

        public BattleUnit CreateInstance(int chapterLevel, float statMultiplier = 1.0f, float dayProgress = 0f)
        {
            var scaledStats = EnemyTable.GetScaledStats(BaseStats, chapterLevel);
            if (dayProgress > 0)
            {
                float dayBonus = 1 + dayProgress * BattleDataTable.Data.Enemy.DayProgressMaxBonus;
                scaledStats = scaledStats.Multiply(dayBonus);
            }
            if (statMultiplier != 1.0f)
            {
                scaledStats = scaledStats.Multiply(statMultiplier);
            }
            return new BattleUnit(
                Name,
                scaledStats,
                ActiveSkills.ToArray(),
                PassiveSkills.ToArray(),
                false);
        }

        public BattleUnit CreateTowerInstance(int floor)
        {
            var scaledStats = EnemyTable.GetTowerScaledStats(BaseStats, floor);
            return new BattleUnit(
                Name,
                scaledStats,
                ActiveSkills.ToArray(),
                PassiveSkills.ToArray(),
                false);
        }
    }
}
