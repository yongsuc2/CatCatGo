using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;

namespace CatCatGo.Tests.Domain
{
    [TestFixture]
    public class SkillExecutionEngineTests
    {
        private class MockUnit : ISkillExecutionUnit
        {
            public string Name { get; set; }
            public int CurrentHp { get; set; }
            public int MaxHp { get; set; }
            public int Rage { get; set; }
            public int MaxRage { get; set; }
            public int RagePerAttack { get; set; }
            public float MagicCoefficient { get; set; }
            public HashSet<string> UsedOnceConditions { get; set; }

            private Func<float> _getEffectiveAtk;
            private Func<float> _getEffectiveDef;
            private Func<float> _getEffectiveCrit;
            private Func<string, float> _getMasteryBonus;

            public MockUnit(
                string name = "TestUnit",
                int currentHp = 100,
                int maxHp = 100,
                Func<float> getEffectiveAtk = null,
                Func<float> getEffectiveDef = null,
                Func<float> getEffectiveCrit = null,
                int rage = 0,
                int maxRage = 100,
                float magicCoefficient = 0f)
            {
                Name = name;
                CurrentHp = currentHp;
                MaxHp = maxHp;
                Rage = rage;
                MaxRage = maxRage;
                MagicCoefficient = magicCoefficient == 0f ? BattleDataTable.Data.Damage.BaseMagicCoefficient : magicCoefficient;
                UsedOnceConditions = new HashSet<string>();
                _getEffectiveAtk = getEffectiveAtk ?? (() => 50f);
                _getEffectiveDef = getEffectiveDef ?? (() => 20f);
                _getEffectiveCrit = getEffectiveCrit ?? (() => 0f);
                _getMasteryBonus = _ => 0f;
            }

            public float GetEffectiveAtk() => _getEffectiveAtk();
            public float GetEffectiveDef() => _getEffectiveDef();
            public float GetEffectiveCrit() => _getEffectiveCrit();

            public int TakeDamage(int amount)
            {
                int dealt = Math.Min(amount, CurrentHp);
                CurrentHp -= dealt;
                return dealt;
            }

            public int Heal(int amount)
            {
                int actual = Math.Min(amount, MaxHp - CurrentHp);
                CurrentHp += actual;
                return actual;
            }

            public void AddStatusEffect(StatusEffect effect) { }
            public bool IsAlive() => CurrentHp > 0;
            public float GetHpPercent() => MaxHp > 0 ? (float)CurrentHp / MaxHp : 0;
            public float GetMasteryBonus(string skillId) => _getMasteryBonus(skillId);
        }

