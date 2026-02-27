using System.Linq;
using NUnit.Framework;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Data;

namespace CatCatGo.Tests.Domain
{
    [TestFixture]
    public class SkillValidatorTests
    {
        [Test]
        public void ValidatesAllRegisteredActiveSkillsPassHierarchyRules()
        {
            var allSkills = ActiveSkillRegistry.GetAll();
            var errors = SkillValidator.ValidateSkillHierarchy(allSkills.ToArray());
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void RejectsLowestSkillWithTriggerSkill()
        {
            var badSkill = new ActiveSkill(
                "bad_lowest", "Bad Lowest", "X",
                SkillHierarchy.LOWEST, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "lightning_summon", Count = 1 } });
            var allSkills = ActiveSkillRegistry.GetAll().Concat(new[] { badSkill }).ToArray();
            var errors = SkillValidator.ValidateSkillHierarchy(allSkills);
            Assert.IsTrue(errors.Any(e => e.Contains("LOWEST skill cannot have TRIGGER_SKILL")));
        }

        [Test]
        public void RejectsUpperTriggeringUpper()
        {
            var badSkill = new ActiveSkill(
                "bad_upper", "Bad Upper", "X",
                SkillHierarchy.UPPER, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "thunder_strike", Count = 1 } });
            var allSkills = ActiveSkillRegistry.GetAll().Concat(new[] { badSkill }).ToArray();
            var errors = SkillValidator.ValidateSkillHierarchy(allSkills);
            Assert.IsTrue(errors.Any(e => e.Contains("cannot trigger")));
        }

        [Test]
        public void RejectsLowerTriggeringLower()
        {
            var badLower = new ActiveSkill(
                "bad_lower", "Bad Lower", "X",
                SkillHierarchy.LOWER, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "shuriken_summon", Count = 1 } });
            var allSkills = ActiveSkillRegistry.GetAll().Concat(new[] { badLower }).ToArray();
            var errors = SkillValidator.ValidateSkillHierarchy(allSkills);
            Assert.IsTrue(errors.Any(e => e.Contains("cannot trigger")));
        }

        [Test]
        public void RejectsSelfReferencingTrigger()
        {
            var selfRef = new ActiveSkill(
                "self_ref", "Self Ref", "X",
                SkillHierarchy.UPPER, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[] { new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "self_ref", Count = 1 } });
            var allSkills = ActiveSkillRegistry.GetAll().Concat(new[] { selfRef }).ToArray();
            var errors = SkillValidator.ValidateSkillHierarchy(allSkills);
            Assert.IsTrue(errors.Any(e => e.Contains("self-reference")));
        }

        [Test]
        public void RejectsInjectionThatViolatesHierarchy()
        {
            var badInject = new ActiveSkill(
                "bad_inject", "Bad Inject", "X",
                SkillHierarchy.UPPER, 1, new SkillTag[0], new HeritageRoute[0],
                TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                new[]
                {
                    new ActiveSkillEffect
                    {
                        Type = SkillEffectType.INJECT_EFFECT,
                        TargetSkillId = "shuriken_summon",
                        InjectedEffects = new System.Collections.Generic.List<ActiveSkillEffect>
                        {
                            new ActiveSkillEffect
                            {
                                Type = SkillEffectType.TRIGGER_SKILL,
                                TargetSkillId = "shuriken_summon",
                                Count = 1,
                            },
                        },
                    },
                });
            var allSkills = ActiveSkillRegistry.GetAll().Concat(new[] { badInject }).ToArray();
            var errors = SkillValidator.ValidateSkillHierarchy(allSkills);
            Assert.IsTrue(errors.Any(e => e.Contains("illegally triggers")));
        }

        [Test]
        public void AcceptsValidUpperLowerLowestChain()
        {
            var allSkills = ActiveSkillRegistry.GetAll();
            var thunderShuriken = allSkills.FirstOrDefault(s => s.Id == "thunder_shuriken" && s.Tier == 1);
            Assert.IsNotNull(thunderShuriken);
            Assert.AreEqual(SkillHierarchy.UPPER, thunderShuriken.Hierarchy);

            var shurikenSummon = allSkills.FirstOrDefault(s => s.Id == "shuriken_summon" && s.Tier == 1);
            Assert.IsNotNull(shurikenSummon);
            Assert.AreEqual(SkillHierarchy.LOWER, shurikenSummon.Hierarchy);

            var lightningSummon = allSkills.FirstOrDefault(s => s.Id == "lightning_summon" && s.Tier == 1);
            Assert.IsNotNull(lightningSummon);
            Assert.AreEqual(SkillHierarchy.LOWEST, lightningSummon.Hierarchy);
        }

        [Test]
        public void MaxSkillChainDepthIs3()
        {
            Assert.AreEqual(3, SkillValidator.MAX_SKILL_CHAIN_DEPTH);
        }
    }

    [TestFixture]
    public class ActiveSkillRegistryTests
    {
        [Test]
        public void GeneratesSkillsForAllFamiliesWithTiers()
        {
            var all = ActiveSkillRegistry.GetAll();
            Assert.Greater(all.Count, 0);

            var thunderShuriken1 = ActiveSkillRegistry.GetById("thunder_shuriken", 1);
            Assert.IsNotNull(thunderShuriken1);
            Assert.AreEqual("\ubc88\uac1c \uc218\ub9ac\uac80", thunderShuriken1.Name);
            Assert.AreEqual(1, thunderShuriken1.Tier);

            var thunderShuriken2 = ActiveSkillRegistry.GetById("thunder_shuriken", 2);
            Assert.IsNotNull(thunderShuriken2);
            Assert.AreEqual("\ubc88\uac1c \uc218\ub9ac\uac80 II", thunderShuriken2.Name);
            Assert.AreEqual(2, thunderShuriken2.Tier);
        }

        [Test]
        public void ReturnsBuiltinSkills()
        {
            var builtins = ActiveSkillRegistry.GetBuiltinSkills();
            Assert.AreEqual(2, builtins.Count);
            var ids = builtins.Select(s => s.Id).ToList();
            Assert.IsTrue(ids.Contains("ilban_attack"));
            Assert.IsTrue(ids.Contains("bunno_attack"));
        }

        [Test]
        public void ReturnsUpperTier1Skills()
        {
            var uppers = ActiveSkillRegistry.GetUpperTier1Skills();
            Assert.Greater(uppers.Count, 0);
            foreach (var s in uppers)
            {
                Assert.AreEqual(SkillHierarchy.UPPER, s.Hierarchy);
                Assert.AreEqual(1, s.Tier);
            }
        }

        [Test]
        public void GetNextTierReturnsCorrectNextTier()
        {
            var next = ActiveSkillRegistry.GetNextTier("thunder_shuriken", 1);
            Assert.IsNotNull(next);
            Assert.AreEqual(2, next.Tier);

            var max = ActiveSkillRegistry.GetNextTier("thunder_shuriken", 4);
            Assert.IsNull(max);
        }

        [Test]
        public void IlbanAttackHasAttackAndTriggerSkillEffects()
        {
            var ilban = ActiveSkillRegistry.GetById("ilban_attack", 1);
            Assert.IsNotNull(ilban);
            Assert.AreEqual(2, ilban.Effects.Length);
            Assert.AreEqual(SkillEffectType.ATTACK, ilban.Effects[0].Type);
            Assert.AreEqual(SkillEffectType.TRIGGER_SKILL, ilban.Effects[1].Type);
            Assert.AreEqual(AttackType.PHYSICAL, ilban.Effects[0].AttackType);
            Assert.AreEqual(1.0f, ilban.Effects[0].Coefficient);
        }

        [Test]
        public void BunnoAttackHasConsumeRageAndAttack()
        {
            var bunno = ActiveSkillRegistry.GetById("bunno_attack", 1);
            Assert.IsNotNull(bunno);
            Assert.AreEqual(SkillEffectType.CONSUME_RAGE, bunno.Effects[0].Type);
            Assert.AreEqual(SkillEffectType.ATTACK, bunno.Effects[1].Type);
        }

        [Test]
        public void ThunderShurikenHasTriggerSkillAndInjectEffect()
        {
            var ts = ActiveSkillRegistry.GetById("thunder_shuriken", 1);
            Assert.IsNotNull(ts);
            var triggerEffect = ts.Effects.FirstOrDefault(e => e.Type == SkillEffectType.TRIGGER_SKILL);
            Assert.IsNotNull(triggerEffect);
            Assert.AreEqual("shuriken_summon", triggerEffect.TargetSkillId);

            var injectEffect = ts.Effects.FirstOrDefault(e => e.Type == SkillEffectType.INJECT_EFFECT);
            Assert.IsNotNull(injectEffect);
            Assert.AreEqual("shuriken_summon", injectEffect.TargetSkillId);
            Assert.Greater(injectEffect.InjectedEffects.Count, 0);
        }

        [Test]
        public void DemonPowerIsSpecialAndOnlyHasTier4()
        {
            Assert.IsTrue(ActiveSkillRegistry.IsSpecialSkill("demon_power"));
            var dp1 = ActiveSkillRegistry.GetById("demon_power", 1);
            Assert.IsNull(dp1);
            var dp4 = ActiveSkillRegistry.GetById("demon_power", 4);
            Assert.IsNotNull(dp4);
        }
    }

    [TestFixture]
    public class PassiveSkillRegistryTests
    {
        [Test]
        public void GeneratesSkillsForAllFamiliesWithTiers()
        {
            var all = PassiveSkillRegistry.GetAll();
            Assert.Greater(all.Count, 0);

            var lifesteal1 = PassiveSkillRegistry.GetById("lifesteal", 1);
            Assert.IsNotNull(lifesteal1);
            Assert.AreEqual(PassiveType.LIFESTEAL, lifesteal1.Effect.Type);

            var lifesteal4 = PassiveSkillRegistry.GetById("lifesteal", 4);
            Assert.IsNotNull(lifesteal4);
            Assert.AreEqual(0.48f, lifesteal4.Effect.Rate, 0.01f);
        }

        [Test]
        public void ReturnsTier1SkillsExcludingSpecials()
        {
            var tier1 = PassiveSkillRegistry.GetTier1Skills();
            Assert.Greater(tier1.Count, 0);
            foreach (var s in tier1)
            {
                Assert.AreEqual(1, s.Tier);
                Assert.IsFalse(PassiveSkillRegistry.IsSpecialSkill(s.Id));
            }
        }

        [Test]
        public void AngelPowerIsSpecialAndOnlyHasTier4()
        {
            Assert.IsTrue(PassiveSkillRegistry.IsSpecialSkill("angel_power"));
            var ap1 = PassiveSkillRegistry.GetById("angel_power", 1);
            Assert.IsNull(ap1);
            var ap4 = PassiveSkillRegistry.GetById("angel_power", 4);
            Assert.IsNotNull(ap4);
        }

        [Test]
        public void ReviveHasCorrectPassiveEffect()
        {
            var revive = PassiveSkillRegistry.GetById("revive", 1);
            Assert.IsNotNull(revive);
            Assert.AreEqual(PassiveType.REVIVE, revive.Effect.Type);
            Assert.AreEqual(0.17f, revive.Effect.HpPercent, 0.01f);
            Assert.AreEqual(1, revive.Effect.MaxUses);
        }

        [Test]
        public void CounterHasCorrectPassiveEffect()
        {
            var counter = PassiveSkillRegistry.GetById("counter", 1);
            Assert.IsNotNull(counter);
            Assert.AreEqual(PassiveType.COUNTER, counter.Effect.Type);
            Assert.AreEqual(0.1f, counter.Effect.TriggerChance, 0.01f);
        }

        [Test]
        public void GetNextTierWorksForPassives()
        {
            var next = PassiveSkillRegistry.GetNextTier("lifesteal", 1);
            Assert.IsNotNull(next);
            Assert.AreEqual(2, next.Tier);
        }

        [Test]
        public void AllStatModifierPassivesHaveCorrectTypes()
        {
            var statMods = new[] { "crit_mastery", "rage_mastery" };
            foreach (var id in statMods)
            {
                var skill = PassiveSkillRegistry.GetById(id, 1);
                Assert.IsNotNull(skill);
                Assert.AreEqual(PassiveType.STAT_MODIFIER, skill.Effect.Type);
            }
        }
    }

    [TestFixture]
    public class TierScalingTests
    {
        [Test]
        public void HigherTiersHaveBetterValuesForActiveSkills()
        {
            for (int tier = 1; tier < 4; tier++)
            {
                var ls1 = ActiveSkillRegistry.GetById("lightning_summon", tier);
                var ls2 = ActiveSkillRegistry.GetById("lightning_summon", tier + 1);
                if (ls1 != null && ls2 != null)
                {
                    var coeff1 = ls1.Effects.FirstOrDefault(e => e.Type == SkillEffectType.ATTACK);
                    var coeff2 = ls2.Effects.FirstOrDefault(e => e.Type == SkillEffectType.ATTACK);
                    if (coeff1 != null && coeff2 != null)
                    {
                        Assert.Greater(coeff2.Coefficient, coeff1.Coefficient);
                    }
                }
            }
        }

        [Test]
        public void HigherTiersHaveBetterValuesForPassiveSkills()
        {
            var ls1 = PassiveSkillRegistry.GetById("lifesteal", 1);
            var ls4 = PassiveSkillRegistry.GetById("lifesteal", 4);
            Assert.IsNotNull(ls1);
            Assert.IsNotNull(ls4);
            Assert.Greater(ls4.Effect.Rate, ls1.Effect.Rate);
        }
    }
}
