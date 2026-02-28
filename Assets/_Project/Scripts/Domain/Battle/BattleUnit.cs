using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Data;

namespace CatCatGo.Domain.Battle
{
    public class BattleUnit : ISkillExecutionUnit
    {
        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }
        public int BaseAtk;
        public int BaseDef;
        public float BaseCrit;
        public List<ActiveSkill> ActiveSkills;
        public List<PassiveSkill> PassiveSkills;
        public List<StatusEffect> StatusEffects;
        public bool IsPlayer;
        public string Name { get; set; }
        public bool ReviveUsed;
        public float ReviveHpPercent;
        public float MultiHitChance;
        public float LifestealRate;
        public float CounterTriggerChance;
        public int Rage { get; set; }
        public int MaxRage { get; set; }
        public float MagicCoefficient { get; set; }
        public float RagePowerMultiplier;
        public int Shield;
        public HashSet<string> UsedOnceConditions { get; set; }
        public Dictionary<SkillTag, float> SkillTagBonuses;
        public List<LowHpModifier> LowHpModifiers;
        public float HpDamageCoefficient;
        public string TemplateId;

        public BattleUnit(
            string name,
            Stats stats,
            ActiveSkill[] activeSkills = null,
            PassiveSkill[] passiveSkills = null,
            bool isPlayer = true)
        {
            Name = name;
            CurrentHp = stats.Hp;
            MaxHp = stats.MaxHp;
            BaseAtk = stats.Atk;
            BaseDef = stats.Def;
            BaseCrit = stats.Crit;
            ActiveSkills = activeSkills != null ? new List<ActiveSkill>(activeSkills) : new List<ActiveSkill>();
            PassiveSkills = passiveSkills != null ? new List<PassiveSkill>(passiveSkills) : new List<PassiveSkill>();
            StatusEffects = new List<StatusEffect>();
            IsPlayer = isPlayer;
            ReviveUsed = false;
            ReviveHpPercent = 0;
            MultiHitChance = 0;
            LifestealRate = 0;
            CounterTriggerChance = 0;
            Rage = 0;
            MaxRage = BattleDataTable.Data.Rage.MaxRage;
            MagicCoefficient = BattleDataTable.Data.Damage.BaseMagicCoefficient;
            RagePowerMultiplier = 1.0f;
            Shield = 0;
            UsedOnceConditions = new HashSet<string>();
            SkillTagBonuses = new Dictionary<SkillTag, float>();
            LowHpModifiers = new List<LowHpModifier>();
            HpDamageCoefficient = 0;

            ApplyPassiveSkills();
        }

        private void ApplyPassiveSkills()
        {
            var statMods = PassiveSkills.Where(s => s.Effect.Type == PassiveType.STAT_MODIFIER).ToList();
            var others = PassiveSkills.Where(s => s.Effect.Type != PassiveType.STAT_MODIFIER).ToList();
            foreach (var skill in statMods) ApplyPassiveSkill(skill);
            foreach (var skill in others) ApplyPassiveSkill(skill);
        }

