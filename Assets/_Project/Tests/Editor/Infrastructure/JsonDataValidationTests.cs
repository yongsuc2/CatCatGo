using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using CatCatGo.Infrastructure;
using CatCatGo.Domain.Data;

namespace CatCatGo.Tests.Infrastructure
{
    [TestFixture]
    public class JsonDataValidationTests
    {
        private static readonly string JsonDir = Path.Combine(
            Directory.GetParent(UnityEngine.Application.dataPath).FullName,
            "Assets", "_Project", "Data", "Json");

        private static readonly string ResourceJsonDir = Path.Combine(
            UnityEngine.Application.dataPath,
            "Resources", "_Project", "Data", "Json");

        private static string[] AllJsonFiles()
        {
            return Directory.GetFiles(JsonDir, "*.data.json")
                .Select(Path.GetFileName)
                .ToArray();
        }

        // ── 1. 모든 JSON 파일 파싱 가능 여부 ──

        [Test]
        public void AllJsonFiles_ParseWithoutError()
        {
            var files = AllJsonFiles();
            Assert.IsNotEmpty(files, "JSON 데이터 파일이 없습니다");

            foreach (var file in files)
            {
                var path = Path.Combine(JsonDir, file);
                var raw = File.ReadAllText(path);
                Assert.DoesNotThrow(() => JToken.Parse(raw),
                    $"{file} 파싱 실패");
            }
        }

        // ── 2. Source/Resources 사본 동기화 확인 ──

        [Test]
        public void SourceAndResources_AreSynced()
        {
            if (!Directory.Exists(ResourceJsonDir))
            {
                Assert.Ignore("Resources JSON 디렉토리 없음 (빌드 전)");
                return;
            }

            var sourceFiles = AllJsonFiles();
            foreach (var file in sourceFiles)
            {
                var srcPath = Path.Combine(JsonDir, file);
                var resPath = Path.Combine(ResourceJsonDir, file);
                Assert.IsTrue(File.Exists(resPath),
                    $"Resources에 {file} 누락");

                var srcContent = File.ReadAllText(srcPath).Trim();
                var resContent = File.ReadAllText(resPath).Trim();
                Assert.AreEqual(srcContent, resContent,
                    $"{file} Source와 Resources 내용 불일치");
            }
        }

        // ── 3. 개별 DataTable 역직렬화 검증 ──

        [Test]
        public void BattleDataJson_DeserializesCorrectly()
        {
            var data = LoadFromFile<BattleData>("battle.data.json");
            Assert.IsNotNull(data.Damage, "damage 섹션 누락");
            Assert.IsNotNull(data.Rage, "rage 섹션 누락");
            Assert.IsNotNull(data.Enemy, "enemy 섹션 누락");
            Assert.IsNotNull(data.CombatGoldReward, "combatGoldReward 섹션 누락");
            Assert.IsNotNull(data.PlayerBaseStats, "playerBaseStats 섹션 누락");
            Assert.IsNotNull(data.DailyReset, "dailyReset 섹션 누락");
            Assert.IsNotNull(data.Stamina, "stamina 섹션 누락");
            Assert.IsNotNull(data.NewGameResources, "newGameResources 섹션 누락");
            Assert.Greater(data.MaxTurns, 0, "maxTurns는 0보다 커야 함");
            Assert.Greater(data.ChapterStaminaCost, 0, "chapterStaminaCost는 0보다 커야 함");
        }

        [Test]
        public void DungeonDataJson_DeserializesCorrectly()
        {
            var json = LoadJObjectFromFile("dungeon.data.json");
            Assert.IsNotNull(json["dailyLimit"], "dailyLimit 누락");
            Assert.IsNotNull(json["dungeons"], "dungeons 섹션 누락");
            Assert.IsNotNull(json["tower"], "tower 섹션 누락");
            Assert.IsNotNull(json["catacomb"], "catacomb 섹션 누락");
            Assert.IsNotNull(json["goblinMiner"], "goblinMiner 섹션 누락");
            Assert.Greater(json["dailyLimit"].Value<int>(), 0);
        }

        [Test]
        public void PetDataJson_DeserializesCorrectly()
        {
            var json = LoadJObjectFromFile("pet.data.json");
            Assert.IsNotNull(json["templates"], "templates 누락");
            Assert.IsNotNull(json["gradeMultipliers"], "gradeMultipliers 누락");
            Assert.IsNotNull(json["growth"], "growth 섹션 누락");

            var templates = json["templates"] as JArray;
            Assert.IsNotNull(templates);
            Assert.IsNotEmpty(templates);
            foreach (var pet in templates)
            {
                Assert.IsNotNull(pet["id"], "pet에 id 누락");
                Assert.IsNotNull(pet["tier"], "pet에 tier 누락");
            }
        }

