using System.Collections.Generic;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Domain.Entities
{
    public class PassiveEffect
    {
        public PassiveType Type;
        public StatType? Stat;
        public float Value;
        public bool IsPercentage;
        public float TriggerChance;
        public float Rate;
        public float HpPercent;
        public int MaxUses;
        public float HealPerTurn;
        public float Chance;
        public string TargetSkillId;
        public float CritBonus;
        public float DamageMultiplier;
        public float MaxBonus;
        public float Coefficient;
    }

    public class PassiveSkill : IHeritageSynergyProvider
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Icon;
        public readonly int Tier;
        public readonly SkillTag[] Tags;
        public readonly HeritageRoute[] HeritageSynergy;
        public readonly PassiveEffect Effect;
        public readonly string Description;

        public PassiveSkill(
            string id,
            string name,
            string icon,
            int tier,
            SkillTag[] tags,
            HeritageRoute[] heritageSynergy,
            PassiveEffect effect,
            string description = "")
        {
            Id = id;
            Name = name;
            Icon = icon;
            Tier = tier;
            Tags = tags;
            HeritageSynergy = heritageSynergy;
            Effect = effect;
            Description = description;
        }

        public SkillGrade Grade => SkillGradeHelper.GetGradeForTier(Tier);

        public bool IsMaxTier()
        {
            return Tier >= 4;
        }

        public bool HasHeritageSynergy(HeritageRoute route)
        {
            return System.Array.IndexOf(HeritageSynergy, route) >= 0;
        }
    }
}