        private void ApplyPassiveSkill(PassiveSkill skill)
        {
            switch (skill.Effect.Type)
            {
                case PassiveType.STAT_MODIFIER:
                {
                    var stat = skill.Effect.Stat;
                    var value = skill.Effect.Value;
                    var isPercentage = skill.Effect.IsPercentage;
                    if (stat == StatType.ATK)
                    {
                        BaseAtk = isPercentage ? (int)(BaseAtk * (1 + value)) : BaseAtk + (int)value;
                    }
                    else if (stat == StatType.DEF)
                    {
                        BaseDef = isPercentage ? (int)(BaseDef * (1 + value)) : BaseDef + (int)value;
                    }
                    else if (stat == StatType.CRIT)
                    {
                        BaseCrit = Math.Min(1.0f, BaseCrit + value);
                    }
                    else if (stat == StatType.HP)
                    {
                        var oldMax = MaxHp;
                        MaxHp = isPercentage ? (int)(MaxHp * (1 + value)) : MaxHp + (int)value;
                        CurrentHp += (MaxHp - oldMax);
                    }
                    else if (stat == StatType.RAGE_POWER)
                    {
                        RagePowerMultiplier += value;
                    }
                    else if (stat == StatType.MAGIC_COEFFICIENT)
                    {
                        MagicCoefficient += value;
                    }
                    break;
                }
                case PassiveType.COUNTER:
                    CounterTriggerChance = Math.Max(CounterTriggerChance, skill.Effect.TriggerChance);
                    break;
                case PassiveType.LIFESTEAL:
                    LifestealRate += skill.Effect.Rate;
                    break;
                case PassiveType.SHIELD_ON_START:
                    Shield += (int)(MaxHp * skill.Effect.HpPercent);
                    break;
                case PassiveType.REVIVE:
                    ReviveHpPercent = skill.Effect.HpPercent;
                    break;
                case PassiveType.REGEN:
                    AddStatusEffect(new StatusEffect(StatusEffectType.REGEN, 999, skill.Effect.HealPerTurn));
                    break;
                case PassiveType.MULTI_HIT:
                    MultiHitChance += skill.Effect.Chance;
                    break;
                case PassiveType.SKILL_MODIFIER:
                {
                    var targetTag = skill.Effect.TargetTag;
                    var damageMultiplier = skill.Effect.DamageMultiplier;
                    if (targetTag.HasValue && damageMultiplier != 0)
                    {
                        SkillTagBonuses.TryGetValue(targetTag.Value, out var current);
                        SkillTagBonuses[targetTag.Value] = current + damageMultiplier;
                    }
                    break;
                }
                case PassiveType.LOW_HP_MODIFIER:
                {
                    if (skill.Effect.Stat.HasValue)
                        LowHpModifiers.Add(new LowHpModifier { Stat = skill.Effect.Stat.Value, MaxBonus = skill.Effect.MaxBonus });
                    break;
                }
                case PassiveType.MAX_HP_DAMAGE:
                {
                    HpDamageCoefficient += skill.Effect.Coefficient;
                    break;
                }
            }
        }

        private float GetLowHpBonus(StatType stat)
        {
            var hpRatio = GetHpPercent();
            if (hpRatio >= 0.5f) return 0;
            var r = (0.5f - hpRatio) / 0.5f;
            float bonus = 0;
            foreach (var mod in LowHpModifiers)
            {
                if (mod.Stat == stat) bonus += mod.MaxBonus * r * r;
            }
            return bonus;
        }

        public int GetHpBonusDamage()
        {
            if (HpDamageCoefficient <= 0) return 0;
            return (int)(MaxHp * HpDamageCoefficient);
        }

        public int GetEffectiveAtk()
        {
            int atk = BaseAtk;
            var lowHpBonus = GetLowHpBonus(StatType.ATK);
            if (lowHpBonus > 0) atk = (int)(atk * (1 + lowHpBonus));
            foreach (var effect in StatusEffects)
            {
                if (effect.Type == StatusEffectType.ATK_UP)
                    atk = (int)(atk * (1 + effect.Value));
                if (effect.Type == StatusEffectType.ATK_DOWN)
                    atk = (int)(atk * (1 - effect.Value));
            }
            return Math.Max(1, atk);
        }

        public int GetEffectiveDef()
        {
            int def = BaseDef;
            var lowHpBonus = GetLowHpBonus(StatType.DEF);
            if (lowHpBonus > 0) def = (int)(def * (1 + lowHpBonus));
            foreach (var effect in StatusEffects)
            {
                if (effect.Type == StatusEffectType.DEF_UP)
                    def = (int)(def * (1 + effect.Value));
                if (effect.Type == StatusEffectType.DEF_DOWN)
                    def = (int)(def * (1 - effect.Value));
            }
            return Math.Max(0, def);
        }

