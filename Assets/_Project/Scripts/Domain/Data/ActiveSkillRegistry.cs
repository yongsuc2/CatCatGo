using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;

namespace CatCatGo.Domain.Data
{
    public class ActiveSkillFamilyDef
    {
        public string Id;
        public string Name;
        public string Icon;
        public SkillHierarchy Hierarchy;
        public SkillTag[] Tags;
        public HeritageRoute[] HeritageSynergy;
        public string[] Traits;
        public Func<int, CompoundTrigger> BuildTrigger;
        public Func<int, ActiveSkillEffect[]> BuildEffects;
        public Func<int, string> BuildDescription;
    }

    public static class ActiveSkillRegistry
    {
        private static List<ActiveSkill> _allSkills;
        private static HashSet<string> _builtinIds;
        private static HashSet<string> _specialIds;
        private static string[] _upperFamilyIds;

        private static Dictionary<string, float> Td(string id, int tier)
        {
            return ActiveSkillTierData.GetTierData(id, tier) ?? new Dictionary<string, float>();
        }

        private static string Pct(float v) => SkillRegistryHelper.Pct(v);
        private static float V(Dictionary<string, float> d, string key) => SkillRegistryHelper.V(d, key);

        private static ActiveSkillFamilyDef[] BuildFamilies()
        {
            return new ActiveSkillFamilyDef[]
            {
                new ActiveSkillFamilyDef
                {
                    Id = "ilban_attack", Name = "\uc77c\ubc18 \uacf5\uaca9", Icon = "\u2694\ufe0f",
                    Hierarchy = SkillHierarchy.BUILTIN,
                    Tags = new[] { SkillTag.PHYSICAL },
                    HeritageSynergy = new HeritageRoute[0],
                    Traits = new string[0],
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = V(Td("ilban_attack", t), "coefficient") },
                        new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "rage_accumulate", Count = 1 },
                    },
                    BuildDescription = _ => "\uae30\ubcf8 \uacf5\uaca9 + \ubd84\ub178 \ucd95\uc801",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "bunno_attack", Name = "\ubd84\ub178 \uacf5\uaca9", Icon = "\ud83d\udca2",
                    Hierarchy = SkillHierarchy.BUILTIN,
                    Tags = new[] { SkillTag.RAGE, SkillTag.PHYSICAL },
                    HeritageSynergy = new HeritageRoute[0],
                    Traits = new string[0],
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1), TriggerFactory.Prob(1.0f), TriggerFactory.RageFull()),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.CONSUME_RAGE, Amount = BattleDataTable.Data.Rage.MaxRage },
                        new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = V(Td("bunno_attack", t), "coefficient") },
                    },
                    BuildDescription = t =>
                    {
                        var d = Td("bunno_attack", t);
                        return $"\ubd84\ub178 \uac8c\uc774\uc9c0 100 \uc18c\ubaa8, \ubb3c\ub9ac \uacf5\uaca9 (\uacc4\uc218 {V(d, "coefficient")})";
                    },
                },
                new ActiveSkillFamilyDef
                {
                    Id = "rage_accumulate", Name = "\ubd84\ub178 \ucd95\uc801", Icon = "\ud83d\udca2",
                    Hierarchy = SkillHierarchy.LOWEST,
                    Tags = new[] { SkillTag.RAGE },
                    HeritageSynergy = new HeritageRoute[0],
                    Traits = new string[0],
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.ADD_RAGE, Amount = (int)V(Td("rage_accumulate", t), "amount"), UseSourceStat = true },
                    },
                    BuildDescription = t => $"\ubd84\ub178 \uac8c\uc774\uc9c0 {V(Td("rage_accumulate", t), "amount")} \ucd94\uac00",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "hp_recovery", Name = "HP \ud68c\ubcf5", Icon = "\ud83d\udc9a",
                    Hierarchy = SkillHierarchy.LOWEST,
                    Tags = new[] { SkillTag.HP_RECOVERY },
                    HeritageSynergy = new HeritageRoute[0],
                    Traits = new string[0],
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.HEAL_HP, Coefficient = (float)V(Td("hp_recovery", t), "amount") },
                    },
                    BuildDescription = t => $"\ucd5c\ub300\uccb4\ub825\uc758 {Pct(V(Td("hp_recovery", t), "amount"))} \ud68c\ubcf5",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "lightning_summon", Name = "\ubc88\uac1c \uc18c\ud658", Icon = "\u26a1",
                    Hierarchy = SkillHierarchy.LOWEST,
                    Tags = new[] { SkillTag.LIGHTNING, SkillTag.MAGIC },
                    HeritageSynergy = new[] { HeritageRoute.GHOST },
                    Traits = new string[0],
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.MAGIC, Coefficient = V(Td("lightning_summon", t), "coefficient") },
                    },
                    BuildDescription = t => $"\ubc88\uac1c \ub9c8\ubc95 \uacf5\uaca9 (\uacc4\uc218 {V(Td("lightning_summon", t), "coefficient")})",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "lance_summon", Name = "\uad11\ucc3d \uc18c\ud658", Icon = "\ud83d\udd31",
                    Hierarchy = SkillHierarchy.LOWEST,
                    Tags = new[] { SkillTag.LANCE, SkillTag.MAGIC },
                    HeritageSynergy = new[] { HeritageRoute.KNIGHT },
                    Traits = new string[0],
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.MAGIC, Coefficient = V(Td("lance_summon", t), "coefficient") },
                    },
                    BuildDescription = t => $"\uad11\ucc3d \ub9c8\ubc95 \uacf5\uaca9 (\uacc4\uc218 {V(Td("lance_summon", t), "coefficient")})",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "sword_aura_summon", Name = "\uac80\uae30 \uc18c\ud658", Icon = "\u2694\ufe0f",
                    Hierarchy = SkillHierarchy.LOWEST,
                    Tags = new[] { SkillTag.SWORD_AURA, SkillTag.PHYSICAL },
                    HeritageSynergy = new[] { HeritageRoute.SKULL },
                    Traits = new string[0],
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = V(Td("sword_aura_summon", t), "coefficient"), IsAoe = true },
                    },
                    BuildDescription = t => $"\uac80\uae30 \uad11\uc5ed \ubb3c\ub9ac \uacf5\uaca9 (\uacc4\uc218 {V(Td("sword_aura_summon", t), "coefficient")})",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "poison_inject", Name = "\ub3c5 \uc8fc\uc785", Icon = "\ud83e\uddea",
                    Hierarchy = SkillHierarchy.LOWEST,
                    Tags = new[] { SkillTag.POISON },
                    HeritageSynergy = new HeritageRoute[0],
                    Traits = new string[0],
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                    BuildEffects = t =>
                    {
                        var d = Td("poison_inject", t);
                        return new[]
                        {
                            new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.FIXED, Coefficient = V(d, "coefficient"), Duration = (int)V(d, "duration") },
                        };
                    },
                    BuildDescription = t =>
                    {
                        var d = Td("poison_inject", t);
                        return $"\ub3c5 \uace0\uc815 \ud53c\ud574 (\uacc4\uc218 {V(d, "coefficient")}, {(int)V(d, "duration")}\ud134)";
                    },
                },
                new ActiveSkillFamilyDef
                {
                    Id = "flame_summon", Name = "\ud654\uc5fc \uc18c\ud658", Icon = "\ud83d\udd25",
                    Hierarchy = SkillHierarchy.LOWEST,
                    Tags = new[] { SkillTag.FLAME, SkillTag.MAGIC },
                    HeritageSynergy = new HeritageRoute[0],
                    Traits = new string[0],
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                    BuildEffects = t =>
                    {
                        var d = Td("flame_summon", t);
                        return new[]
                        {
                            new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.MAGIC, Coefficient = V(d, "coefficient"), Duration = (int)V(d, "duration") },
                        };
                    },
                    BuildDescription = t =>
                    {
                        var d = Td("flame_summon", t);
                        return $"\ud654\uc5fc \ub9c8\ubc95 \ub3c4\ud2b8 (\uacc4\uc218 {V(d, "coefficient")}, {(int)V(d, "duration")}\ud134)";
                    },
                },
                new ActiveSkillFamilyDef
                {
                    Id = "shuriken_summon", Name = "\uc218\ub9ac\uac80 \uc18c\ud658", Icon = "\ud83c\udf00",
                    Hierarchy = SkillHierarchy.LOWER,
                    Tags = new[] { SkillTag.SHURIKEN, SkillTag.PHYSICAL },
                    HeritageSynergy = new[] { HeritageRoute.RANGER },
                    Traits = new string[0],
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = V(Td("shuriken_summon", t), "coefficient") },
                    },
                    BuildDescription = t => $"\uc218\ub9ac\uac80 \ubb3c\ub9ac \uacf5\uaca9 (\uacc4\uc218 {V(Td("shuriken_summon", t), "coefficient")})",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "thunder_shuriken", Name = "\ubc88\uac1c \uc218\ub9ac\uac80", Icon = "\u26a1\ud83c\udf00",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.SHURIKEN, SkillTag.LIGHTNING },
                    HeritageSynergy = new[] { HeritageRoute.RANGER, HeritageRoute.GHOST },
                    Traits = new[] { "\uc218\ub9ac\uac80+\ubc88\uac1c \ubcf5\ud569", "\ud655\ub960 \uae30\ubc18 \ucd94\uac00 \ud53c\ud574" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(2)),
                    BuildEffects = t =>
                    {
                        float p = V(Td("thunder_shuriken", t), "injectedProbability");
                        return new[]
                        {
                            new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "shuriken_summon", Count = 1 },
                            new ActiveSkillEffect
                            {
                                Type = SkillEffectType.INJECT_EFFECT, TargetSkillId = "shuriken_summon",
                                InjectedEffects = new List<ActiveSkillEffect>
                                {
                                    new ActiveSkillEffect
                                    {
                                        Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "lightning_summon", Count = 1,
                                        TriggerConditions = TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("shuriken_summon"), TriggerFactory.Prob(p)),
                                    },
                                },
                            },
                        };
                    },
                    BuildDescription = t => $"2\ud134\ub9c8\ub2e4 \uc218\ub9ac\uac80 \uc18c\ud658, \uc218\ub9ac\uac80\uc774 {Pct(V(Td("thunder_shuriken", t), "injectedProbability"))} \ud655\ub960\ub85c \ubc88\uac1c \uc18c\ud658",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "rage_shuriken", Name = "\ubd84\ub178 \uc218\ub9ac\uac80", Icon = "\ud83d\udca2\ud83c\udf00",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.SHURIKEN, SkillTag.RAGE },
                    HeritageSynergy = new[] { HeritageRoute.RANGER },
                    Traits = new[] { "\uc218\ub9ac\uac80+\ubd84\ub178 \ubcf5\ud569", "\ubd84\ub178 \uac8c\uc774\uc9c0 \ube60\ub978 \ucd95\uc801" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(2)),
                    BuildEffects = t =>
                    {
                        float p = V(Td("rage_shuriken", t), "injectedProbability");
                        return new[]
                        {
                            new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "shuriken_summon", Count = 1 },
                            new ActiveSkillEffect
                            {
                                Type = SkillEffectType.INJECT_EFFECT, TargetSkillId = "shuriken_summon",
                                InjectedEffects = new List<ActiveSkillEffect>
                                {
                                    new ActiveSkillEffect
                                    {
                                        Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "rage_accumulate", Count = 1,
                                        TriggerConditions = TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("shuriken_summon"), TriggerFactory.Prob(p)),
                                    },
                                },
                            },
                        };
                    },
                    BuildDescription = t => $"2\ud134\ub9c8\ub2e4 \uc218\ub9ac\uac80 \uc18c\ud658, \uc218\ub9ac\uac80\uc774 {Pct(V(Td("rage_shuriken", t), "injectedProbability"))} \ud655\ub960\ub85c \ubd84\ub178 \ucd94\uac00",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "recovery_shuriken", Name = "\ud68c\ubcf5 \uc218\ub9ac\uac80", Icon = "\ud83d\udc9a\ud83c\udf00",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.SHURIKEN, SkillTag.HP_RECOVERY },
                    HeritageSynergy = new[] { HeritageRoute.RANGER },
                    Traits = new[] { "\uc218\ub9ac\uac80+\ud68c\ubcf5 \ubcf5\ud569", "\uacf5\uaca9\uacfc \ud68c\ubcf5 \ub3d9\uc2dc" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(2)),
                    BuildEffects = t =>
                    {
                        float p = V(Td("recovery_shuriken", t), "injectedProbability");
                        return new[]
                        {
                            new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "shuriken_summon", Count = 1 },
                            new ActiveSkillEffect
                            {
                                Type = SkillEffectType.INJECT_EFFECT, TargetSkillId = "shuriken_summon",
                                InjectedEffects = new List<ActiveSkillEffect>
                                {
                                    new ActiveSkillEffect
                                    {
                                        Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "hp_recovery", Count = 1,
                                        TriggerConditions = TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("shuriken_summon"), TriggerFactory.Prob(p)),
                                    },
                                },
                            },
                        };
                    },
                    BuildDescription = t => $"2\ud134\ub9c8\ub2e4 \uc218\ub9ac\uac80 \uc18c\ud658, \uc218\ub9ac\uac80\uc774 {Pct(V(Td("recovery_shuriken", t), "injectedProbability"))} \ud655\ub960\ub85c HP \ud68c\ubcf5",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "poison_shuriken", Name = "\ub3c5 \uc218\ub9ac\uac80", Icon = "\u2620\ufe0f\ud83c\udf00",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.SHURIKEN, SkillTag.POISON },
                    HeritageSynergy = new[] { HeritageRoute.RANGER },
                    Traits = new[] { "\uc218\ub9ac\uac80+\ub3c5 \ubcf5\ud569", "\ud655\ub960 \uae30\ubc18 \uc9c0\uc18d \ud53c\ud574" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(2)),
                    BuildEffects = t =>
                    {
                        float p = V(Td("poison_shuriken", t), "injectedProbability");
                        return new[]
                        {
                            new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "shuriken_summon", Count = 1 },
                            new ActiveSkillEffect
                            {
                                Type = SkillEffectType.INJECT_EFFECT, TargetSkillId = "shuriken_summon",
                                InjectedEffects = new List<ActiveSkillEffect>
                                {
                                    new ActiveSkillEffect
                                    {
                                        Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "poison_inject", Count = 1,
                                        TriggerConditions = TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("shuriken_summon"), TriggerFactory.Prob(p)),
                                    },
                                },
                            },
                        };
                    },
                    BuildDescription = t => $"2\ud134\ub9c8\ub2e4 \uc218\ub9ac\uac80 \uc18c\ud658, \uc218\ub9ac\uac80\uc774 {Pct(V(Td("poison_shuriken", t), "injectedProbability"))} \ud655\ub960\ub85c \ub3c5 \uc8fc\uc785",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "shuriken_strike", Name = "\uc218\ub9ac\uac80 \uac15\ud0c0", Icon = "\ud83c\udf00",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.SHURIKEN },
                    HeritageSynergy = new[] { HeritageRoute.RANGER },
                    Traits = new[] { "\uc77c\ubc18 \uacf5\uaca9 \uc5f0\uacc4", "\ubb3c\ub9ac \uacf5\uaca9" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "shuriken_summon", Count = (int)V(Td("shuriken_strike", t), "count") },
                    },
                    BuildDescription = t => $"\uc77c\ubc18 \uacf5\uaca9 \uc2dc \uc218\ub9ac\uac80 \uc18c\ud658 {(int)V(Td("shuriken_strike", t), "count")}\ud68c",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "thunder_strike", Name = "\ubc88\uac1c \uac15\ud0c0", Icon = "\u26a1",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.LIGHTNING },
                    HeritageSynergy = new[] { HeritageRoute.GHOST },
                    Traits = new[] { "\uc77c\ubc18 \uacf5\uaca9 \uc5f0\uacc4", "\ub9c8\ubc95 \uacf5\uaca9 (\ubc29\uc5b4 \uad00\ud1b5 \ub192\uc74c)" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "lightning_summon", Count = (int)V(Td("thunder_strike", t), "count") },
                    },
                    BuildDescription = t => $"\uc77c\ubc18 \uacf5\uaca9 \uc2dc \ubc88\uac1c \uc18c\ud658 {(int)V(Td("thunder_strike", t), "count")}\ud68c",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "lance_strike", Name = "\uad11\ucc3d \uac15\ud0c0", Icon = "\ud83d\udd31",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.LANCE },
                    HeritageSynergy = new[] { HeritageRoute.KNIGHT },
                    Traits = new[] { "\ub9e4\ud134 \uc790\ub3d9 \ubc1c\ub3d9", "\ub9c8\ubc95 \uacf5\uaca9 (\ubc29\uc5b4 \uad00\ud1b5 \ub192\uc74c)" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "lance_summon", Count = (int)V(Td("lance_strike", t), "count") },
                    },
                    BuildDescription = t => $"\ub9e4\ud134 \uad11\ucc3d \uc18c\ud658 {(int)V(Td("lance_strike", t), "count")}\ud68c",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "aura_strike", Name = "\uac80\uae30 \uac15\ud0c0", Icon = "\u2694\ufe0f",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.SWORD_AURA },
                    HeritageSynergy = new[] { HeritageRoute.SKULL },
                    Traits = new[] { "\uc77c\ubc18 \uacf5\uaca9 \uc5f0\uacc4", "\uad11\uc5ed \ubb3c\ub9ac \uacf5\uaca9", "\ub2e8\uc77c \uacc4\uc218 \ub0ae\uc74c" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "sword_aura_summon", Count = (int)V(Td("aura_strike", t), "count") },
                    },
                    BuildDescription = t => $"\uc77c\ubc18 \uacf5\uaca9 \uc2dc \uac80\uae30 \uc18c\ud658 {(int)V(Td("aura_strike", t), "count")}\ud68c",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "bunno_thunder", Name = "\ubd84\ub178 \uacf5\uaca9 \ubc88\uac1c", Icon = "\ud83d\udca2\u26a1",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.RAGE, SkillTag.LIGHTNING },
                    HeritageSynergy = new[] { HeritageRoute.GHOST },
                    Traits = new[] { "\ubd84\ub178 \uacf5\uaca9 \uc5f0\uacc4", "\ub9c8\ubc95 \uacf5\uaca9 (\ubc29\uc5b4 \uad00\ud1b5 \ub192\uc74c)" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("bunno_attack")),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "lightning_summon", Count = (int)V(Td("bunno_thunder", t), "count") },
                    },
                    BuildDescription = t => $"\ubd84\ub178 \uacf5\uaca9 \uc2dc \ubc88\uac1c \uc18c\ud658 {(int)V(Td("bunno_thunder", t), "count")}\ud68c",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "bunno_lance", Name = "\ubd84\ub178 \uacf5\uaca9 \uad11\ucc3d", Icon = "\ud83d\udca2\ud83d\udd31",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.RAGE, SkillTag.LANCE },
                    HeritageSynergy = new[] { HeritageRoute.KNIGHT },
                    Traits = new[] { "\ubd84\ub178 \uacf5\uaca9 \uc5f0\uacc4", "\ub9c8\ubc95 \uacf5\uaca9 (\ubc29\uc5b4 \uad00\ud1b5 \ub192\uc74c)" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("bunno_attack")),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "lance_summon", Count = (int)V(Td("bunno_lance", t), "count") },
                    },
                    BuildDescription = t => $"\ubd84\ub178 \uacf5\uaca9 \uc2dc \uad11\ucc3d \uc18c\ud658 {(int)V(Td("bunno_lance", t), "count")}\ud68c",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "bunno_flame", Name = "\ubd84\ub178 \uacf5\uaca9 \ud654\uc5fc", Icon = "\ud83d\udca2\ud83d\udd25",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.RAGE, SkillTag.FLAME },
                    HeritageSynergy = new HeritageRoute[0],
                    Traits = new[] { "\ubd84\ub178 \uacf5\uaca9 \uc5f0\uacc4", "\ub3c4\ud2b8 \ub370\ubbf8\uc9c0" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("bunno_attack")),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "flame_summon", Count = (int)V(Td("bunno_flame", t), "count") },
                    },
                    BuildDescription = t => $"\ubd84\ub178 \uacf5\uaca9 \uc2dc \ud654\uc5fc \uc18c\ud658 {(int)V(Td("bunno_flame", t), "count")}\ud68c",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "rage_gauge_boost", Name = "\ubd84\ub178 \uac8c\uc774\uc9c0 \uc99d\uac00", Icon = "\ud83d\udca2",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.NORMAL_ATTACK, SkillTag.RAGE },
                    HeritageSynergy = new HeritageRoute[0],
                    Traits = new[] { "\uc77c\ubc18 \uacf5\uaca9 \uc5f0\uacc4", "\ubd84\ub178 \uac8c\uc774\uc9c0 \ube60\ub978 \ucd95\uc801" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.ADD_RAGE, Amount = (int)V(Td("rage_gauge_boost", t), "amount") },
                    },
                    BuildDescription = t => $"\uc77c\ubc18 \uacf5\uaca9 \uc2dc \ubd84\ub178 \uac8c\uc774\uc9c0 {V(Td("rage_gauge_boost", t), "amount")} \ucd94\uac00",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "venom_sword", Name = "\uc9c0\ub3c5\ud55c \uac80", Icon = "\ud83e\uddea",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.NORMAL_ATTACK, SkillTag.POISON },
                    HeritageSynergy = new HeritageRoute[0],
                    Traits = new[] { "\uc77c\ubc18 \uacf5\uaca9 \uc5f0\uacc4", "\uace0\uc815 \ud53c\ud574 (\ubc29\uc5b4 \ubb34\uc2dc)" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                    BuildEffects = _ => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "poison_inject", Count = 1 },
                    },
                    BuildDescription = _ => "\uc77c\ubc18 \uacf5\uaca9 \uc2dc \ub3c5 \uc8fc\uc785",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "tyrant", Name = "\ud3ed\uad70\uc758 \uc77c\uaca9", Icon = "\ud83d\udc79",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.PHYSICAL },
                    HeritageSynergy = new[] { HeritageRoute.SKULL },
                    Traits = new[] { "\uc77c\ubc18 \uacf5\uaca9 \uc5f0\uacc4", "\ubb3c\ub9ac \ucd94\uac00 \ud0c0\uaca9" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = V(Td("tyrant", t), "coefficient") },
                    },
                    BuildDescription = t => $"\uc77c\ubc18 \uacf5\uaca9 \uc2dc \ubb3c\ub9ac \ucd94\uac00 \uacf5\uaca9 (\uacc4\uc218 {V(Td("tyrant", t), "coefficient")})",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "shrink_magic", Name = "\ucd95\uc18c \ub9c8\ubc95", Icon = "\ud83d\udd2e",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.DEBUFF },
                    HeritageSynergy = new HeritageRoute[0],
                    Traits = new[] { "\ub9e4\ud134 \uc790\ub3d9 \ubc1c\ub3d9", "\uc801 \uc57d\ud654 \ub514\ubc84\ud504" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                    BuildEffects = t =>
                    {
                        var d = Td("shrink_magic", t);
                        return new[]
                        {
                            new ActiveSkillEffect { Type = SkillEffectType.DEBUFF, Stat = StatType.ATK, Reduction = V(d, "reduction"), Duration = (int)V(d, "duration") },
                        };
                    },
                    BuildDescription = t =>
                    {
                        var d = Td("shrink_magic", t);
                        return $"\ub9e4\ud134 \uc801 ATK {Pct(V(d, "reduction"))} \uac10\uc18c ({(int)V(d, "duration")}\ud134)";
                    },
                },
                new ActiveSkillFamilyDef
                {
                    Id = "max_hp_damage", Name = "\uc555\ub3c4", Icon = "\ud83d\udcaa",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.PHYSICAL },
                    HeritageSynergy = new[] { HeritageRoute.KNIGHT },
                    Traits = new[] { "\uc77c\ubc18 \uacf5\uaca9 \uc5f0\uacc4", "\ucd5c\ub300 \uccb4\ub825 \ube44\ub840 \ucd94\uac00 \ub370\ubbf8\uc9c0" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = V(Td("max_hp_damage", t), "coefficient"), DamageBase = DamageBase.SOURCE_MAX_HP },
                    },
                    BuildDescription = t => $"\uc77c\ubc18 \uacf5\uaca9 \uc2dc \ucd5c\ub300 \uccb4\ub825\uc758 {Pct(V(Td("max_hp_damage", t), "coefficient"))} \ubb3c\ub9ac \ucd94\uac00 \ub370\ubbf8\uc9c0",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "hp_crush", Name = "\ubd84\uc1c4", Icon = "\ud83d\udd28",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.PHYSICAL },
                    HeritageSynergy = new[] { HeritageRoute.SKULL },
                    Traits = new[] { "\uc77c\ubc18 \uacf5\uaca9 \uc5f0\uacc4", "\uc801 \ucd5c\ub300 \uccb4\ub825 \ube44\ub840 \ub370\ubbf8\uc9c0" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                    BuildEffects = t => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.PHYSICAL, Coefficient = V(Td("hp_crush", t), "coefficient"), DamageBase = DamageBase.TARGET_MAX_HP },
                    },
                    BuildDescription = t => $"\uc77c\ubc18 \uacf5\uaca9 \uc2dc \uc801 \ucd5c\ub300 \uccb4\ub825\uc758 {Pct(V(Td("hp_crush", t), "coefficient"))} \ubb3c\ub9ac \ub370\ubbf8\uc9c0",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "stun_apply", Name = "\uae30\uc808", Icon = "\ud83d\udcab",
                    Hierarchy = SkillHierarchy.LOWEST,
                    Tags = new[] { SkillTag.DEBUFF },
                    HeritageSynergy = new HeritageRoute[0],
                    Traits = new string[0],
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.EveryNTurns(1)),
                    BuildEffects = t =>
                    {
                        var d = Td("stun_apply", t);
                        return new[]
                        {
                            new ActiveSkillEffect { Type = SkillEffectType.STUN, Chance = V(d, "chance"), Duration = (int)V(d, "duration") },
                        };
                    },
                    BuildDescription = t =>
                    {
                        var d = Td("stun_apply", t);
                        return $"{Pct(V(d, "chance"))} \ud655\ub960\ub85c {(int)V(d, "duration")}\ud134 \uae30\uc808";
                    },
                },
                new ActiveSkillFamilyDef
                {
                    Id = "stun_strike", Name = "\uac15\ud0c0", Icon = "\ud83c\udf1f",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.DEBUFF },
                    HeritageSynergy = new[] { HeritageRoute.KNIGHT },
                    Traits = new[] { "\uc77c\ubc18 \uacf5\uaca9 \uc5f0\uacc4", "\ud655\ub960 \uae30\uc808", "\uc801 \ud589\ub3d9 \ubd09\uc1c4" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                    BuildEffects = _ => new[]
                    {
                        new ActiveSkillEffect { Type = SkillEffectType.TRIGGER_SKILL, TargetSkillId = "stun_apply", Count = 1 },
                    },
                    BuildDescription = _ => "\uc77c\ubc18 \uacf5\uaca9 \uc2dc \uae30\uc808 \uc2dc\ub3c4",
                },
                new ActiveSkillFamilyDef
                {
                    Id = "demon_power", Name = "\uc545\ub9c8\uc758 \ud798", Icon = "\ud83d\ude08",
                    Hierarchy = SkillHierarchy.UPPER,
                    Tags = new[] { SkillTag.MAGIC },
                    HeritageSynergy = new HeritageRoute[0],
                    Traits = new[] { "\uc77c\ubc18 \uacf5\uaca9 \uc5f0\uacc4", "\ub9c8\ubc95 \ucd94\uac00 \ud0c0\uaca9" },
                    BuildTrigger = _ => TriggerFactory.Trigger(TriggerFactory.OnSkillActivation("ilban_attack")),
                    BuildEffects = t =>
                    {
                        var d = Td("demon_power", t);
                        float coeff = d.ContainsKey("coefficient") ? d["coefficient"] : V(Td("demon_power", 4), "coefficient");
                        return new[]
                        {
                            new ActiveSkillEffect { Type = SkillEffectType.ATTACK, AttackType = AttackType.MAGIC, Coefficient = coeff },
                        };
                    },
                    BuildDescription = t =>
                    {
                        var d = Td("demon_power", t);
                        float coeff = d.ContainsKey("coefficient") ? d["coefficient"] : V(Td("demon_power", 4), "coefficient");
                        return $"\uc77c\ubc18 \uacf5\uaca9 \uc2dc \ub9c8\ubc95 \ucd94\uac00 \uacf5\uaca9 ({Pct(coeff)})";
                    },
                },
            };
        }

        private static void EnsureLoaded()
        {
            if (_allSkills != null) return;

            var families = BuildFamilies();
            _allSkills = new List<ActiveSkill>();
            _builtinIds = new HashSet<string>();
            _specialIds = new HashSet<string> { "demon_power" };
            var upperFamilies = new List<string>();

            foreach (var family in families)
            {
                if (family.Hierarchy == SkillHierarchy.BUILTIN)
                    _builtinIds.Add(family.Id);
                if (family.Hierarchy == SkillHierarchy.UPPER)
                    upperFamilies.Add(family.Id);

                var familyData = ActiveSkillTierData.GetFamily(family.Id);
                if (familyData == null) continue;

                foreach (var tier in familyData.Keys.OrderBy(k => k))
                {
                    string suffix = SkillRegistryHelper.GetTierSuffix(tier);

                    _allSkills.Add(new ActiveSkill(
                        family.Id,
                        family.Name + suffix,
                        family.Icon,
                        family.Hierarchy,
                        tier,
                        family.Tags,
                        family.HeritageSynergy,
                        family.BuildTrigger(tier),
                        family.BuildEffects(tier),
                        family.BuildDescription(tier)
                    ));
                }
            }

            _upperFamilyIds = upperFamilies.ToArray();
        }

        public static List<ActiveSkill> GetAll()
        {
            EnsureLoaded();
            return _allSkills;
        }

        public static ActiveSkill GetById(string id, int tier = 1)
        {
            EnsureLoaded();
            return _allSkills.FirstOrDefault(s => s.Id == id && s.Tier == tier);
        }

        public static ActiveSkill GetNextTier(string id, int currentTier)
        {
            EnsureLoaded();
            return _allSkills.FirstOrDefault(s => s.Id == id && s.Tier == currentTier + 1);
        }

        public static List<ActiveSkill> GetBuiltinSkills()
        {
            EnsureLoaded();
            return _allSkills.Where(s => s.Hierarchy == SkillHierarchy.BUILTIN && s.Tier == 1).ToList();
        }

        public static List<ActiveSkill> GetUpperTier1Skills()
        {
            EnsureLoaded();
            return _allSkills.Where(s => s.Hierarchy == SkillHierarchy.UPPER && s.Tier == 1 && !_specialIds.Contains(s.Id)).ToList();
        }

        public static bool IsBuiltinSkill(string id)
        {
            EnsureLoaded();
            return _builtinIds.Contains(id);
        }

        public static bool IsSpecialSkill(string id)
        {
            EnsureLoaded();
            return _specialIds.Contains(id);
        }

        public static string[] GetUpperFamilyIds()
        {
            EnsureLoaded();
            return _upperFamilyIds;
        }
    }
}