        [Test]
        public void ExecutesSimplePhysicalAttackSkill()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = new ActiveSkill(
                "test_attack", "Test Attack", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = 1.0f } });

            var source = new MockUnit();
            var target = new MockUnit();
            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(1, results.Count);
            Assert.Greater(results[0].Damage, 0);
            Assert.Less(target.CurrentHp, 100);
        }

        [Test]
        public void ExecutesMagicAttackWithMagicCoefficient()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = new ActiveSkill(
                "test_magic", "Test Magic", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.MAGIC, Coefficient = 0.5f } });

            var source = new MockUnit();
            var target = new MockUnit();
            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(1, results.Count);
            Assert.Greater(results[0].Damage, 0);
        }

        [Test]
        public void ExecutesTriggerSkillToChainSkills()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var allSkills = ActiveSkillRegistry.GetAll();

            var thunderStrike = ActiveSkillRegistry.GetById("thunder_strike", 1);
            Assert.IsNotNull(thunderStrike);
            var results = engine.ExecuteSkillEffects(
                thunderStrike, new MockUnit(), new MockUnit(), allSkills);

            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.Any(r => r.Damage > 0));
        }

        [Test]
        public void RespectsMaxSkillChainDepth()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));

            var lowest = new ActiveSkill(
                "test_lowest", "Lowest", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = 0.1f } });

            var lower = new ActiveSkill(
                "test_lower", "Lower", "X",
                SkillHierarchy.LOWER, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "test_lowest", Count = 1 } });

            var upper = new ActiveSkill(
                "test_upper", "Upper", "X",
                SkillHierarchy.UPPER, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "test_lower", Count = 1 } });

            var allSkills = new List<ActiveSkill> { upper, lower, lowest };
            var results = engine.ExecuteSkillEffects(upper, new MockUnit(), new MockUnit(), allSkills);

            Assert.AreEqual(1, results.Count);
            Assert.Greater(results[0].Damage, 0);
        }

        [Test]
        public void InjectEffectAddsEffectsToTargetSkill()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(1));

            var allSkills = ActiveSkillRegistry.GetAll();
            engine.ResolveInjections(allSkills);

            var shurikenSummon = allSkills.FirstOrDefault(s => s.Id == "shuriken_summon" && s.Tier == 1);
            Assert.IsNotNull(shurikenSummon);
            var resolved = engine.GetResolvedEffects(shurikenSummon);

            var thunderShuriken = allSkills.FirstOrDefault(s => s.Id == "thunder_shuriken" && s.Tier == 1);
            if (thunderShuriken != null)
            {
                Assert.GreaterOrEqual(resolved.Count, shurikenSummon.Effects.Length);
            }
        }

        [Test]
        public void HealHpEffectHealsTheSource()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = new ActiveSkill(
                "test_heal", "Heal", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.HEAL_HP, Coefficient = 0.2f } });

            var source = new MockUnit(currentHp: 50);
            var target = new MockUnit();
            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(20, results[0].HealAmount);
            Assert.AreEqual(70, source.CurrentHp);
        }

        [Test]
        public void AddRageEffectIncreasesRage()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = new ActiveSkill(
                "test_rage", "Rage Add", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.ADD_RAGE, Amount = 25 } });

            var source = new MockUnit(rage: 0);
            var target = new MockUnit();
            engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(25, source.Rage);
        }

        [Test]
        public void ConsumeRageEffectDecreasesRage()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = new ActiveSkill(
                "test_consume", "Consume", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.CONSUME_RAGE, Amount = 100 } });

            var source = new MockUnit(rage: 100);
            var target = new MockUnit();
            engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(0, source.Rage);
        }

        [Test]
        public void EvaluateTriggerChecksEveryNTurns()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var source = new MockUnit();

            var t = TriggerFactory.Trigger(TriggerFactory.EveryNTurns(2));
            Assert.IsFalse(engine.EvaluateTrigger(t, 1, source));
            Assert.IsTrue(engine.EvaluateTrigger(t, 2, source));
            Assert.IsFalse(engine.EvaluateTrigger(t, 3, source));
            Assert.IsTrue(engine.EvaluateTrigger(t, 4, source));
        }

        [Test]
        public void EvaluateTriggerChecksOnSkillActivation()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var source = new MockUnit();

            var t = TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack"));
            Assert.IsTrue(engine.EvaluateTrigger(t, 1, source, "ilban_attack"));
            Assert.IsFalse(engine.EvaluateTrigger(t, 1, source, "bunno_attack"));
            Assert.IsFalse(engine.EvaluateTrigger(t, 1, source));
        }

        [Test]
        public void EvaluateTriggerChecksRageFullCondition()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));

            var sourceNoRage = new MockUnit(rage: 50);
            var sourceFullRage = new MockUnit(rage: 100);

            var t = TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1), TriggerFactory.Prob(1.0f), TriggerFactory.RageFull());
            Assert.IsFalse(engine.EvaluateTrigger(t, 1, sourceNoRage));
            Assert.IsTrue(engine.EvaluateTrigger(t, 1, sourceFullRage));
        }

        [Test]
        public void EvaluateTriggerChecksProbability()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var source = new MockUnit();

            var t = TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1), TriggerFactory.Prob(0.0f));
            Assert.IsFalse(engine.EvaluateTrigger(t, 1, source));
        }

        [Test]
        public void StopsExecutingWhenTargetDies()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var source = new MockUnit(getEffectiveAtk: () => 1000);
            var target = new MockUnit(currentHp: 1);

            var skill = new ActiveSkill(
                "overkill", "Overkill", "X",
                SkillHierarchy.UPPER, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[]
                {
                    new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "hit1", Count = 1 },
                    new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "hit2", Count = 1 },
                });

            var hit1 = new ActiveSkill(
                "hit1", "Hit 1", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = 10.0f } });

            var hit2 = new ActiveSkill(
                "hit2", "Hit 2", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = 10.0f } });

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill, hit1, hit2 });

            Assert.LessOrEqual(target.CurrentHp, 0);
            Assert.LessOrEqual(results.Count, 2);
        }

        [Test]
        public void AoeAttackHitsAllAliveTargets()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = new ActiveSkill(
                "test_aoe", "AoE Attack", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = 1.0f, IsAoe = true } });

            var source = new MockUnit(getEffectiveAtk: () => 50);
            var target1 = new MockUnit(name: "Enemy1", currentHp: 100);
            var target2 = new MockUnit(name: "Enemy2", currentHp: 100);
            var allTargets = new List<ISkillExecutionUnit> { target1, target2 };

            var results = engine.ExecuteSkillEffects(skill, source, target1, new List<ActiveSkill> { skill }, 0, allTargets);

            Assert.AreEqual(2, results.Count);
            Assert.Less(target1.CurrentHp, 100);
            Assert.Less(target2.CurrentHp, 100);
            Assert.AreEqual("Enemy1", results[0].TargetName);
            Assert.AreEqual("Enemy2", results[1].TargetName);
        }

        [Test]
        public void AoeSkipsDeadTargets()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = new ActiveSkill(
                "test_aoe", "AoE Attack", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = 1.0f, IsAoe = true } });

            var source = new MockUnit(getEffectiveAtk: () => 50);
            var target1 = new MockUnit(name: "Dead", currentHp: 0);
            var target2 = new MockUnit(name: "Alive", currentHp: 100);
            var allTargets = new List<ISkillExecutionUnit> { target1, target2 };

            var results = engine.ExecuteSkillEffects(skill, source, target2, new List<ActiveSkill> { skill }, 0, allTargets);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Alive", results[0].TargetName);
        }

        [Test]
        public void PercentageDefenseFormulaPhysicalDamage()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = new ActiveSkill(
                "test_phys", "Phys", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = 1.0f } });

            int atk = 100;
            int def = 50;
            float k = BattleDataTable.Data.Damage.DefenseConstant;
            int expectedDamage = Math.Max(1, (int)(atk * 1.0f * (k / (k + def))));

            var source = new MockUnit(getEffectiveAtk: () => atk, getEffectiveCrit: () => 0f);
            var target = new MockUnit(currentHp: 1000, maxHp: 1000, getEffectiveDef: () => def);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(expectedDamage, results[0].Damage);
            Assert.AreEqual((int)(100f * (100f / 150f)), expectedDamage);
        }

        [Test]
        public void PercentageDefenseFormulaMagicDamageIncludesMagicCoefficient()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = new ActiveSkill(
                "test_magic_def", "Magic", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.MAGIC, Coefficient = 0.5f } });

            int atk = 100;
            int def = 50;
            float magicCoeff = 0.7f;
            float k = BattleDataTable.Data.Damage.MagicDefenseConstant;
            int expectedDamage = Math.Max(1, (int)(atk * magicCoeff * 0.5f * (k / (k + def))));

            var source = new MockUnit(
                getEffectiveAtk: () => atk,
                getEffectiveCrit: () => 0f,
                magicCoefficient: magicCoeff);
            var target = new MockUnit(currentHp: 1000, maxHp: 1000, getEffectiveDef: () => def);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(expectedDamage, results[0].Damage);
        }
    }
}