        public float GetEffectiveCrit()
        {
            float crit = BaseCrit;
            foreach (var effect in StatusEffects)
            {
                if (effect.Type == StatusEffectType.CRIT_UP)
                    crit += effect.Value;
            }
            return Math.Min(1.0f, crit);
        }

        public float GetSkillDamageMultiplier(SkillTag[] tags)
        {
            float bonus = 0;
            foreach (var tag in tags)
            {
                SkillTagBonuses.TryGetValue(tag, out var val);
                bonus += val;
            }
            return 1 + bonus;
        }

        public int TakeDamage(int amount)
        {
            int remaining = amount;
            int dealt = 0;
            if (Shield > 0)
            {
                int absorbed = Math.Min(remaining, Shield);
                Shield -= absorbed;
                remaining -= absorbed;
                dealt += absorbed;
            }
            int hpDmg = Math.Max(0, Math.Min(remaining, CurrentHp));
            CurrentHp -= hpDmg;
            dealt += hpDmg;
            return dealt;
        }

        public int Heal(int amount)
        {
            int actual = Math.Min(amount, MaxHp - CurrentHp);
            CurrentHp += actual;
            return actual;
        }

        public bool IsAlive()
        {
            return CurrentHp > 0;
        }

        public bool CanRevive()
        {
            return !ReviveUsed && ReviveHpPercent > 0;
        }

        public bool TryRevive()
        {
            if (!CanRevive()) return false;
            ReviveUsed = true;
            CurrentHp = (int)(MaxHp * ReviveHpPercent);
            return true;
        }

        public void AddStatusEffect(StatusEffect effect)
        {
            if (effect.IsDot())
            {
                if (effect.SourceSkillId != null)
                {
                    int existingIdx = StatusEffects.FindIndex(
                        e => e.IsDot() && e.SourceSkillId == effect.SourceSkillId);
                    if (existingIdx >= 0)
                    {
                        if (effect.Value >= StatusEffects[existingIdx].Value)
                            StatusEffects[existingIdx] = effect;
                        return;
                    }
                }
                StatusEffects.Add(effect);
                return;
            }
            var existing = StatusEffects.FirstOrDefault(e => e.Type == effect.Type);
            if (existing != null)
            {
                existing.RemainingTurns = Math.Max(existing.RemainingTurns, effect.RemainingTurns);
                return;
            }
            StatusEffects.Add(effect);
        }

        public TickResult TickStatusEffects()
        {
            int totalDamage = 0;
            int totalHeal = 0;

            foreach (var effect in StatusEffects)
            {
                totalDamage += (int)effect.GetDamagePerTurn();
                if (effect.IsHot())
                    totalHeal += (int)(MaxHp * effect.Value);
                effect.Tick();
            }

            if (totalDamage > 0) TakeDamage(totalDamage);
            if (totalHeal > 0) Heal(totalHeal);

            StatusEffects = StatusEffects.Where(e => !e.IsExpired()).ToList();

            return new TickResult { Damage = totalDamage, Heal = totalHeal };
        }

        public float GetHpPercent()
        {
            return MaxHp > 0 ? (float)CurrentHp / MaxHp : 0;
        }

        public List<ActiveSkill> GetBuiltinSkills()
        {
            return ActiveSkills.Where(s => s.Hierarchy == SkillHierarchy.BUILTIN).ToList();
        }

        public List<ActiveSkill> GetUpperSkills()
        {
            return ActiveSkills.Where(s => s.Hierarchy == SkillHierarchy.UPPER).ToList();
        }

        public List<ActiveSkill> GetAllSkillsForEngine()
        {
            return ActiveSkills;
        }
    }

    public struct LowHpModifier
    {
        public StatType Stat;
        public float MaxBonus;
    }

    public struct TickResult
    {
        public int Damage;
        public int Heal;
    }
}
