using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public class GachaGradeWeight
    {
        public EquipmentGrade Grade;
        public float Weight;
    }

    public class GachaChestConfig
    {
        public int CostPerPull;
        public int PityThreshold;
        public List<GachaGradeWeight> GradeWeights;
    }

    public class GachaPetConfig
    {
        public int CostPerPull;
        public int PityThreshold;
        public int EggAmount;
        public int FoodMin;
        public int FoodMax;
    }

    public class GachaGemConfig
    {
        public int CostPerPull;
        public int PityThreshold;
        public int GemsMin;
        public int GemsMax;
    }

    public static class GachaDataTable
    {
        private static GachaChestConfig _equipment;
        private static GachaPetConfig _pet;
        private static GachaGemConfig _gem;
        private static float _sRate;
        private static HashSet<EquipmentGrade> _sEligibleGrades;

        private static void EnsureLoaded()
        {
            if (_equipment != null) return;

            var data = JsonDataLoader.LoadJObject("gacha.data.json");
            if (data == null) return;

            var eq = data["equipment"];
            _equipment = new GachaChestConfig
            {
                CostPerPull = eq["costPerPull"].Value<int>(),
                PityThreshold = eq["pityThreshold"].Value<int>(),
                GradeWeights = eq["gradeWeights"].Select(w => new GachaGradeWeight
                {
                    Grade = (EquipmentGrade)System.Enum.Parse(typeof(EquipmentGrade), w["grade"].ToString()),
                    Weight = w["weight"].Value<float>(),
                }).ToList(),
            };
            _sRate = eq["sRate"].Value<float>();
            _sEligibleGrades = new HashSet<EquipmentGrade>(
                eq["sEligibleGrades"].Select(g => (EquipmentGrade)System.Enum.Parse(typeof(EquipmentGrade), g.ToString()))
            );

            var p = data["pet"];
            _pet = new GachaPetConfig
            {
                CostPerPull = p["costPerPull"].Value<int>(),
                PityThreshold = p["pityThreshold"].Value<int>(),
                EggAmount = p["eggAmount"].Value<int>(),
                FoodMin = p["foodMin"].Value<int>(),
                FoodMax = p["foodMax"].Value<int>(),
            };

            var g = data["gem"];
            _gem = new GachaGemConfig
            {
                CostPerPull = g["costPerPull"].Value<int>(),
                PityThreshold = g["pityThreshold"].Value<int>(),
                GemsMin = g["gemsMin"].Value<int>(),
                GemsMax = g["gemsMax"].Value<int>(),
            };
        }

        public static GachaChestConfig Equipment { get { EnsureLoaded(); return _equipment; } }
        public static GachaPetConfig Pet { get { EnsureLoaded(); return _pet; } }
        public static GachaGemConfig Gem { get { EnsureLoaded(); return _gem; } }
        public static float SRate { get { EnsureLoaded(); return _sRate; } }
        public static HashSet<EquipmentGrade> SEligibleGrades { get { EnsureLoaded(); return _sEligibleGrades; } }
    }
}
