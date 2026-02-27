using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;

namespace CatCatGo.Domain.Battle
{
    public static class SkillValidator
    {
        public const int MAX_SKILL_CHAIN_DEPTH = 3;

        private static readonly Dictionary<SkillHierarchy, int> HierarchyOrder = new Dictionary<SkillHierarchy, int>
        {
            { SkillHierarchy.BUILTIN, 0 },
            { SkillHierarchy.UPPER, 1 },
            { SkillHierarchy.LOWER, 2 },
            { SkillHierarchy.LOWEST, 3 },
        };

        private static bool IsStrictlyLower(SkillHierarchy source, SkillHierarchy target)
        {
            return HierarchyOrder[target] > HierarchyOrder[source];
        }

        public static List<string> ValidateSkillHierarchy(ActiveSkill[] skills)
        {
            var errors = new List<string>();

            ActiveSkill GetSkill(string id)
            {
                return skills.FirstOrDefault(s => s.Id == id);
            }

            foreach (var skill in skills)
            {
                var key = $"{skill.Id}:t{skill.Tier}";

                foreach (var effect in skill.Effects)
                {
                    if (effect.Type == SkillEffectType.TRIGGER_SKILL)
                    {
                        var target = GetSkill(effect.TargetSkillId);
                        if (target == null)
                        {
                            errors.Add($"[{key}] triggers non-existent skill '{effect.TargetSkillId}'");
                            continue;
                        }
                        if (!IsStrictlyLower(skill.Hierarchy, target.Hierarchy))
                        {
                            errors.Add($"[{key}] ({skill.Hierarchy}) cannot trigger '{effect.TargetSkillId}' ({target.Hierarchy})");
                        }
                        if (effect.TargetSkillId == skill.Id)
                        {
                            errors.Add($"[{key}] self-reference: triggers itself");
                        }
                    }

                    if (effect.Type == SkillEffectType.INJECT_EFFECT)
                    {
                        var target = GetSkill(effect.TargetSkillId);
                        if (target == null)
                        {
                            errors.Add($"[{key}] injects into non-existent skill '{effect.TargetSkillId}'");
                            continue;
                        }

                        if (effect.TargetSkillId == skill.Id)
                        {
                            errors.Add($"[{key}] self-reference: injects into itself");
                        }

                        ValidateInjectedEffects(key, target, effect.InjectedEffects, GetSkill, errors);
                    }
                }

                if (skill.Hierarchy == SkillHierarchy.LOWEST)
                {
                    var hasTrigger = skill.Effects.Any(e => e.Type == SkillEffectType.TRIGGER_SKILL);
                    if (hasTrigger)
                    {
                        errors.Add($"[{key}] LOWEST skill cannot have TRIGGER_SKILL effects");
                    }
                }
            }

            return errors;
        }

        private static void ValidateInjectedEffects(
            string sourceKey,
            ActiveSkill targetSkill,
            List<ActiveSkillEffect> injectedEffects,
            System.Func<string, ActiveSkill> getSkill,
            List<string> errors)
        {
            if (injectedEffects == null) return;

            foreach (var injected in injectedEffects)
            {
                if (injected.Type == SkillEffectType.TRIGGER_SKILL)
                {
                    var subTarget = getSkill(injected.TargetSkillId);
                    if (subTarget == null)
                    {
                        errors.Add($"[{sourceKey}] injection: triggers non-existent '{injected.TargetSkillId}'");
                        continue;
                    }
                    if (!IsStrictlyLower(targetSkill.Hierarchy, subTarget.Hierarchy))
                    {
                        errors.Add($"[{sourceKey}] injection into '{targetSkill.Id}' ({targetSkill.Hierarchy}) illegally triggers '{injected.TargetSkillId}' ({subTarget.Hierarchy})");
                    }
                }
            }
        }
    }
}
