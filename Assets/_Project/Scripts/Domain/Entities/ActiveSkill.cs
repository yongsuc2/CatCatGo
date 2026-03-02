using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Domain.Entities
{
    public class TriggerCondition1
    {
        public TriggerCondition1Kind Kind;
        public int Interval;
        public string SkillId;
    }

    public class TriggerCondition2
    {
        public float Probability;
    }

    public class TriggerCondition3
    {
        public SpecialConditionType Type;
        public float Threshold;
    }

    public class CompoundTrigger
    {
        public TriggerCondition1 Condition1;
        public TriggerCondition2 Condition2;
        public TriggerCondition3 Condition3;
    }

    public class ActiveSkillEffect
    {
        public SkillEffectType Type;
        public AttackType AttackType;
        public float Coefficient;
        public int Duration;
        public bool IsAoe;
        public DamageBase DamageBase;
        public string TargetSkillId;
        public int Count;
        public CompoundTrigger TriggerConditions;
        public List<ActiveSkillEffect> InjectedEffects;
        public int Amount;
        public bool UseSourceStat;
        public StatType Stat;
        public float Reduction;
        public float Chance;
    }

    public class ActiveSkill : IHeritageSynergyProvider
    {
        private static readonly Dictionary<int, SkillGrade> TierToGrade = new Dictionary<int, SkillGrade>
        {
            { 1, SkillGrade.NORMAL },
            { 2, SkillGrade.LEGENDARY },
            { 3, SkillGrade.MYTHIC },
            { 4, SkillGrade.IMMORTAL },
        };

        public readonly string Id;
        public readonly string Name;
        public readonly string Icon;
        public readonly SkillHierarchy Hierarchy;
        public readonly int Tier;
        public readonly SkillTag[] Tags;
        public readonly HeritageRoute[] HeritageSynergy;
        public readonly CompoundTrigger Trigger;
        public readonly ActiveSkillEffect[] Effects;
        public readonly string Description;

        public ActiveSkill(
            string id,
            string name,
            string icon,
            SkillHierarchy hierarchy,
            int tier,
            SkillTag[] tags,
            HeritageRoute[] heritageSynergy,
            CompoundTrigger trigger,
            ActiveSkillEffect[] effects,
            string description = "")
        {
            Id = id;
            Name = name;
            Icon = icon;
            Hierarchy = hierarchy;
            Tier = tier;
            Tags = tags;
            HeritageSynergy = heritageSynergy;
            Trigger = trigger;
            Effects = effects;
            Description = description;
        }

        public SkillGrade Grade =>
            TierToGrade.TryGetValue(Tier, out var g) ? g : SkillGrade.NORMAL;

        public bool IsMaxTier()
        {
            return Tier >= 4;
        }

        public bool HasTag(SkillTag tag)
        {
            return Tags.Contains(tag);
        }

        public bool IsSynergyWith(HeritageRoute route)
        {
            return System.Array.IndexOf(HeritageSynergy, route) >= 0;
        }

        public bool HasHeritageSynergy(HeritageRoute route)
        {
            return IsSynergyWith(route);
        }
    }

    public static class TriggerFactory
    {
        public static TriggerCondition1 EveryNTurns(int interval)
        {
            return new TriggerCondition1
            {
                Kind = TriggerCondition1Kind.EVERY_N_TURNS,
                Interval = interval,
            };
        }

        public static TriggerCondition1 OnSkillActivation(string skillId)
        {
            return new TriggerCondition1
            {
                Kind = TriggerCondition1Kind.ON_SKILL_ACTIVATION,
                SkillId = skillId,
            };
        }

        public static TriggerCondition2 Prob(float probability)
        {
            return new TriggerCondition2 { Probability = probability };
        }

        public static TriggerCondition3 NoCondition()
        {
            return new TriggerCondition3 { Type = SpecialConditionType.NONE };
        }

        public static TriggerCondition3 RageFull()
        {
            return new TriggerCondition3 { Type = SpecialConditionType.RAGE_FULL };
        }

        public static TriggerCondition3 HpBelow(float threshold)
        {
            return new TriggerCondition3
            {
                Type = SpecialConditionType.HP_BELOW,
                Threshold = threshold,
            };
        }

        public static TriggerCondition3 HpAbove(float threshold)
        {
            return new TriggerCondition3
            {
                Type = SpecialConditionType.HP_ABOVE,
                Threshold = threshold,
            };
        }

        public static TriggerCondition3 HpBelowOnce(float threshold)
        {
            return new TriggerCondition3
            {
                Type = SpecialConditionType.HP_BELOW_ONCE,
                Threshold = threshold,
            };
        }

        public static CompoundTrigger Trigger(
            TriggerCondition1 c1,
            TriggerCondition2 c2 = null,
            TriggerCondition3 c3 = null)
        {
            return new CompoundTrigger
            {
                Condition1 = c1,
                Condition2 = c2 ?? Prob(1.0f),
                Condition3 = c3 ?? NoCondition(),
            };
        }
    }
}
