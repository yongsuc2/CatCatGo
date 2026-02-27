using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Data;

namespace CatCatGo.Tests.Domain
{
    [TestFixture]
    public class BattleLogCategorizerTests
    {
        private const string PLAYER = "Player";
        private const string ENEMY = "Orc";

        private static BattleLogEntry MakeEntry(BattleLogType type, string source, string target, int value, string skillName = null)
        {
            return new BattleLogEntry
            {
                Turn = 1,
                Type = type,
                Source = source,
                Target = target,
                Value = value,
                SkillName = skillName,
            };
        }

        [Test]
        public void BasicAttackCategorizesAsNormalAttack()
        {
            var entries = new List<BattleLogEntry>
            {
                MakeEntry(BattleLogType.ATTACK, PLAYER, ENEMY, 100),
            };

            var result = BattleLogCategorizer.Categorize(entries, PLAYER);

            Assert.AreEqual(1, result.DamageMap.Count);
            Assert.IsTrue(result.DamageMap.ContainsKey("\uc77c\ubc18 \uacf5\uaca9"));
            Assert.AreEqual(100, result.DamageMap["\uc77c\ubc18 \uacf5\uaca9"]);
        }

        [Test]
        public void BasicCritCategorizesAsNormalAttack()
        {
            var entries = new List<BattleLogEntry>
            {
                MakeEntry(BattleLogType.CRIT, PLAYER, ENEMY, 200),
            };

            var result = BattleLogCategorizer.Categorize(entries, PLAYER);

            Assert.AreEqual(1, result.DamageMap.Count);
            Assert.IsTrue(result.DamageMap.ContainsKey("\uc77c\ubc18 \uacf5\uaca9"));
            Assert.AreEqual(200, result.DamageMap["\uc77c\ubc18 \uacf5\uaca9"]);
        }

        [Test]
        public void SkillDamageCategorizesbySkillName()
        {
            var entries = new List<BattleLogEntry>
            {
                MakeEntry(BattleLogType.SKILL_DAMAGE, PLAYER, ENEMY, 150, "\uad11\ucc3d \uc18c\ud658"),
            };

            var result = BattleLogCategorizer.Categorize(entries, PLAYER);

            Assert.AreEqual(1, result.DamageMap.Count);
            Assert.IsTrue(result.DamageMap.ContainsKey("\uad11\ucc3d \uc18c\ud658"));
            Assert.AreEqual(150, result.DamageMap["\uad11\ucc3d \uc18c\ud658"]);
        }

        [Test]
        public void SkillCritCategorizesbySkillName()
        {
            var entries = new List<BattleLogEntry>
            {
                MakeEntry(BattleLogType.CRIT, PLAYER, ENEMY, 300, "\uad11\ucc3d \uc18c\ud658"),
            };

            var result = BattleLogCategorizer.Categorize(entries, PLAYER);

            Assert.AreEqual(1, result.DamageMap.Count);
            Assert.IsTrue(result.DamageMap.ContainsKey("\uad11\ucc3d \uc18c\ud658"));
            Assert.AreEqual(300, result.DamageMap["\uad11\ucc3d \uc18c\ud658"]);
        }

        [Test]
        public void MixedEntriesCategorizeCorrectly()
        {
            var entries = new List<BattleLogEntry>
            {
                MakeEntry(BattleLogType.ATTACK, PLAYER, ENEMY, 100),
                MakeEntry(BattleLogType.CRIT, PLAYER, ENEMY, 200),
                MakeEntry(BattleLogType.SKILL_DAMAGE, PLAYER, ENEMY, 150, "\uad11\ucc3d \uc18c\ud658"),
                MakeEntry(BattleLogType.CRIT, PLAYER, ENEMY, 300, "\uad11\ucc3d \uc18c\ud658"),
                MakeEntry(BattleLogType.COUNTER, PLAYER, ENEMY, 80),
                MakeEntry(BattleLogType.RAGE_ATTACK, PLAYER, ENEMY, 250),
            };

            var result = BattleLogCategorizer.Categorize(entries, PLAYER);

            Assert.AreEqual(4, result.DamageMap.Count);
            Assert.AreEqual(300, result.DamageMap["\uc77c\ubc18 \uacf5\uaca9"]);
            Assert.AreEqual(450, result.DamageMap["\uad11\ucc3d \uc18c\ud658"]);
            Assert.AreEqual(80, result.DamageMap["\ubc18\uaca9"]);
            Assert.AreEqual(250, result.DamageMap["\ubd84\ub178 \uacf5\uaca9"]);
        }

        [Test]
        public void EnemyDamageExcludedFromPlayerMap()
        {
            var entries = new List<BattleLogEntry>
            {
                MakeEntry(BattleLogType.ATTACK, ENEMY, PLAYER, 100),
                MakeEntry(BattleLogType.ATTACK, PLAYER, ENEMY, 50),
            };

            var result = BattleLogCategorizer.Categorize(entries, PLAYER);

            Assert.AreEqual(1, result.DamageMap.Count);
            Assert.AreEqual(50, result.DamageMap["\uc77c\ubc18 \uacf5\uaca9"]);
        }

        [Test]
        public void DotDamageCategorizesAsDotDamage()
        {
            var entries = new List<BattleLogEntry>
            {
                MakeEntry(BattleLogType.DOT_DAMAGE, PLAYER, ENEMY, 30),
            };

            var result = BattleLogCategorizer.Categorize(entries, PLAYER);

            Assert.AreEqual(1, result.DamageMap.Count);
            Assert.IsTrue(result.DamageMap.ContainsKey("\ub3c5 \ud53c\ud574"));
            Assert.AreEqual(30, result.DamageMap["\ub3c5 \ud53c\ud574"]);
        }

