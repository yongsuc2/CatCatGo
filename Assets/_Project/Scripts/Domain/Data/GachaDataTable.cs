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
        public string CostCurrency;
        public int PityThreshold;
        public List<GachaGradeWeight> GradeWeights;
        public float SRate;
        public HashSet<EquipmentGrade> SEligibleGrades;
    }

    public class GachaPetConfig
    {
        public int CostPerPull;
        public string CostCurrency;
        public int PityThreshold;
        public int EggAmount;
        public int FoodMin;
        public int FoodMax;
    }

    public static class GachaDataTable
    {
        private static GachaChestConfig _equipment;
        private static GachaChestConfig _adventurerChest;
        private static GachaChestConfig _heroChest;
        private static GachaPetConfig _pet;
        private static GachaPetConfig _basicPet;

        private static void EnsureLoaded()
        {
            if (_equipment != null) return;

            var data = JsonDataLoader.LoadJObject("gacha.data.json");
            if (data == null) return;

            _equipment = ParseChestConfig(data["equipment"]);
            _adventurerChest = ParseChestConfig(data["adventurerChest"]);
            _heroChest = ParseChestConfig(data["heroChest"]);
            _pet = ParsePetConfig(data["pet"]);
            _basicPet = ParsePetConfig(data["basicPet"]);
        }

        private static GachaChestConfig ParseChestConfig(JToken token)
        {
            var config = new GachaChestConfig
            {
                CostPerPull = token["costPerPull"].Value<int>(),
                CostCurrency = token["costCurrency"]?.Value<string>() ?? "GEMS",
                PityThreshold = token["pityThreshold"].Value<int>(),
                GradeWeights = token["gradeWeights"].Select(w => new GachaGradeWeight
                {
                    Grade = (EquipmentGrade)System.Enum.Parse(typeof(EquipmentGrade), w["grade"].ToString()),
                    Weight = w["weight"].Value<float>(),
                }).ToList(),
                SRate = token["sRate"]?.Value<float>() ?? 0f,
            };

            var sGrades = token["sEligibleGrades"];
            config.SEligibleGrades = sGrades != null && sGrades.HasValues
                ? new HashSet<EquipmentGrade>(sGrades.Select(g => (EquipmentGrade)System.Enum.Parse(typeof(EquipmentGrade), g.ToString())))
                : new HashSet<EquipmentGrade>();

            return config;
        }

        private static GachaPetConfig ParsePetConfig(JToken token)
        {
            return new GachaPetConfig
            {
                CostPerPull = token["costPerPull"].Value<int>(),
                CostCurrency = token["costCurrency"]?.Value<string>() ?? "GEMS",
                PityThreshold = token["pityThreshold"].Value<int>(),
                EggAmount = token["eggAmount"].Value<int>(),
                FoodMin = token["foodMin"].Value<int>(),
                FoodMax = token["foodMax"].Value<int>(),
            };
        }

        public static GachaChestConfig Equipment { get { EnsureLoaded(); return _equipment; } }
        public static GachaChestConfig AdventurerChest { get { EnsureLoaded(); return _adventurerChest; } }
        public static GachaChestConfig HeroChest { get { EnsureLoaded(); return _heroChest; } }
        public static GachaPetConfig Pet { get { EnsureLoaded(); return _pet; } }
        public static GachaPetConfig BasicPet { get { EnsureLoaded(); return _basicPet; } }
    }
}
