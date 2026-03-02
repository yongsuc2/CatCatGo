using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Battle
{
    public interface ISkillExecutionUnit
    {
        string Name { get; }
        int CurrentHp { get; }
        int MaxHp { get; }
        int GetEffectiveAtk();
        int GetEffectiveDef();
        float GetEffectiveCrit();
        int Rage { get; set; }
        int MaxRage { get; set; }
        int RagePerAttack { get; set; }
        float MagicCoefficient { get; set; }
        int TakeDamage(int amount);
        int Heal(int amount);
        void AddStatusEffect(StatusEffect effect);
        bool IsAlive();
        float GetHpPercent();
        HashSet<string> UsedOnceConditions { get; set; }
        float GetSkillDamageMultiplier(SkillTag[] tags);
        int GetHpBonusDamage();
    }

    public class SkillDamageResult
    {
        public string SkillName;
        public string SkillId;
        public string SkillIcon;
        public int Damage;
        public bool IsCrit;
        public int HealAmount;
        public int RageChange;
        public bool DebuffApplied;
        public string TargetName;
        public AttackType AttackType;
    }

    public class SkillExecutionEngine
    {
        private SeededRandom _rng;
        private Dictionary<string, List<ActiveSkillEffect>> _resolvedEffectsMap = new Dictionary<string, List<ActiveSkillEffect>>();

        public SkillExecutionEngine(SeededRandom rng)
        {
            _rng = rng;
        }

        public void ResolveInjections(List<ActiveSkill> skills)
        {
            _resolvedEffectsMap.Clear();

            foreach (var skill in skills)
            {
                var key = $"{skill.Id}:{skill.Tier}";
                _resolvedEffectsMap[key] = new List<ActiveSkillEffect>(skill.Effects);
            }

            foreach (var skill in skills)
            {
                foreach (var effect in skill.Effects)
                {
                    if (effect.Type == SkillEffectType.INJECT_EFFECT)
                    {
                        var targetSkill = skills.FirstOrDefault(s => s.Id == effect.TargetSkillId);
                        if (targetSkill == null) continue;
                        var targetKey = $"{targetSkill.Id}:{targetSkill.Tier}";
                        if (!_resolvedEffectsMap.TryGetValue(targetKey, out var existing))
                        {
                            existing = new List<ActiveSkillEffect>();
                            _resolvedEffectsMap[targetKey] = existing;
                        }
                        if (effect.InjectedEffects != null)
                            existing.AddRange(effect.InjectedEffects);
                    }
                }
            }
        }

        public List<ActiveSkillEffect> GetResolvedEffects(ActiveSkill skill)
        {
            var key = $"{skill.Id}:{skill.Tier}";
            if (_resolvedEffectsMap.TryGetValue(key, out var resolved))
                return resolved;
            return new List<ActiveSkillEffect>(skill.Effects);
        }

        public bool EvaluateTrigger(
            CompoundTrigger trigger,
            int turnCount,
            ISkillExecutionUnit source,
            string activatedSkillId = null)
        {
            var c1 = trigger.Condition1;
            if (c1.Kind == TriggerCondition1Kind.EVERY_N_TURNS)
            {
                if (turnCount % c1.Interval != 0) return false;
            }
            else if (c1.Kind == TriggerCondition1Kind.ON_SKILL_ACTIVATION)
            {
                if (activatedSkillId != c1.SkillId) return false;
            }

            if (_rng.Next() > trigger.Condition2.Probability) return false;

            var c3 = trigger.Condition3;
            switch (c3.Type)
            {
                case SpecialConditionType.NONE:
                    break;
                case SpecialConditionType.RAGE_FULL:
                    if (source.Rage < source.MaxRage) return false;
                    break;
                case SpecialConditionType.HP_BELOW:
                    if (source.GetHpPercent() > c3.Threshold) return false;
                    break;
                case SpecialConditionType.HP_ABOVE:
                    if (source.GetHpPercent() < c3.Threshold) return false;
                    break;
                case SpecialConditionType.HP_BELOW_ONCE:
                    if (source.GetHpPercent() > c3.Threshold) return false;
                    if (source.UsedOnceConditions.Contains("HP_BELOW_ONCE")) return false;
                    source.UsedOnceConditions.Add("HP_BELOW_ONCE");
                    break;
            }

            return true;
        }

        public List<SkillDamageResult> ExecuteSkillEffects(
            ActiveSkill skill,
            ISkillExecutionUnit source,
            ISkillExecutionUnit target,
            List<ActiveSkill> allSkills,
            int depth = 0,
            List<ISkillExecutionUnit> allTargets = null)
        {
            if (depth >= SkillValidator.MAX_SKILL_CHAIN_DEPTH) return new List<SkillDamageResult>();
            if (!target.IsAlive()) return new List<SkillDamageResult>();

            var results = new List<SkillDamageResult>();
            var effects = GetResolvedEffects(skill);

            foreach (var effect in effects)
            {
                if (!target.IsAlive() && !(effect.Type == SkillEffectType.ATTACK && effect.IsAoe)) break;

                switch (effect.Type)
                {
                    case SkillEffectType.ATTACK:
                    {
                        var targets = (effect.IsAoe && allTargets != null)
                            ? allTargets.Where(t => t.IsAlive()).ToList()
                            : new List<ISkillExecutionUnit> { target };

                        foreach (var t in targets)
                        {
                            var calcResult = CalculateSkillDamage(
                                source, t, effect.AttackType, effect.Coefficient, effect.IsTargetHpBased);
                            var tagMult = source.GetSkillDamageMultiplier(skill.Tags);
                            int damage = Math.Max(1, (int)(calcResult.Damage * tagMult));
                            int dealt = t.TakeDamage(damage);

                            results.Add(new SkillDamageResult
                            {
                                SkillName = skill.Name,
                                SkillId = skill.Id,
                                SkillIcon = skill.Icon,
                                Damage = dealt,
                                IsCrit = calcResult.IsCrit,
                                HealAmount = 0,
                                RageChange = 0,
                                DebuffApplied = false,
                                TargetName = (effect.IsAoe && allTargets != null) ? t.Name : null,
                                AttackType = effect.AttackType,
                            });

                            if (effect.Duration > 0)
                            {
                                int dotDamage;
                                if (effect.AttackType == AttackType.MAGIC)
                                {
                                    int dotAtk = source.GetEffectiveAtk();
                                    int dotDef = t.GetEffectiveDef();
                                    float mk = BattleDataTable.Data.Damage.MagicDefenseConstant;
                                    dotDamage = Math.Max(1, (int)(dotAtk * source.MagicCoefficient * effect.Coefficient * (mk / (mk + dotDef))));
                                }
                                else
                                {
                                    dotDamage = (int)(source.GetEffectiveAtk() * effect.Coefficient);
                                }
                                var dotType = effect.AttackType == AttackType.MAGIC
                                    ? StatusEffectType.BURN : StatusEffectType.POISON;
                                t.AddStatusEffect(new StatusEffect(dotType, effect.Duration, dotDamage, skill.Id));
                            }
                        }
                        break;
                    }

                    case SkillEffectType.TRIGGER_SKILL:
                    {
                        bool shouldTrigger = true;
                        if (effect.TriggerConditions != null)
                        {
                            shouldTrigger = EvaluateTrigger(
                                effect.TriggerConditions, 1, source, skill.Id);
                        }
                        if (shouldTrigger)
                        {
                            var childSkill = allSkills.FirstOrDefault(s => s.Id == effect.TargetSkillId);
                            if (childSkill != null)
                            {
                                for (int i = 0; i < effect.Count; i++)
                                {
                                    if (!target.IsAlive() && (allTargets == null || !allTargets.Any(t => t.IsAlive()))) break;
                                    var childResults = ExecuteSkillEffects(
                                        childSkill, source, target, allSkills, depth + 1, allTargets);
                                    results.AddRange(childResults);
                                }
                            }
                        }
                        break;
                    }

                    case SkillEffectType.HEAL_HP:
                    {
                        int healAmount = (int)(source.MaxHp * effect.Coefficient);
                        int healed = source.Heal(healAmount);
                        if (healed > 0)
                        {
                            results.Add(new SkillDamageResult
                            {
                                SkillName = skill.Name,
                                SkillId = skill.Id,
                                SkillIcon = skill.Icon,
                                Damage = 0,
                                IsCrit = false,
                                HealAmount = healed,
                                RageChange = 0,
                                DebuffApplied = false,
                            });
                        }
                        break;
                    }

                    case SkillEffectType.ADD_RAGE:
                    {
                        int rageAmount = effect.UseSourceStat ? source.RagePerAttack : effect.Amount;
                        source.Rage = source.Rage + rageAmount;
                        results.Add(new SkillDamageResult
                        {
                            SkillName = skill.Name,
                            SkillId = skill.Id,
                            SkillIcon = skill.Icon,
                            Damage = 0,
                            IsCrit = false,
                            HealAmount = 0,
                            RageChange = rageAmount,
                            DebuffApplied = false,
                        });
                        break;
                    }

                    case SkillEffectType.CONSUME_RAGE:
                    {
                        source.Rage = Math.Max(0, source.Rage - effect.Amount);
                        break;
                    }

                    case SkillEffectType.DEBUFF:
                    {
                        StatusEffectType seType;
                        switch (effect.Stat)
                        {
                            case StatType.ATK: seType = StatusEffectType.ATK_DOWN; break;
                            case StatType.DEF: seType = StatusEffectType.DEF_DOWN; break;
                            default: seType = StatusEffectType.ATK_DOWN; break;
                        }
                        target.AddStatusEffect(new StatusEffect(seType, effect.Duration, effect.Reduction));
                        results.Add(new SkillDamageResult
                        {
                            SkillName = skill.Name,
                            SkillId = skill.Id,
                            SkillIcon = skill.Icon,
                            Damage = 0,
                            IsCrit = false,
                            HealAmount = 0,
                            RageChange = 0,
                            DebuffApplied = true,
                        });
                        break;
                    }

                    case SkillEffectType.STUN:
                    {
                        if (target.IsAlive() && _rng.Chance(effect.Chance))
                        {
                            target.AddStatusEffect(new StatusEffect(StatusEffectType.STUN, effect.Duration, 0));
                            results.Add(new SkillDamageResult
                            {
                                SkillName = skill.Name,
                                SkillId = skill.Id,
                                SkillIcon = skill.Icon,
                                Damage = 0,
                                IsCrit = false,
                                HealAmount = 0,
                                RageChange = 0,
                                DebuffApplied = true,
                            });
                        }
                        break;
                    }

                    case SkillEffectType.INJECT_EFFECT:
                        break;
                }
            }

            return results;
        }

        private DamageCalcResult CalculateSkillDamage(
            ISkillExecutionUnit source,
            ISkillExecutionUnit target,
            AttackType attackType,
            float coefficient,
            bool isTargetHpBased = false)
        {
            int rawDamage;

            switch (attackType)
            {
                case AttackType.PHYSICAL:
                {
                    int def = target.GetEffectiveDef();
                    float k = BattleDataTable.Data.Damage.DefenseConstant;
                    if (isTargetHpBased)
                    {
                        rawDamage = Math.Max(1, (int)(target.MaxHp * coefficient * (k / (k + def))));
                    }
                    else
                    {
                        int atk = source.GetEffectiveAtk() + source.GetHpBonusDamage();
                        rawDamage = Math.Max(1, (int)(atk * coefficient * (k / (k + def))));
                    }
                    break;
                }
                case AttackType.MAGIC:
                {
                    int atk = source.GetEffectiveAtk();
                    int def = target.GetEffectiveDef();
                    float k = BattleDataTable.Data.Damage.MagicDefenseConstant;
                    rawDamage = Math.Max(1, (int)(atk * source.MagicCoefficient * coefficient * (k / (k + def))));
                    break;
                }
                case AttackType.FIXED:
                {
                    rawDamage = (int)(source.GetEffectiveAtk() * coefficient);
                    break;
                }
                default:
                    rawDamage = 1;
                    break;
            }

            bool isCrit = attackType == AttackType.PHYSICAL && _rng.Chance(source.GetEffectiveCrit());
            float critMult = isCrit ? BattleDataTable.Data.Damage.CritMultiplier : 1.0f;

            return new DamageCalcResult
            {
                Damage = Math.Max(1, (int)(rawDamage * critMult)),
                IsCrit = isCrit,
            };
        }

        private struct DamageCalcResult
        {
            public int Damage;
            public bool IsCrit;
        }
    }
}