        [Test]
        public void LifestealCategorizesInHealMap()
        {
            var entries = new List<BattleLogEntry>
            {
                MakeEntry(BattleLogType.LIFESTEAL, PLAYER, PLAYER, 40),
            };

            var result = BattleLogCategorizer.Categorize(entries, PLAYER);

            Assert.AreEqual(1, result.HealMap.Count);
            Assert.IsTrue(result.HealMap.ContainsKey("\ud761\ud608"));
            Assert.AreEqual(40, result.HealMap["\ud761\ud608"]);
        }

        [Test]
        public void HotHealCategorizesAsRegeneration()
        {
            var entries = new List<BattleLogEntry>
            {
                MakeEntry(BattleLogType.HOT_HEAL, PLAYER, PLAYER, 25),
            };

            var result = BattleLogCategorizer.Categorize(entries, PLAYER);

            Assert.AreEqual(1, result.HealMap.Count);
            Assert.IsTrue(result.HealMap.ContainsKey("\uc7ac\uc0dd"));
        }

        [Test]
        public void ReviveCategorizesInHealMap()
        {
            var entries = new List<BattleLogEntry>
            {
                MakeEntry(BattleLogType.REVIVE, PLAYER, PLAYER, 30),
            };

            var result = BattleLogCategorizer.Categorize(entries, PLAYER);

            Assert.AreEqual(1, result.HealMap.Count);
            Assert.IsTrue(result.HealMap.ContainsKey("\ubd80\ud65c"));
        }

        [Test]
        public void SkillHealCategorizesbySkillName()
        {
            var entries = new List<BattleLogEntry>
            {
                MakeEntry(BattleLogType.HEAL, PLAYER, PLAYER, 50, "\uce58\uc720\uc758 \ube5b"),
            };

            var result = BattleLogCategorizer.Categorize(entries, PLAYER);

            Assert.AreEqual(1, result.HealMap.Count);
            Assert.IsTrue(result.HealMap.ContainsKey("\uce58\uc720\uc758 \ube5b"));
            Assert.AreEqual(50, result.HealMap["\uce58\uc720\uc758 \ube5b"]);
        }

        [Test]
        public void SkillHealWithoutNameCategorizesAsGenericHeal()
        {
            var entries = new List<BattleLogEntry>
            {
                MakeEntry(BattleLogType.HEAL, PLAYER, PLAYER, 50),
            };

            var result = BattleLogCategorizer.Categorize(entries, PLAYER);

            Assert.AreEqual(1, result.HealMap.Count);
            Assert.IsTrue(result.HealMap.ContainsKey("\ud68c\ubcf5"));
        }

        [Test]
        public void IntegrationWithRealBattle_SkillCritsUseSkillName()
        {
            var lance = new ActiveSkill(
                "lance_summon", "\uad11\ucc3d \uc18c\ud658", "X", SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1), TriggerFactory.Prob(1.0f), TriggerFactory.NoCondition()),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = 0.5f } });

            var builtinSkills = ActiveSkillRegistry.GetBuiltinSkills();
            var allSkills = new List<ActiveSkill>(builtinSkills) { lance };

            var player = new BattleUnit(
                "Player",
                Stats.Create(hp: 500, maxHp: 500, atk: 50, def: 10, crit: 1.0f),
                allSkills.ToArray(),
                null,
                true);
            var enemy = new BattleUnit("Orc", Stats.Create(hp: 300, maxHp: 300, atk: 10, def: 3), null, null, false);

            var battle = new Battle(player, enemy, 42);
            battle.RunToCompletion();

            var critWithSkill = battle.Log.Entries
                .Where(e => e.Type == BattleLogType.CRIT && !string.IsNullOrEmpty(e.SkillName) && e.SkillName != "\uc77c\ubc18 \uacf5\uaca9")
                .ToList();

            if (critWithSkill.Count > 0)
            {
                var result = BattleLogCategorizer.Categorize(battle.Log.Entries, "Player");
                foreach (var entry in critWithSkill)
                {
                    Assert.IsTrue(
                        result.DamageMap.ContainsKey(entry.SkillName),
                        $"Skill crit '{entry.SkillName}' should be categorized by skill name, not as normal attack");
                }
            }
        }

        [Test]
        public void IntegrationWithRealBattle_AllPlayerDamageAccountedFor()
        {
            var player = new BattleUnit("Player", Stats.Create(hp: 200, maxHp: 200, atk: 30, def: 10), null, null, true);
            var enemy = new BattleUnit("Slime", Stats.Create(hp: 100, maxHp: 100, atk: 5, def: 2), null, null, false);

            var battle = new Battle(player, enemy, 42);
            battle.RunToCompletion();

            var result = BattleLogCategorizer.Categorize(battle.Log.Entries, "Player");

            int categorizedTotal = result.DamageMap.Values.Sum();

            int expectedTotal = battle.Log.Entries
                .Where(e =>
                    e.Source == "Player" && e.Target != "Player" &&
                    (e.Type == BattleLogType.ATTACK || e.Type == BattleLogType.CRIT ||
                     e.Type == BattleLogType.SKILL_DAMAGE || e.Type == BattleLogType.COUNTER ||
                     e.Type == BattleLogType.RAGE_ATTACK || e.Type == BattleLogType.DOT_DAMAGE))
                .Sum(e => e.Value);

            Assert.AreEqual(expectedTotal, categorizedTotal,
                "All player damage should be accounted for in the categorized map");
        }
    }
}
