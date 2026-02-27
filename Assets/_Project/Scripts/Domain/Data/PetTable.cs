using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public class PetAbilityDef
    {
        public PassiveType PassiveType;
        public StatType? Stat;
        public float BaseValue;
        public float GradeScale;
        public bool IsPercentage;
    }

    public class PetTemplateData
    {
        public string Id;
        public string Name;
        public PetTier Tier;
        public Stats BasePassiveBonus;
        public float Weight;
        public PetAbilityDef Ability;
    }

    public static class PetTable
    {
        private static List<PetTemplateData> _templates;
        private static Dictionary<PetGrade, float> _gradeMultipliers;
        private static Dictionary<PassiveType, string> _abilityLabels;
        private static Dictionary<StatType, string> _statLabels;

        private static void EnsureLoaded()
        {
            if (_templates != null) return;

            var data = JsonDataLoader.LoadJObject("pet.data.json");
            if (data == null) return;

            _templates = new List<PetTemplateData>();
            foreach (var t in data["templates"])
            {
                var ability = t["ability"];
                var abilityDef = new PetAbilityDef
                {
                    PassiveType = (PassiveType)Enum.Parse(typeof(PassiveType), ability["passiveType"].ToString()),
                    BaseValue = ability["baseValue"].Value<float>(),
                    GradeScale = ability["gradeScale"].Value<float>(),
                    IsPercentage = ability["isPercentage"].Value<bool>(),
                };
                if (ability["stat"] != null && ability["stat"].Type != JTokenType.Null)
                    abilityDef.Stat = (StatType)Enum.Parse(typeof(StatType), ability["stat"].ToString());

                _templates.Add(new PetTemplateData
                {
                    Id = t["id"].ToString(),
                    Name = t["name"].ToString(),
                    Tier = (PetTier)Enum.Parse(typeof(PetTier), t["tier"].ToString()),
                    BasePassiveBonus = Stats.Create(
                        atk: t["atk"].Value<int>(),
                        maxHp: t["maxHp"].Value<int>(),
                        def: t["def"].Value<int>(),
                        crit: t["crit"].Value<float>()
                    ),
                    Weight = t["weight"].Value<float>(),
                    Ability = abilityDef,
                });
            }

            _gradeMultipliers = new Dictionary<PetGrade, float>();
            foreach (var kv in (JObject)data["gradeMultipliers"])
                _gradeMultipliers[(PetGrade)Enum.Parse(typeof(PetGrade), kv.Key)] = kv.Value.Value<float>();

            _abilityLabels = new Dictionary<PassiveType, string>();
            foreach (var kv in (JObject)data["abilityLabels"])
            {
                if (Enum.TryParse<PassiveType>(kv.Key, out var pt))
                    _abilityLabels[pt] = kv.Value.ToString();
            }

            _statLabels = new Dictionary<StatType, string>();
            foreach (var kv in (JObject)data["statLabels"])
            {
                if (Enum.TryParse<StatType>(kv.Key, out var st))
                    _statLabels[st] = kv.Value.ToString();
            }
        }

        public static PetTemplateData GetTemplate(string id)
        {
            EnsureLoaded();
            return _templates.FirstOrDefault(p => p.Id == id);
        }

        public static List<PetTemplateData> GetAllTemplates()
        {
            EnsureLoaded();
            return _templates;
        }

        public static PetTemplateData GetRandomTemplate(SeededRandom rng)
        {
            EnsureLoaded();
            var entries = _templates
                .Select(t => (item: t, weight: t.Weight))
                .ToList();
            return rng.WeightedPick(entries);
        }

        public static List<PetTemplateData> GetTemplatesByTier(PetTier tier)
        {
            EnsureLoaded();
            return _templates.Where(p => p.Tier == tier).ToList();
        }

        public static float GetAbilityValue(PetAbilityDef ability, PetGrade grade)
        {
            EnsureLoaded();
            float mult = _gradeMultipliers[grade];
            return ability.BaseValue + ability.GradeScale * (mult - 1f);
        }

        public static string GetAbilityDescription(string petId, PetGrade grade)
        {
            EnsureLoaded();
            var template = _templates.FirstOrDefault(p => p.Id == petId);
            if (template == null) return "";
            var ab = template.Ability;
            float val = GetAbilityValue(ab, grade);

            string label;
            _abilityLabels.TryGetValue(ab.PassiveType, out label);
            if (label == null) label = ab.PassiveType.ToString();

            switch (ab.PassiveType)
            {
                case PassiveType.STAT_MODIFIER:
                    string statLabel;
                    _statLabels.TryGetValue(ab.Stat.Value, out statLabel);
                    if (statLabel == null) statLabel = ab.Stat.ToString();
                    return $"{statLabel} +{Pct(val)}";
                case PassiveType.COUNTER:
                    return $"{label} \ud655\ub960 {Pct(val)}";
                case PassiveType.LIFESTEAL:
                    return $"{label} {Pct(val)}";
                case PassiveType.SHIELD_ON_START:
                    return $"{label} (\ucd5c\ub300\uccb4\ub825 {Pct(val)})";
                case PassiveType.REVIVE:
                    return $"{label} (\uccb4\ub825 {Pct(val)} \ud68c\ubcf5)";
                case PassiveType.REGEN:
                    return $"\ub9e4\ud134 HP +{Mathf.RoundToInt(val)}";
                case PassiveType.MULTI_HIT:
                    return $"{label} \ud655\ub960 {Pct(val)}";
                default:
                    return label;
            }
        }

        public static Dictionary<PetGrade, float> GradeMultipliers
        {
            get { EnsureLoaded(); return _gradeMultipliers; }
        }

        private static string Pct(float v)
        {
            return $"{Mathf.RoundToInt(v * 100f)}%";
        }
    }
}
