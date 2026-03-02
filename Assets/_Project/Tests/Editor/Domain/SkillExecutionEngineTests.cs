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
            public List<StatusEffect> AppliedEffects;

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
                Func<string, float> getMasteryBonus = null,
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
                AppliedEffects = new List<StatusEffect>();
                _getEffectiveAtk = getEffectiveAtk ?? (() => 50f);
                _getEffectiveDef = getEffectiveDef ?? (() => 20f);
                _getEffectiveCrit = getEffectiveCrit ?? (() => 0f);
                _getMasteryBonus = getMasteryBonus ?? (_ => 0f);
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

            public void AddStatusEffect(StatusEffect effect) { AppliedEffects.Add(effect); }
            public bool IsAlive() => CurrentHp > 0;
            public float GetHpPercent() => MaxHp > 0 ? (float)CurrentHp / MaxHp : 0;
            public float GetMasteryBonus(string skillId) => _getMasteryBonus(skillId);
        }

        private ActiveSkill MakeAttackSkill(
            string id,
            AttackType attackType,
            float coefficient,
            DamageBase damageBase = DamageBase.ATK,
            int duration = 0,
            bool isAoe = false)
        {
            return new ActiveSkill(
                id, id, "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect
                {
                    Type = SkillEffectType.ATTACK,
                    AttackType = attackType,
                    Coefficient = coefficient,
                    DamageBase = damageBase,
                    Duration = duration,
                    IsAoe = isAoe,
                }});
        }

        #region Basic Skill Effects

        [Test]
        public void ExecutesSimplePhysicalAttackSkill()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("test_attack", AttackType.PHYSICAL, 1.0f);

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
            var skill = MakeAttackSkill("test_magic", AttackType.MAGIC, 0.5f);

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

        #endregion

        #region Trigger Evaluation

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

        #endregion

        #region Target Alive Checks

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

            var hit1 = MakeAttackSkill("hit1", AttackType.PHYSICAL, 10.0f);
            var hit2 = MakeAttackSkill("hit2", AttackType.PHYSICAL, 10.0f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill, hit1, hit2 });

            Assert.LessOrEqual(target.CurrentHp, 0);
            Assert.LessOrEqual(results.Count, 2);
        }

        [Test]
        public void AoeAttackHitsAllAliveTargets()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("test_aoe", AttackType.PHYSICAL, 1.0f, isAoe: true);

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
            var skill = MakeAttackSkill("test_aoe", AttackType.PHYSICAL, 1.0f, isAoe: true);

            var source = new MockUnit(getEffectiveAtk: () => 50);
            var target1 = new MockUnit(name: "Dead", currentHp: 0);
            var target2 = new MockUnit(name: "Alive", currentHp: 100);
            var allTargets = new List<ISkillExecutionUnit> { target1, target2 };

            var results = engine.ExecuteSkillEffects(skill, source, target2, new List<ActiveSkill> { skill }, 0, allTargets);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Alive", results[0].TargetName);
        }

        #endregion

        #region Physical Damage Formula

        [Test]
        public void PhysicalDamage_AtkBase_WithDefense()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("phys", AttackType.PHYSICAL, 1.0f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 100f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(1, results.Count);
            Assert.IsFalse(results[0].IsCrit);
            Assert.AreEqual(100, results[0].Damage);
        }

        [Test]
        public void PhysicalDamage_AtkBase_HigherCoefficient()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("phys_high", AttackType.PHYSICAL, 2.5f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 100f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(250, results[0].Damage);
        }

        [Test]
        public void PhysicalDamage_WithCriticalHit()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("phys_crit", AttackType.PHYSICAL, 1.0f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 1.0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 100f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.IsTrue(results[0].IsCrit);
            Assert.AreEqual(150, results[0].Damage);
        }

        [Test]
        public void PhysicalDamage_WithMasteryBonus()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("phys_mastery", AttackType.PHYSICAL, 1.0f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f,
                getMasteryBonus: id => id == "phys_mastery" ? 0.5f : 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 100f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(150, results[0].Damage);
        }

        [Test]
        public void PhysicalDamage_WithCritAndMastery()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("phys_both", AttackType.PHYSICAL, 1.0f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 1.0f,
                getMasteryBonus: id => id == "phys_both" ? 0.5f : 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 100f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.IsTrue(results[0].IsCrit);
            Assert.AreEqual(225, results[0].Damage);
        }

        [Test]
        public void PhysicalDamage_ZeroDefense_FullDamage()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("phys_nodef", AttackType.PHYSICAL, 1.0f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 0f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(200, results[0].Damage);
        }

        #endregion

        #region DamageBase Variants (압도, 분쇄)

        [Test]
        public void PhysicalDamage_SourceMaxHpBase()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("apdo", AttackType.PHYSICAL, 0.1f, DamageBase.SOURCE_MAX_HP);

            var source = new MockUnit(
                maxHp: 1000, currentHp: 1000,
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 100f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(50, results[0].Damage);
        }

        [Test]
        public void PhysicalDamage_TargetMaxHpBase()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("bunsoe", AttackType.PHYSICAL, 0.05f, DamageBase.TARGET_MAX_HP);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 2000,
                getEffectiveDef: () => 100f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(50, results[0].Damage);
        }

        [Test]
        public void PhysicalDamage_SourceMaxHpBase_WithCritAndMastery()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("apdo_full", AttackType.PHYSICAL, 0.1f, DamageBase.SOURCE_MAX_HP);

            var source = new MockUnit(
                maxHp: 1000, currentHp: 1000,
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 1.0f,
                getMasteryBonus: id => id == "apdo_full" ? 0.5f : 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 100f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.IsTrue(results[0].IsCrit);
            Assert.AreEqual(112, results[0].Damage);
        }

        [Test]
        public void PhysicalDamage_SourceMaxHpBase_IgnoresAtk()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("apdo_atk", AttackType.PHYSICAL, 0.1f, DamageBase.SOURCE_MAX_HP);

            var source1 = new MockUnit(
                maxHp: 1000, currentHp: 1000,
                getEffectiveAtk: () => 100f,
                getEffectiveCrit: () => 0f);
            var source2 = new MockUnit(
                maxHp: 1000, currentHp: 1000,
                getEffectiveAtk: () => 999f,
                getEffectiveCrit: () => 0f);
            var target1 = new MockUnit(currentHp: 10000, maxHp: 10000, getEffectiveDef: () => 100f);
            var target2 = new MockUnit(currentHp: 10000, maxHp: 10000, getEffectiveDef: () => 100f);

            var r1 = engine.ExecuteSkillEffects(skill, source1, target1, new List<ActiveSkill> { skill });
            var r2 = engine.ExecuteSkillEffects(skill, source2, target2, new List<ActiveSkill> { skill });

            Assert.AreEqual(r1[0].Damage, r2[0].Damage);
        }

        #endregion

        #region Magic Damage Formula

        [Test]
        public void MagicDamage_WithDefense()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("magic", AttackType.MAGIC, 2.0f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f,
                magicCoefficient: 0.5f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 150f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(100, results[0].Damage);
        }

        [Test]
        public void MagicDamage_NeverCrits()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("magic_nocrit", AttackType.MAGIC, 1.0f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 1.0f,
                magicCoefficient: 0.5f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 0f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.IsFalse(results[0].IsCrit);
            Assert.AreEqual(100, results[0].Damage);
        }

        [Test]
        public void MagicDamage_WithMastery()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("magic_mastery", AttackType.MAGIC, 1.0f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f,
                magicCoefficient: 0.5f,
                getMasteryBonus: id => id == "magic_mastery" ? 0.5f : 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 0f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(150, results[0].Damage);
        }

        [Test]
        public void MagicDamage_ZeroDefense_FullDamage()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("magic_nodef", AttackType.MAGIC, 1.0f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f,
                magicCoefficient: 0.5f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 0f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(100, results[0].Damage);
        }

        #endregion

        #region Fixed Damage Formula

        [Test]
        public void FixedDamage_IgnoresDefense()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("fixed", AttackType.FIXED, 0.5f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 9999f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(100, results[0].Damage);
        }

        [Test]
        public void FixedDamage_NeverCrits()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("fixed_nocrit", AttackType.FIXED, 0.5f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 1.0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 0f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.IsFalse(results[0].IsCrit);
            Assert.AreEqual(100, results[0].Damage);
        }

        [Test]
        public void FixedDamage_WithMastery()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("fixed_mastery", AttackType.FIXED, 0.5f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f,
                getMasteryBonus: id => id == "fixed_mastery" ? 1.0f : 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 0f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(200, results[0].Damage);
        }

        #endregion

        #region Float Precision

        [Test]
        public void FloatChain_NoPrematureTruncation()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("float_test", AttackType.PHYSICAL, 1.5f);

            var source = new MockUnit(
                getEffectiveAtk: () => 33f,
                getEffectiveCrit: () => 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 70f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(29, results[0].Damage);
        }

        [Test]
        public void FloatChain_AnotherPrecisionCase()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("float_test2", AttackType.PHYSICAL, 1.7f);

            var source = new MockUnit(
                getEffectiveAtk: () => 57f,
                getEffectiveCrit: () => 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 100f);

            var results = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            float k = BattleDataTable.Data.Damage.DefenseConstant;
            float expected = 57f * 1.7f * 1.0f * 1.0f * (k / (k + 100f));
            Assert.AreEqual(Math.Max(1, (int)expected), results[0].Damage);
        }

        #endregion

        #region DOT Snapshot

        [Test]
        public void DotSnapshot_PhysicalCreatesPoison()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("phys_dot", AttackType.PHYSICAL, 1.0f, duration: 3);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 100f);

            engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(1, target.AppliedEffects.Count);
            var dot = target.AppliedEffects[0];
            Assert.AreEqual(StatusEffectType.POISON, dot.Type);
            Assert.AreEqual(3, dot.RemainingTurns);
            Assert.AreEqual(100, (int)dot.Value);
            Assert.AreEqual("phys_dot", dot.SourceSkillId);
        }

        [Test]
        public void DotSnapshot_MagicCreatesBurn()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("magic_dot", AttackType.MAGIC, 2.0f, duration: 2);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f,
                magicCoefficient: 0.5f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 150f);

            engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(1, target.AppliedEffects.Count);
            var dot = target.AppliedEffects[0];
            Assert.AreEqual(StatusEffectType.BURN, dot.Type);
            Assert.AreEqual(2, dot.RemainingTurns);
            Assert.AreEqual(100, (int)dot.Value);
            Assert.AreEqual(AttackType.MAGIC, dot.AttackType);
        }

        [Test]
        public void DotSnapshot_FixedCreatesPoison()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("fixed_dot", AttackType.FIXED, 0.5f, duration: 4);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 100f);

            engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(1, target.AppliedEffects.Count);
            var dot = target.AppliedEffects[0];
            Assert.AreEqual(StatusEffectType.POISON, dot.Type);
            Assert.AreEqual(4, dot.RemainingTurns);
            Assert.AreEqual(100, (int)dot.Value);
        }

        [Test]
        public void DotSnapshot_StoresDamageBeforeDefenseIsNotReapplied()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("dot_snapshot", AttackType.MAGIC, 2.0f, duration: 3);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f,
                magicCoefficient: 0.5f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 150f);

            engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            var dot = target.AppliedEffects[0];
            Assert.AreEqual(100, (int)dot.Value);
            Assert.AreEqual(dot.GetDamagePerTurn(), dot.Value);
        }

        #endregion

        #region Combined Scenarios

        [Test]
        public void CombinedScenario_NormalAttackPlusUpperSkills()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));

            var normalAttack = new ActiveSkill(
                "ilban_attack", "일반 공격", "X",
                SkillHierarchy.BUILTIN, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect
                {
                    Type = SkillEffectType.ATTACK,
                    AttackType = AttackType.PHYSICAL,
                    Coefficient = 1.0f,
                    DamageBase = DamageBase.ATK,
                }});

            var apdo = new ActiveSkill(
                "max_hp_damage", "압도", "X",
                SkillHierarchy.UPPER, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                new[] { new ActiveSkillEffect
                {
                    Type = SkillEffectType.ATTACK,
                    AttackType = AttackType.PHYSICAL,
                    Coefficient = 0.1f,
                    DamageBase = DamageBase.SOURCE_MAX_HP,
                }});

            var bunsoe = new ActiveSkill(
                "hp_crush", "분쇄", "X",
                SkillHierarchy.UPPER, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                new[] { new ActiveSkillEffect
                {
                    Type = SkillEffectType.ATTACK,
                    AttackType = AttackType.PHYSICAL,
                    Coefficient = 0.05f,
                    DamageBase = DamageBase.TARGET_MAX_HP,
                }});

            var source = new MockUnit(
                maxHp: 1000, currentHp: 1000,
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 2000,
                getEffectiveDef: () => 100f);

            var allSkills = new List<ActiveSkill> { normalAttack, apdo, bunsoe };

            var normalResults = engine.ExecuteSkillEffects(normalAttack, source, target, allSkills);
            Assert.AreEqual(1, normalResults.Count);
            Assert.AreEqual(100, normalResults[0].Damage);
            Assert.AreEqual("ilban_attack", normalResults[0].SkillId);

            var apdoResults = engine.ExecuteSkillEffects(apdo, source, target, allSkills);
            Assert.AreEqual(1, apdoResults.Count);
            Assert.AreEqual(50, apdoResults[0].Damage);
            Assert.AreEqual("max_hp_damage", apdoResults[0].SkillId);

            var bunsoeResults = engine.ExecuteSkillEffects(bunsoe, source, target, allSkills);
            Assert.AreEqual(1, bunsoeResults.Count);
            Assert.AreEqual(50, bunsoeResults[0].Damage);
            Assert.AreEqual("hp_crush", bunsoeResults[0].SkillId);

            int totalDamage = normalResults[0].Damage + apdoResults[0].Damage + bunsoeResults[0].Damage;
            Assert.AreEqual(200, totalDamage);
        }

        [Test]
        public void CombinedScenario_UpperSkillsAreIndependentFromNormalAttack()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));

            var normalAttack = new ActiveSkill(
                "ilban_attack", "일반 공격", "X",
                SkillHierarchy.BUILTIN, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect
                {
                    Type = SkillEffectType.ATTACK,
                    AttackType = AttackType.PHYSICAL,
                    Coefficient = 1.0f,
                }});

            var apdo = new ActiveSkill(
                "max_hp_damage", "압도", "X",
                SkillHierarchy.UPPER, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                new[] { new ActiveSkillEffect
                {
                    Type = SkillEffectType.ATTACK,
                    AttackType = AttackType.PHYSICAL,
                    Coefficient = 0.1f,
                    DamageBase = DamageBase.SOURCE_MAX_HP,
                }});

            var weakSource = new MockUnit(
                maxHp: 1000, currentHp: 1000,
                getEffectiveAtk: () => 50f,
                getEffectiveCrit: () => 0f);
            var strongSource = new MockUnit(
                maxHp: 1000, currentHp: 1000,
                getEffectiveAtk: () => 500f,
                getEffectiveCrit: () => 0f);
            var target1 = new MockUnit(currentHp: 10000, maxHp: 10000, getEffectiveDef: () => 100f);
            var target2 = new MockUnit(currentHp: 10000, maxHp: 10000, getEffectiveDef: () => 100f);

            var allSkills = new List<ActiveSkill> { normalAttack, apdo };

            var weakApdo = engine.ExecuteSkillEffects(apdo, weakSource, target1, allSkills);
            var strongApdo = engine.ExecuteSkillEffects(apdo, strongSource, target2, allSkills);

            Assert.AreEqual(weakApdo[0].Damage, strongApdo[0].Damage);
        }

        [Test]
        public void CombinedScenario_PhysicalMagicFixedAllInOneSkill()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));

            var multiSkill = new ActiveSkill(
                "multi", "Multi Attack", "X",
                SkillHierarchy.LOWER, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[]
                {
                    new ActiveSkillEffect
                    {
                        Type = SkillEffectType.ATTACK,
                        AttackType = AttackType.PHYSICAL,
                        Coefficient = 1.0f,
                    },
                    new ActiveSkillEffect
                    {
                        Type = SkillEffectType.ATTACK,
                        AttackType = AttackType.MAGIC,
                        Coefficient = 1.0f,
                    },
                    new ActiveSkillEffect
                    {
                        Type = SkillEffectType.ATTACK,
                        AttackType = AttackType.FIXED,
                        Coefficient = 0.5f,
                    },
                });

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f,
                magicCoefficient: 0.5f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 100f);

            var results = engine.ExecuteSkillEffects(multiSkill, source, target, new List<ActiveSkill> { multiSkill });

            Assert.AreEqual(3, results.Count);

            Assert.AreEqual(AttackType.PHYSICAL, results[0].AttackType);
            Assert.AreEqual(100, results[0].Damage);

            float kMagic = BattleDataTable.Data.Damage.MagicDefenseConstant;
            int expectedMagic = Math.Max(1, (int)(200f * 0.5f * 1.0f * (kMagic / (kMagic + 100f))));
            Assert.AreEqual(AttackType.MAGIC, results[1].AttackType);
            Assert.AreEqual(expectedMagic, results[1].Damage);

            Assert.AreEqual(AttackType.FIXED, results[2].AttackType);
            Assert.AreEqual(100, results[2].Damage);
        }

        [Test]
        public void CombinedScenario_MultiHitVerifiesConsistentDamage()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("multi_hit", AttackType.PHYSICAL, 1.0f);

            var source = new MockUnit(
                getEffectiveAtk: () => 200f,
                getEffectiveCrit: () => 0f);
            var target = new MockUnit(
                currentHp: 10000, maxHp: 10000,
                getEffectiveDef: () => 100f);

            var r1 = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });
            var r2 = engine.ExecuteSkillEffects(skill, source, target, new List<ActiveSkill> { skill });

            Assert.AreEqual(r1[0].Damage, r2[0].Damage);
            Assert.AreEqual(100, r1[0].Damage);
        }

        #endregion

        #region Defense Formula Verification

        [Test]
        public void PercentageDefenseFormulaPhysicalDamage()
        {
            var engine = new SkillExecutionEngine(new SeededRandom(42));
            var skill = MakeAttackSkill("test_phys", AttackType.PHYSICAL, 1.0f);

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
            var skill = MakeAttackSkill("test_magic_def", AttackType.MAGIC, 0.5f);

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

        #endregion
    }
}
