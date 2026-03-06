using System.Collections.Generic;
using NUnit.Framework;
using Newtonsoft.Json;
using CatCatGo.Domain.Enums;
using CatCatGo.Services;

namespace CatCatGo.Tests.Services
{
    [TestFixture]
    public class SaveSerializerTests
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
        };

        [Test]
        public void SaveStateRoundTripViaJson()
        {
            var state = CreateTestSaveState();
            var json = JsonConvert.SerializeObject(state, JsonSettings);
            var restored = JsonConvert.DeserializeObject<SaveState>(json, JsonSettings);

            Assert.AreEqual(state.Player.Talent.AtkLevel, restored.Player.Talent.AtkLevel);
            Assert.AreEqual(state.Player.Talent.HpLevel, restored.Player.Talent.HpLevel);
            Assert.AreEqual(state.Player.Talent.DefLevel, restored.Player.Talent.DefLevel);
            Assert.AreEqual(state.Player.Heritage.Route, restored.Player.Heritage.Route);
            Assert.AreEqual(state.Player.Heritage.Level, restored.Player.Heritage.Level);
            Assert.AreEqual(state.Player.ClearedChapterMax, restored.Player.ClearedChapterMax);
            Assert.AreEqual(state.Player.ActivePetId, restored.Player.ActivePetId);
        }

        [Test]
        public void ResourcesRoundTripViaJson()
        {
            var state = CreateTestSaveState();
            state.Player.Resources["GOLD"] = 12345;
            state.Player.Resources["GEMS"] = 678;
            state.Player.Resources["STAMINA"] = 50;

            var json = JsonConvert.SerializeObject(state, JsonSettings);
            var restored = JsonConvert.DeserializeObject<SaveState>(json, JsonSettings);

            Assert.AreEqual(12345, restored.Player.Resources["GOLD"]);
            Assert.AreEqual(678, restored.Player.Resources["GEMS"]);
            Assert.AreEqual(50, restored.Player.Resources["STAMINA"]);
        }

        [Test]
        public void EquipmentDataRoundTripViaJson()
        {
            var equip = new EquipmentData
            {
                Id = "eq_001",
                Name = "Iron Sword",
                Slot = SlotType.WEAPON,
                Grade = EquipmentGrade.EPIC,
                IsS = true,
                Level = 15,
                PromoteCount = 1,
                WeaponSubType = WeaponSubType.SWORD,
                MergeLevel = 0,
                SubStats = new List<SubStatData>
                {
                    new SubStatData { Stat = "CRIT", Value = 0.05f },
                    new SubStatData { Stat = "MAXHP", Value = 50f }
                }
            };

            var json = JsonConvert.SerializeObject(equip, JsonSettings);
            var restored = JsonConvert.DeserializeObject<EquipmentData>(json, JsonSettings);

            Assert.AreEqual("eq_001", restored.Id);
            Assert.AreEqual(SlotType.WEAPON, restored.Slot);
            Assert.AreEqual(EquipmentGrade.EPIC, restored.Grade);
            Assert.IsTrue(restored.IsS);
            Assert.AreEqual(15, restored.Level);
            Assert.AreEqual(1, restored.PromoteCount);
            Assert.AreEqual(WeaponSubType.SWORD, restored.WeaponSubType);
            Assert.AreEqual(2, restored.SubStats.Count);
            Assert.AreEqual("CRIT", restored.SubStats[0].Stat);
        }

        [Test]
        public void PetDataRoundTripViaJson()
        {
            var pet = new PetData
            {
                Id = "pet_12345",
                Name = "FireCat",
                Tier = PetTier.S,
                Grade = PetGrade.EPIC,
                Level = 10,
                BasePassiveBonus = new StatBonusData { Atk = 5, MaxHp = 10 },
                Exp = 350
            };

            var json = JsonConvert.SerializeObject(pet, JsonSettings);
            var restored = JsonConvert.DeserializeObject<PetData>(json, JsonSettings);

            Assert.AreEqual("pet_12345", restored.Id);
            Assert.AreEqual(PetTier.S, restored.Tier);
            Assert.AreEqual(PetGrade.EPIC, restored.Grade);
            Assert.AreEqual(10, restored.Level);
            Assert.AreEqual(350, restored.Exp);
            Assert.AreEqual(5, restored.BasePassiveBonus.Atk);
        }

        [Test]
        public void AttendanceSaveDataRoundTripViaJson()
        {
            var attendance = new AttendanceSaveData
            {
                CheckedDays = new List<bool> { true, true, false, false, false, false, false },
                CycleStartDate = "2026-3-1",
                LastCheckDate = "2026-3-2"
            };

            var json = JsonConvert.SerializeObject(attendance, JsonSettings);
            var restored = JsonConvert.DeserializeObject<AttendanceSaveData>(json, JsonSettings);

            Assert.AreEqual(7, restored.CheckedDays.Count);
            Assert.IsTrue(restored.CheckedDays[0]);
            Assert.IsTrue(restored.CheckedDays[1]);
            Assert.IsFalse(restored.CheckedDays[2]);
            Assert.AreEqual("2026-3-1", restored.CycleStartDate);
        }

        [Test]
        public void DungeonsSaveDataRoundTripViaJson()
        {
            var dungeons = new DungeonsSaveData
            {
                TodayCount = 2,
                ClearedStages = new Dictionary<string, int>
                {
                    { "GIANT_BEEHIVE", 3 },
                    { "ANCIENT_TREE", 1 }
                }
            };

            var json = JsonConvert.SerializeObject(dungeons, JsonSettings);
            var restored = JsonConvert.DeserializeObject<DungeonsSaveData>(json, JsonSettings);

            Assert.AreEqual(2, restored.TodayCount);
            Assert.AreEqual(3, restored.ClearedStages["GIANT_BEEHIVE"]);
            Assert.AreEqual(1, restored.ClearedStages["ANCIENT_TREE"]);
        }

        [Test]
        public void ClaimedMilestonesRoundTripViaJson()
        {
            var state = CreateTestSaveState();
            state.Player.ClaimedMilestones = new List<string> { "talent_10", "talent_20", "talent_30" };

            var json = JsonConvert.SerializeObject(state, JsonSettings);
            var restored = JsonConvert.DeserializeObject<SaveState>(json, JsonSettings);

            Assert.AreEqual(3, restored.Player.ClaimedMilestones.Count);
            Assert.IsTrue(restored.Player.ClaimedMilestones.Contains("talent_20"));
        }

        [Test]
        public void BestSurvivalDaysRoundTripViaJson()
        {
            var state = CreateTestSaveState();
            state.Player.BestSurvivalDays = new Dictionary<string, int>
            {
                { "1", 45 },
                { "2", 30 }
            };

            var json = JsonConvert.SerializeObject(state, JsonSettings);
            var restored = JsonConvert.DeserializeObject<SaveState>(json, JsonSettings);

            Assert.AreEqual(45, restored.Player.BestSurvivalDays["1"]);
            Assert.AreEqual(30, restored.Player.BestSurvivalDays["2"]);
        }

        [Test]
        public void NullFieldsDeserializeGracefully()
        {
            var json = @"{
                ""Player"": {
                    ""Talent"": { ""AtkLevel"": 5, ""HpLevel"": 3, ""DefLevel"": 1 },
                    ""Heritage"": { ""Route"": ""SKULL"", ""Level"": 0 },
                    ""Resources"": { ""GOLD"": 100 },
                    ""EquipmentSlots"": {},
                    ""Inventory"": [],
                    ""OwnedPets"": [],
                    ""ClearedChapterMax"": 0,
                    ""ClaimedMilestones"": null,
                    ""BestSurvivalDays"": null,
                    ""SlotLevels"": null,
                    ""SlotPromoteCounts"": null,
                    ""ActivePetId"": null
                },
                ""Tower"": { ""CurrentFloor"": 1, ""CurrentStage"": 0 },
                ""Catacomb"": { ""HighestFloor"": 0 },
                ""Dungeons"": null,
                ""GoblinMiner"": { ""OreCount"": 0 },
                ""EquipmentChest"": null,
                ""Collection"": [],
                ""DailyReset"": { ""LastResetDate"": """" },
                ""Events"": [],
                ""Attendance"": null
            }";

            var restored = JsonConvert.DeserializeObject<SaveState>(json, JsonSettings);

            Assert.IsNotNull(restored);
            Assert.AreEqual(5, restored.Player.Talent.AtkLevel);
            Assert.IsNull(restored.Player.ClaimedMilestones);
            Assert.IsNull(restored.Player.BestSurvivalDays);
            Assert.IsNull(restored.Dungeons);
            Assert.IsNull(restored.Attendance);
            Assert.IsNull(restored.EquipmentChest);
        }

        private static SaveState CreateTestSaveState()
        {
            return new SaveState
            {
                Player = new PlayerSaveData
                {
                    Talent = new TalentSaveData { AtkLevel = 10, HpLevel = 8, DefLevel = 5 },
                    Heritage = new HeritageSaveData { Route = HeritageRoute.SKULL, Level = 3 },
                    Resources = new Dictionary<string, float> { { "GOLD", 1000 }, { "GEMS", 50 }, { "STAMINA", 100 } },
                    EquipmentSlots = new Dictionary<string, List<EquipmentData>>(),
                    SlotLevels = new Dictionary<string, List<int>>(),
                    SlotPromoteCounts = new Dictionary<string, List<int>>(),
                    Inventory = new List<EquipmentData>(),
                    ActivePetId = null,
                    OwnedPets = new List<PetData>(),
                    ClearedChapterMax = 3,
                    BestSurvivalDays = new Dictionary<string, int>(),
                    ClaimedMilestones = new List<string>(),
                },
                Tower = new TowerSaveData { CurrentFloor = 5, CurrentStage = 2 },
                Catacomb = new CatacombSaveData { HighestFloor = 10 },
                Dungeons = new DungeonsSaveData { TodayCount = 1, ClearedStages = new Dictionary<string, int>() },
                GoblinMiner = new GoblinMinerSaveData { OreCount = 15 },
                EquipmentChest = new EquipmentChestSaveData { PityCount = 42 },
                Collection = new List<string> { "col_001" },
                DailyReset = new DailyResetSaveData { LastResetDate = "2026-3-6" },
                Events = new List<EventData>(),
                Attendance = new AttendanceSaveData
                {
                    CheckedDays = new List<bool> { true, false, false, false, false, false, false },
                    CycleStartDate = "2026-3-1",
                    LastCheckDate = "2026-3-1",
                },
            };
        }
    }
}
