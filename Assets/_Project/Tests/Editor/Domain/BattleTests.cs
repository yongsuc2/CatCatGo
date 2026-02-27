using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Data;

namespace CatCatGo.Tests.Domain
{
    [TestFixture]
    public class BattleUnitTests
    {
        private static PassiveSkill MakePassive(string id, PassiveEffect effect)
        {
            return new PassiveSkill(id, id, "X", 1, new SkillTag[0], new HeritageRoute[0], effect);
        }

        [Test]
        public void TracksHpCorrectly()
        {
            var unit = new BattleUnit("Test", Stats.Create(hp: 100, maxHp: 100, atk: 10, def: 5));
            Assert.IsTrue(unit.IsAlive());

            unit.TakeDamage(60);
            Assert.AreEqual(40, unit.CurrentHp);
            Assert.IsTrue(unit.IsAlive());

            unit.TakeDamage(50);
            Assert.AreEqual(0, unit.CurrentHp);
            Assert.IsFalse(unit.IsAlive());
        }

        [Test]
        public void HealsUpToMaxHp()
        {
            var unit = new BattleUnit("Test", Stats.Create(hp: 50, maxHp: 100, atk: 10, def: 5));
            unit.CurrentHp = 50;
            int healed = unit.Heal(80);
            Assert.AreEqual(50, healed);
            Assert.AreEqual(100, unit.CurrentHp);
        }

        [Test]
        public void ReportsHpPercent()
        {
            var unit = new BattleUnit("Test", Stats.Create(hp: 100, maxHp: 200, atk: 10, def: 5));
            unit.CurrentHp = 100;
            Assert.AreEqual(0.5f, unit.GetHpPercent(), 0.01f);
        }

        [Test]
        public void AppliesMultiHitChanceFromPassiveSkill()
        {
            var multiHit = MakePassive("mh", new PassiveEffect { Type = PassiveType.MULTI_HIT, Chance = 0.5f });
            var unit = new BattleUnit("Test", Stats.Create(hp: 100, maxHp: 100, atk: 10, def: 5), null, new[] { multiHit });
            Assert.AreEqual(0.5f, unit.MultiHitChance, 0.01f);
        }

        [Test]
        public void AppliesLifestealFromPassiveSkill()
        {
            var lifesteal = MakePassive("ls", new PassiveEffect { Type = PassiveType.LIFESTEAL, Rate = 0.15f });
            var unit = new BattleUnit("Test", Stats.Create(hp: 100, maxHp: 100, atk: 10, def: 5), null, new[] { lifesteal });
            Assert.AreEqual(0.15f, unit.LifestealRate, 0.01f);
        }

        [Test]
        public void HpFortifyIncreasesMaxHpAndCurrentHpByDelta()
        {
            var hpFortify = MakePassive("hp_fortify", new PassiveEffect
            {
                Type = PassiveType.STAT_MODIFIER, Stat = StatType.HP, Value = 0.1f, IsPercentage = true,
            });
            var unit = new BattleUnit("Test", Stats.Create(hp: 100, maxHp: 100, atk: 10, def: 5), null, new[] { hpFortify });
            Assert.AreEqual(110, unit.MaxHp);
            Assert.AreEqual(110, unit.CurrentHp);
        }

        [Test]
        public void HpFortifyPreservesHpDeltaWhenNotAtFullHp()
        {
            var hpFortify = MakePassive("hp_fortify", new PassiveEffect
            {
                Type = PassiveType.STAT_MODIFIER, Stat = StatType.HP, Value = 0.1f, IsPercentage = true,
            });
            var unit = new BattleUnit("Test", Stats.Create(hp: 50, maxHp: 100, atk: 10, def: 5), null, new[] { hpFortify });
            Assert.AreEqual(110, unit.MaxHp);
            Assert.AreEqual(60, unit.CurrentHp);
        }

        [Test]
        public void MagicMasteryIncreasesMagicCoefficient()
        {
            var magicMastery = MakePassive("magic_mastery", new PassiveEffect
            {
                Type = PassiveType.STAT_MODIFIER, Stat = StatType.MAGIC_COEFFICIENT, Value = 0.1f, IsPercentage = false,
            });
            var unit = new BattleUnit("Test", Stats.Create(hp: 100, maxHp: 100, atk: 10, def: 5), null, new[] { magicMastery });
            Assert.AreEqual(BattleDataTable.Data.Damage.BaseMagicCoefficient + 0.1f, unit.MagicCoefficient, 0.01f);
        }

        [Test]
        public void StatModifierAppliedBeforeShieldOnStartForCorrectShieldCalc()
        {
            var hpUp = MakePassive("hp_up", new PassiveEffect
            {
                Type = PassiveType.STAT_MODIFIER, Stat = StatType.HP, Value = 0.5f, IsPercentage = true,
            });
            var shield = MakePassive("shield", new PassiveEffect
            {
                Type = PassiveType.SHIELD_ON_START, HpPercent = 0.1f,
            });
            var unit = new BattleUnit("Test", Stats.Create(hp: 100, maxHp: 100, atk: 10, def: 5), null, new[] { shield, hpUp });
            Assert.AreEqual(150, unit.MaxHp);
            Assert.AreEqual(15, unit.Shield);
        }
    }

