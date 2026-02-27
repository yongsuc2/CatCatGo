using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;

namespace CatCatGo.Domain.Data
{
    public class PassiveSkillFamilyDef
    {
        public string Id;
        public string Name;
        public string Icon;
        public SkillTag[] Tags;
        public HeritageRoute[] HeritageSynergy;
        public string[] Traits;
        public Func<int, PassiveEffect> BuildEffect;
        public Func<int, string> BuildDescription;
    }

    public static class PassiveSkillRegistry
    {
        private static List<PassiveSkill> _allSkills;
        private static HashSet<string> _specialIds;
        private static string[] _familyIds;

        private static readonly Dictionary<int, string> TierSuffix = new Dictionary<int, string>
        {
            { 1, "" }, { 2, " II" }, { 3, " III" }, { 4, " IV" },
        };

        private static Dictionary<string, float> Td(string id, int tier)
        {
            return PassiveSkillTierData.GetTierData(id, tier) ?? new Dictionary<string, float>();
        }

        private static string Pct(float v)
        {
            return $"{Math.Round(v * 100)}%";
        }

        private static float V(Dictionary<string, float> d, string key)
        {
            return d.TryGetValue(key, out var v) ? v : 0f;
        }

        private static PassiveSkillFamilyDef[] BuildFamilies()
        {
            return new PassiveSkillFamilyDef[]
            {
                new PassiveSkillFamilyDef
                {
                    Id = "lifesteal", Name = "\ud761\ud608", Icon = "\ud83e\ude78",
                    Tags = new SkillTag[0], HeritageSynergy = new HeritageRoute[0],
                    Traits = new[] { "\ubb3c\ub9ac \uacf5\uaca9\uc5d0 \ud6a8\uacfc\uc801", "\uc9c0\uc18d\uc801 \uccb4\ub825 \ud68c\ubcf5" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.LIFESTEAL, Rate = V(Td("lifesteal", t), "rate") },
                    BuildDescription = t => $"\uac00\ud55c \ud53c\ud574\uc758 {Pct(V(Td("lifesteal", t), "rate"))} HP \ud68c\ubcf5",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "regen", Name = "\uc7ac\uc0dd", Icon = "\ud83d\udc9a",
                    Tags = new[] { SkillTag.HP_RECOVERY }, HeritageSynergy = new HeritageRoute[0],
                    Traits = new[] { "\ub9e4\ud134 \uc790\ub3d9 \ud68c\ubcf5", "\uc804\ud22c \uc9c0\uad6c\ub825 \ud5a5\uc0c1" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.REGEN, HealPerTurn = V(Td("regen", t), "healPerTurn") },
                    BuildDescription = t => $"\ub9e4\ud134 \ucd5c\ub300\uccb4\ub825\uc758 {Pct(V(Td("regen", t), "healPerTurn"))} \ud68c\ubcf5",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "counter", Name = "\ubc18\uaca9", Icon = "\ud83d\udee1\ufe0f",
                    Tags = new SkillTag[0], HeritageSynergy = new[] { HeritageRoute.KNIGHT },
                    Traits = new[] { "\ud53c\uaca9 \uc2dc \ubc1c\ub3d9", "\ubb3c\ub9ac \ubc18\uaca9" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.COUNTER, TriggerChance = V(Td("counter", t), "triggerChance") },
                    BuildDescription = t => $"\ud53c\uaca9 \uc2dc {Pct(V(Td("counter", t), "triggerChance"))} \ud655\ub960\ub85c \uc77c\ubc18 \uacf5\uaca9 \ubc18\uaca9",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "iron_shield", Name = "\ubc29\uc5b4\ub9c9", Icon = "\ud83d\udd30",
                    Tags = new SkillTag[0], HeritageSynergy = new HeritageRoute[0],
                    Traits = new[] { "\uc804\ud22c \uc2dc\uc791 \uc2dc 1\ud68c", "\ucd08\ubc18 \uc548\uc815\uc131" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.SHIELD_ON_START, HpPercent = V(Td("iron_shield", t), "hpPercent") },
                    BuildDescription = t => $"\uc804\ud22c \uc2dc\uc791 \uc2dc \ucd5c\ub300 HP\uc758 {Pct(V(Td("iron_shield", t), "hpPercent"))} \ubc29\uc5b4\ub9c9",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "multi_hit", Name = "\uc5f0\ud0c0", Icon = "\ud83d\udcab",
                    Tags = new SkillTag[0], HeritageSynergy = new[] { HeritageRoute.SKULL },
                    Traits = new[] { "\uc77c\ubc18 \uacf5\uaca9 \uc804\uc6a9", "\ud655\ub960 \uae30\ubc18 \ucd94\uac00 \ud0c0\uaca9" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.MULTI_HIT, Chance = V(Td("multi_hit", t), "chance") },
                    BuildDescription = t => $"{Pct(V(Td("multi_hit", t), "chance"))} \ud655\ub960\ub85c \ucd94\uac00 \ud0c0\uaca9",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "crit_mastery", Name = "\uce58\uba85\ud0c0 \ub9c8\uc2a4\ud130\ub9ac", Icon = "\ud83c\udfaf",
                    Tags = new SkillTag[0], HeritageSynergy = new[] { HeritageRoute.RANGER },
                    Traits = new[] { "\ubb3c\ub9ac \uacf5\uaca9\uc5d0\ub9cc \uc801\uc6a9", "\ub192\uc740 \ud3ed\ubc1c\ub825" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.STAT_MODIFIER, Stat = StatType.CRIT, Value = V(Td("crit_mastery", t), "value"), IsPercentage = false },
                    BuildDescription = t => $"\uce58\uba85\ud0c0 \ud655\ub960 +{Pct(V(Td("crit_mastery", t), "value"))}",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "rage_mastery", Name = "\ubd84\ub178 \ub9c8\uc2a4\ud130\ub9ac", Icon = "\ud83d\udca2",
                    Tags = new[] { SkillTag.RAGE }, HeritageSynergy = new[] { HeritageRoute.SKULL, HeritageRoute.KNIGHT },
                    Traits = new[] { "\ubd84\ub178 \uacf5\uaca9 \uac15\ud654 \uc804\uc6a9", "\ubd84\ub178 \ube4c\ub4dc \ud575\uc2ec" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.STAT_MODIFIER, Value = V(Td("rage_mastery", t), "value"), IsPercentage = true },
                    BuildDescription = t => $"\ubd84\ub178 \uacf5\uaca9 \ub370\ubbf8\uc9c0 +{Pct(V(Td("rage_mastery", t), "value"))}",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "lightning_mastery", Name = "\ubc88\uac1c \ub9c8\uc2a4\ud130\ub9ac", Icon = "\u26a1",
                    Tags = new[] { SkillTag.LIGHTNING }, HeritageSynergy = new[] { HeritageRoute.GHOST },
                    Traits = new[] { "\ubc88\uac1c \uc2a4\ud0ac \uc804\uc6a9", "\ub9c8\ubc95 \ube4c\ub4dc \uc2dc\ub108\uc9c0" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.SKILL_MODIFIER, TargetTag = SkillTag.LIGHTNING, DamageMultiplier = V(Td("lightning_mastery", t), "damageBonus") },
                    BuildDescription = t => $"\ubc88\uac1c \uc2a4\ud0ac \ub370\ubbf8\uc9c0 +{Pct(V(Td("lightning_mastery", t), "damageBonus"))}",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "shuriken_mastery", Name = "\uc218\ub9ac\uac80 \ub9c8\uc2a4\ud130\ub9ac", Icon = "\ud83c\udf00",
                    Tags = new[] { SkillTag.SHURIKEN }, HeritageSynergy = new[] { HeritageRoute.RANGER },
                    Traits = new[] { "\uc218\ub9ac\uac80 \uc2a4\ud0ac \uc804\uc6a9", "\ubb3c\ub9ac \ube4c\ub4dc \uc2dc\ub108\uc9c0" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.SKILL_MODIFIER, TargetTag = SkillTag.SHURIKEN, DamageMultiplier = V(Td("shuriken_mastery", t), "damageBonus") },
                    BuildDescription = t => $"\uc218\ub9ac\uac80 \uc2a4\ud0ac \ub370\ubbf8\uc9c0 +{Pct(V(Td("shuriken_mastery", t), "damageBonus"))}",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "lance_mastery", Name = "\uad11\ucc3d \ub9c8\uc2a4\ud130\ub9ac", Icon = "\ud83d\udd31",
                    Tags = new[] { SkillTag.LANCE }, HeritageSynergy = new[] { HeritageRoute.KNIGHT },
                    Traits = new[] { "\uad11\ucc3d \uc2a4\ud0ac \uc804\uc6a9", "\ub9c8\ubc95 \ube4c\ub4dc \uc2dc\ub108\uc9c0" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.SKILL_MODIFIER, TargetTag = SkillTag.LANCE, DamageMultiplier = V(Td("lance_mastery", t), "damageBonus") },
                    BuildDescription = t => $"\uad11\ucc3d \uc2a4\ud0ac \ub370\ubbf8\uc9c0 +{Pct(V(Td("lance_mastery", t), "damageBonus"))}",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "revive", Name = "\ubd80\ud65c", Icon = "\u2728",
                    Tags = new SkillTag[0], HeritageSynergy = new HeritageRoute[0],
                    Traits = new[] { "\uc0ac\ub9dd \uc2dc 1\ud68c \ubd80\ud65c", "\ubcf4\ud5d8\ud615 \ud328\uc2dc\ube0c" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.REVIVE, HpPercent = V(Td("revive", t), "hpPercent"), MaxUses = 1 },
                    BuildDescription = t => $"\uc0ac\ub9dd \uc2dc HP {Pct(V(Td("revive", t), "hpPercent"))}\ub85c \ubd80\ud65c (1\ud68c)",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "magic_mastery", Name = "\ub9c8\ubc95 \ub9c8\uc2a4\ud130\ub9ac", Icon = "\ud83d\udd2e",
                    Tags = new[] { SkillTag.MAGIC }, HeritageSynergy = new[] { HeritageRoute.GHOST },
                    Traits = new[] { "\ubaa8\ub4e0 \ub9c8\ubc95 \uacf5\uaca9 \uac15\ud654", "\ub9c8\ubc95 \ube4c\ub4dc \ud575\uc2ec" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.STAT_MODIFIER, Value = V(Td("magic_mastery", t), "value"), IsPercentage = false },
                    BuildDescription = t => $"\ub9c8\ubc95 \uacc4\uc218 +{Pct(V(Td("magic_mastery", t), "value"))}",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "hp_fortify", Name = "\uccb4\ub825 \uac15\ud654", Icon = "\u2764\ufe0f",
                    Tags = new[] { SkillTag.HP_RECOVERY }, HeritageSynergy = new[] { HeritageRoute.KNIGHT },
                    Traits = new[] { "\ucd5c\ub300 \uccb4\ub825 \uc99d\uac00", "\ubc29\uc5b4\ub9c9/\uc7ac\uc0dd\uacfc \uc2dc\ub108\uc9c0" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.STAT_MODIFIER, Stat = StatType.HP, Value = V(Td("hp_fortify", t), "value"), IsPercentage = true },
                    BuildDescription = t => $"\ucd5c\ub300 \uccb4\ub825 +{Pct(V(Td("hp_fortify", t), "value"))}",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "atk_fortify", Name = "\uacf5\uaca9 \uac15\ud654", Icon = "\u2694\ufe0f",
                    Tags = new SkillTag[0], HeritageSynergy = new[] { HeritageRoute.SKULL },
                    Traits = new[] { "\uacf5\uaca9\ub825 \uc9c1\uc811 \uc99d\uac00", "\ubaa8\ub4e0 \ube4c\ub4dc \ubc94\uc6a9" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.STAT_MODIFIER, Stat = StatType.ATK, Value = V(Td("atk_fortify", t), "value"), IsPercentage = true },
                    BuildDescription = t => $"\uacf5\uaca9\ub825 +{Pct(V(Td("atk_fortify", t), "value"))}",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "def_fortify", Name = "\ubc29\uc5b4 \uac15\ud654", Icon = "\ud83d\udee1\ufe0f",
                    Tags = new SkillTag[0], HeritageSynergy = new[] { HeritageRoute.KNIGHT },
                    Traits = new[] { "\ubc29\uc5b4\ub825 \uc9c1\uc811 \uc99d\uac00", "\ubaa8\ub4e0 \ube4c\ub4dc \ubc94\uc6a9" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.STAT_MODIFIER, Stat = StatType.DEF, Value = V(Td("def_fortify", t), "value"), IsPercentage = true },
                    BuildDescription = t => $"\ubc29\uc5b4\ub825 +{Pct(V(Td("def_fortify", t), "value"))}",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "low_hp_atk", Name = "\ubc30\uc218\uc9c4", Icon = "\ud83d\udd25",
                    Tags = new SkillTag[0], HeritageSynergy = new[] { HeritageRoute.SKULL },
                    Traits = new[] { "\uccb4\ub825 \ub0ae\uc744\uc218\ub85d \uacf5\uaca9\ub825 \uc99d\uac00", "\uc2e4\uc2dc\uac04 \uc801\uc6a9", "\ud558\uc774\ub9ac\uc2a4\ud06c \ud558\uc774\ub9ac\ud134" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.LOW_HP_MODIFIER, Stat = StatType.ATK, MaxBonus = V(Td("low_hp_atk", t), "maxBonus") },
                    BuildDescription = t => $"\uccb4\ub825\uc774 \ub0ae\uc744\uc218\ub85d \uacf5\uaca9\ub825 \uc99d\uac00 (\ucd5c\ub300 +{Pct(V(Td("low_hp_atk", t), "maxBonus"))})",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "low_hp_def", Name = "\ubd88\uad74", Icon = "\ud83c\udfd4\ufe0f",
                    Tags = new SkillTag[0], HeritageSynergy = new[] { HeritageRoute.KNIGHT },
                    Traits = new[] { "\uccb4\ub825 \ub0ae\uc744\uc218\ub85d \ubc29\uc5b4\ub825 \uc99d\uac00", "\uc2e4\uc2dc\uac04 \uc801\uc6a9", "\uc0dd\uc874\ub825 \ud5a5\uc0c1" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.LOW_HP_MODIFIER, Stat = StatType.DEF, MaxBonus = V(Td("low_hp_def", t), "maxBonus") },
                    BuildDescription = t => $"\uccb4\ub825\uc774 \ub0ae\uc744\uc218\ub85d \ubc29\uc5b4\ub825 \uc99d\uac00 (\ucd5c\ub300 +{Pct(V(Td("low_hp_def", t), "maxBonus"))})",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "max_hp_damage", Name = "\uc555\ub3c4", Icon = "\ud83d\udcaa",
                    Tags = new[] { SkillTag.PHYSICAL }, HeritageSynergy = new[] { HeritageRoute.KNIGHT },
                    Traits = new[] { "\ucd5c\ub300 \uccb4\ub825 \ube44\ub840 \ucd94\uac00 \ub370\ubbf8\uc9c0", "\ubb3c\ub9ac \uacf5\uaca9 \uc804\uc6a9", "\ud0f1\ud06c \ube4c\ub4dc \uc2dc\ub108\uc9c0" },
                    BuildEffect = t => new PassiveEffect { Type = PassiveType.MAX_HP_DAMAGE, Coefficient = V(Td("max_hp_damage", t), "coefficient") },
                    BuildDescription = t => $"\ubb3c\ub9ac \uacf5\uaca9 \uc2dc \ucd5c\ub300 \uccb4\ub825\uc758 {Pct(V(Td("max_hp_damage", t), "coefficient"))}\ub9cc\ud07c \ucd94\uac00 \ub370\ubbf8\uc9c0",
                },
                new PassiveSkillFamilyDef
                {
                    Id = "angel_power", Name = "\ucc9c\uc0ac\uc758 \ud798", Icon = "\ud83d\ude07",
                    Tags = new SkillTag[0], HeritageSynergy = new HeritageRoute[0],
                    Traits = new[] { "\uacf5\uaca9\ub825 \uc9c1\uc811 \uc99d\uac00", "\ubaa8\ub4e0 \ube4c\ub4dc \ubc94\uc6a9" },
                    BuildEffect = t =>
                    {
                        var d = Td("angel_power", t);
                        float val = d.ContainsKey("value") ? d["value"] : V(Td("angel_power", 4), "value");
                        return new PassiveEffect { Type = PassiveType.STAT_MODIFIER, Stat = StatType.ATK, Value = val, IsPercentage = true };
                    },
                    BuildDescription = _ => "\uacf5\uaca9\ub825 +30%",
                },
            };
        }