        [Test]
        public void EnemyDataJson_DeserializesCorrectly()
        {
            var json = LoadJArrayFromFile("enemy.data.json");
            Assert.IsNotEmpty(json);
            foreach (var enemy in json)
            {
                Assert.IsNotNull(enemy["id"], "enemy에 id 누락");
                Assert.IsNotNull(enemy["name"], "enemy에 name 누락");
            }
        }

        [Test]
        public void ChapterTreasureDataJson_DeserializesCorrectly()
        {
            var json = LoadJObjectFromFile("chapter-treasure.data.json");
            Assert.IsNotNull(json["totalDays"], "totalDays 누락");
            Assert.IsNotNull(json["chapters"], "chapters 섹션 누락");
        }

        [Test]
        public void EncounterDataJson_DeserializesCorrectly()
        {
            var json = LoadJObjectFromFile("encounter.data.json");
            Assert.IsNotNull(json["events"], "events 섹션 누락");
        }

        [Test]
        public void GachaDataJson_DeserializesCorrectly()
        {
            var json = LoadJObjectFromFile("gacha.data.json");
            Assert.IsNotNull(json["singleCost"], "singleCost 누락");
            Assert.IsNotNull(json["tenPullCost"], "tenPullCost 누락");
        }

        [Test]
        public void QuestDataJson_DeserializesCorrectly()
        {
            var json = LoadJObjectFromFile("quest.data.json");
            Assert.IsNotNull(json["daily"], "daily 섹션 누락");
            Assert.IsNotNull(json["weekly"], "weekly 섹션 누락");
        }

        [Test]
        public void AttendanceDataJson_IsValidArray()
        {
            var json = LoadJArrayFromFile("attendance.data.json");
            Assert.IsNotEmpty(json);
            foreach (var entry in json)
                Assert.IsNotNull(entry["day"], "attendance에 day 누락");
        }

        [Test]
        public void CollectionDataJson_DeserializesCorrectly()
        {
            var json = LoadJObjectFromFile("collection.data.json");
            Assert.IsNotNull(json["collections"], "collections 섹션 누락");
        }

        [Test]
        public void TalentDataJson_DeserializesCorrectly()
        {
            var json = LoadJObjectFromFile("talent.data.json");
            Assert.IsNotNull(json["talents"], "talents 섹션 누락");
        }

        [Test]
        public void EquipmentConstantsDataJson_DeserializesCorrectly()
        {
            var json = LoadJObjectFromFile("equipment-constants.data.json");
            Assert.IsNotNull(json["enhancement"], "enhancement 섹션 누락");
        }

        [Test]
        public void EquipmentBaseStatsDataJson_IsValidArray()
        {
            var json = LoadJArrayFromFile("equipment-base-stats.data.json");
            Assert.IsNotEmpty(json);
            foreach (var entry in json)
            {
                Assert.IsNotNull(entry["slot"], "equipment에 slot 누락");
                Assert.IsNotNull(entry["grade"], "equipment에 grade 누락");
            }
        }

        [Test]
        public void ActiveSkillTierDataJson_IsValidArray()
        {
            var json = LoadJArrayFromFile("active-skill-tier.data.json");
            Assert.IsNotEmpty(json);
            foreach (var entry in json)
            {
                Assert.IsNotNull(entry["skill"], "skill 누락");
                Assert.IsNotNull(entry["tier"], "tier 누락");
            }
        }

        [Test]
        public void PassiveSkillTierDataJson_IsValidArray()
        {
            var json = LoadJArrayFromFile("passive-skill-tier.data.json");
            Assert.IsNotEmpty(json);
            foreach (var entry in json)
            {
                Assert.IsNotNull(entry["skill"], "skill 누락");
                Assert.IsNotNull(entry["tier"], "tier 누락");
            }
        }

        // ── 헬퍼 메서드 (파일 직접 읽기 — Resources.Load 불필요) ──

        private static T LoadFromFile<T>(string fileName)
        {
            var path = Path.Combine(JsonDir, fileName);
            Assert.IsTrue(File.Exists(path), $"{fileName} 파일 없음");
            var raw = File.ReadAllText(path);
            var result = JsonConvert.DeserializeObject<T>(raw);
            Assert.IsNotNull(result, $"{fileName} 역직렬화 결과 null");
            return result;
        }

        private static JObject LoadJObjectFromFile(string fileName)
        {
            var path = Path.Combine(JsonDir, fileName);
            Assert.IsTrue(File.Exists(path), $"{fileName} 파일 없음");
            var raw = File.ReadAllText(path);
            return JObject.Parse(raw);
        }

        private static JArray LoadJArrayFromFile(string fileName)
        {
            var path = Path.Combine(JsonDir, fileName);
            Assert.IsTrue(File.Exists(path), $"{fileName} 파일 없음");
            var raw = File.ReadAllText(path);
            return JArray.Parse(raw);
        }
    }
}
