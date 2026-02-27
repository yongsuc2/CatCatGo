using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Domain.Entities
{
    public struct SkillEffect
    {
        public EffectType Type;
        public float Value;
        public float Duration;
        public StatType? ScalingStat;
        public StatusEffectType? StatusEffectTypeValue;

        public SkillEffect(EffectType type, float value, float duration, StatType? scalingStat = null, StatusEffectType? statusEffectType = null)
        {
            Type = type;
            Value = value;
            Duration = duration;
            ScalingStat = scalingStat;
            StatusEffectTypeValue = statusEffectType;
        }
    }

    public class Skill
    {
        private static readonly Dictionary<int, SkillGrade> TIER_TO_GRADE = new Dictionary<int, SkillGrade>
        {
            { 1, SkillGrade.NORMAL },
            { 2, SkillGrade.LEGENDARY },
            { 3, SkillGrade.MYTHIC },
            { 4, SkillGrade.IMMORTAL },
        };

        private static readonly SkillGrade[] GRADE_ORDER = new[]
        {
            SkillGrade.NORMAL,
            SkillGrade.LEGENDARY,
            SkillGrade.MYTHIC,
            SkillGrade.IMMORTAL,
        };

        public string Id { get; }
        public string Name { get; }
        public string Icon { get; }
        public int Tier { get; }
        public SkillCategory Category { get; }
        public List<HeritageRoute> HeritageSynergy { get; }
        public SkillEffect Effect { get; }
        public TriggerCondition TriggerConditionValue { get; }
        public string Description { get; }

        public Skill(
            string id,
            string name,
            string icon,
            int tier,
            SkillCategory category,
            List<HeritageRoute> heritageSynergy,
            SkillEffect effect,
            TriggerCondition triggerCondition,
            string description = "")
        {
            Id = id;
            Name = name;
            Icon = icon;
            Tier = tier;
            Category = category;
            HeritageSynergy = heritageSynergy;
            Effect = effect;
            TriggerConditionValue = triggerCondition;
            Description = description;
        }

        public SkillGrade Grade
        {
            get
            {
                return TIER_TO_GRADE.TryGetValue(Tier, out var grade) ? grade : SkillGrade.NORMAL;
            }
        }

        public bool IsMaxTier()
        {
            return Tier >= 4;
        }

        public bool IsSynergyWith(HeritageRoute route)
        {
            return HeritageSynergy.Contains(route);
        }

        public bool IsGradeAtLeast(SkillGrade grade)
        {
            return System.Array.IndexOf(GRADE_ORDER, Grade) >= System.Array.IndexOf(GRADE_ORDER, grade);
        }
    }
}