        private static void EnsureLoaded()
        {
            if (_allSkills != null) return;

            var families = BuildFamilies();
            _allSkills = new List<PassiveSkill>();
            _specialIds = new HashSet<string> { "angel_power" };
            var familyIdList = new List<string>();

            foreach (var family in families)
            {
                familyIdList.Add(family.Id);

                var familyData = PassiveSkillTierData.GetFamily(family.Id);
                if (familyData == null) continue;

                foreach (var tier in familyData.Keys.OrderBy(k => k))
                {
                    string suffix;
                    TierSuffix.TryGetValue(tier, out suffix);
                    if (suffix == null) suffix = "";

                    _allSkills.Add(new PassiveSkill(
                        family.Id,
                        family.Name + suffix,
                        family.Icon,
                        tier,
                        family.Tags,
                        family.HeritageSynergy,
                        family.BuildEffect(tier),
                        family.BuildDescription(tier)
                    ));
                }
            }

            _familyIds = familyIdList.ToArray();
        }

        public static List<PassiveSkill> GetAll()
        {
            EnsureLoaded();
            return _allSkills;
        }

        public static PassiveSkill GetById(string id, int tier = 1)
        {
            EnsureLoaded();
            return _allSkills.FirstOrDefault(s => s.Id == id && s.Tier == tier);
        }

        public static PassiveSkill GetNextTier(string id, int currentTier)
        {
            EnsureLoaded();
            return _allSkills.FirstOrDefault(s => s.Id == id && s.Tier == currentTier + 1);
        }

        public static List<PassiveSkill> GetTier1Skills()
        {
            EnsureLoaded();
            return _allSkills.Where(s => s.Tier == 1 && !_specialIds.Contains(s.Id)).ToList();
        }

        public static bool IsSpecialSkill(string id)
        {
            EnsureLoaded();
            return _specialIds.Contains(id);
        }

        public static string[] GetFamilyIds()
        {
            EnsureLoaded();
            return _familyIds;
        }
    }
}