    [TestFixture]
    public class BattleTests
    {
        private static ActiveSkill MakeLowestAttackSkill(string id, string name, float coefficient)
        {
            return new ActiveSkill(id, name, "X", SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1), TriggerFactory.Prob(1.0f), TriggerFactory.NoCondition()),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = coefficient } });
        }

        private static PassiveSkill MakePassive(string id, PassiveEffect effect)
        {
            return new PassiveSkill(id, id, "X", 1, new SkillTag[0], new HeritageRoute[0], effect);
        }

        [Test]
        public void PlayerWinsAgainstWeakerEnemy()
        {
            var player = new BattleUnit("Player", Stats.Create(hp: 200, maxHp: 200, atk: 30, def: 10, crit: 0.1f), null, null, true);
            var enemy = new BattleUnit("Slime", Stats.Create(hp: 50, maxHp: 50, atk: 5, def: 2), null, null, false);

            var battle = new Battle(player, enemy, 42);
            battle.RunToCompletion();

            Assert.AreEqual(BattleState.VICTORY, battle.State);
            Assert.IsTrue(battle.Player.IsAlive());
            Assert.IsFalse(battle.Enemy.IsAlive());
        }

        [Test]
        public void PlayerLosesAgainstMuchStrongerEnemy()
        {
            var player = new BattleUnit("Player", Stats.Create(hp: 50, maxHp: 50, atk: 5, def: 2), null, null, true);
            var enemy = new BattleUnit("Dragon", Stats.Create(hp: 500, maxHp: 500, atk: 50, def: 20), null, null, false);

            var battle = new Battle(player, enemy, 42);
            battle.RunToCompletion();

            Assert.AreEqual(BattleState.DEFEAT, battle.State);
        }

        [Test]
        public void TracksTurnCount()
        {
            var player = new BattleUnit("Player", Stats.Create(hp: 100, maxHp: 100, atk: 20, def: 5), null, null, true);
            var enemy = new BattleUnit("Slime", Stats.Create(hp: 30, maxHp: 30, atk: 5, def: 2), null, null, false);

            var battle = new Battle(player, enemy, 42);
            battle.RunToCompletion();

            Assert.Greater(battle.TurnCount, 0);
            Assert.Less(battle.TurnCount, 20);
        }

        [Test]
        public void ReviveAllowsPlayerToSurviveOnce()
        {
            var revive = MakePassive("revive", new PassiveEffect { Type = PassiveType.REVIVE, HpPercent = 0.3f, MaxUses = 1 });

            var player = new BattleUnit("Player", Stats.Create(hp: 30, maxHp: 100, atk: 20, def: 5), null, new[] { revive }, true);
            player.CurrentHp = 30;
            var enemy = new BattleUnit("Orc", Stats.Create(hp: 80, maxHp: 80, atk: 25, def: 5), null, null, false);

            var battle = new Battle(player, enemy, 42);
            battle.RunToCompletion();

            var reviveLog = battle.Log.Entries.FirstOrDefault(e => e.Type == BattleLogType.REVIVE);
            if (battle.State == BattleState.DEFEAT)
            {
                Assert.IsTrue(player.ReviveUsed);
            }
            else
            {
                Assert.IsNotNull(reviveLog);
            }
        }

        [Test]
        public void GeneratesBattleLogEntries()
        {
            var player = new BattleUnit("Player", Stats.Create(hp: 100, maxHp: 100, atk: 20, def: 5), null, null, true);
            var enemy = new BattleUnit("Slime", Stats.Create(hp: 40, maxHp: 40, atk: 8, def: 2), null, null, false);

            var battle = new Battle(player, enemy, 42);
            battle.RunToCompletion();

            Assert.Greater(battle.Log.Entries.Count, 0);
            Assert.IsTrue(battle.Log.Entries.Any(e => e.Type == BattleLogType.ATTACK));
        }

        [Test]
        public void LifestealHealsPlayerDuringCombat()
        {
            var lifesteal = MakePassive("ls", new PassiveEffect { Type = PassiveType.LIFESTEAL, Rate = 0.5f });

            var player = new BattleUnit("Player", Stats.Create(hp: 100, maxHp: 200, atk: 30, def: 5), null, new[] { lifesteal }, true);
            player.CurrentHp = 100;
            var enemy = new BattleUnit("Orc", Stats.Create(hp: 200, maxHp: 200, atk: 10, def: 3), null, null, false);

            var battle = new Battle(player, enemy, 42);
            battle.RunToCompletion();

            var lsEntries = battle.Log.Entries.Where(e => e.Type == BattleLogType.LIFESTEAL).ToList();
            Assert.Greater(lsEntries.Count, 0);
        }

        [Test]
        public void PlayerActiveSkillsDealExtraDamage()
        {
            var lance = MakeLowestAttackSkill("lance_summon", "\uad11\ucc3d \uc18c\ud658", 0.5f);

            var upperLance = new ActiveSkill(
                "lance_strike", "\uad11\ucc3d \uac15\ud0c0", "X", SkillHierarchy.UPPER, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1), TriggerFactory.Prob(1.0f), TriggerFactory.NoCondition()),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "lance_summon", Count = 1 } });

            var builtinSkills = ActiveSkillRegistry.GetBuiltinSkills();
            var allSkills = new List<ActiveSkill>(builtinSkills) { upperLance, lance };

            var player = new BattleUnit(
                "Player",
                Stats.Create(hp: 200, maxHp: 200, atk: 10, def: 5),
                allSkills.ToArray(),
                null,
                true);
            var enemy = new BattleUnit("Orc", Stats.Create(hp: 100, maxHp: 100, atk: 10, def: 3), null, null, false);

            var battle = new Battle(player, enemy, 42);
            battle.RunToCompletion();

            var skillEntries = battle.Log.Entries.Where(e => e.Type == BattleLogType.SKILL_DAMAGE && e.SkillName == "\uad11\ucc3d \uc18c\ud658").ToList();
            Assert.Greater(skillEntries.Count, 0);
        }
    }
}
