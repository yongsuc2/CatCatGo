using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public class EnemyTemplateData
    {
        public string Id;
        public string Name;
        public Stats BaseStats;
        public string[] SkillIds;
        public bool IsBoss;
        public int RagePerAttack;
    }

    public static class EnemyTable
    {
        private static List<EnemyTemplateData> _templates;
        private static string[] _enemyPool;
        private static string[] _elitePool;
        private static string[] _bossPool;
        private static Stats _baseEnemyStats;
        private static Stats _baseBossStats;

        private static void EnsureLoaded()
        {
            if (_templates != null) return;

            var data = JsonDataLoader.LoadJObject("enemy.data.json");
            if (data == null) return;

            _templates = new List<EnemyTemplateData>();
            foreach (var t in data["templates"])
            {
                _templates.Add(new EnemyTemplateData
                {
                    Id = t["id"].ToString(),
                    Name = t["name"].ToString(),
                    BaseStats = Stats.Create(
                        hp: t["hp"].Value<int>(),
                        maxHp: t["hp"].Value<int>(),
                        atk: t["atk"].Value<int>(),
                        def: t["def"].Value<int>(),
                        crit: t["crit"].Value<float>()
                    ),
                    SkillIds = t["skillIds"].Select(s => s.ToString()).ToArray(),
                    IsBoss = t["isBoss"].Value<bool>(),
                    RagePerAttack = t["ragePerAttack"]?.Value<int>() ?? 0,
                });
            }

            _enemyPool = data["pools"]["enemy"].Select(s => s.ToString()).ToArray();
            _elitePool = data["pools"]["elite"].Select(s => s.ToString()).ToArray();
            _bossPool = data["pools"]["boss"].Select(s => s.ToString()).ToArray();

            var bs = data["baseStats"];
            var e = bs["enemy"];
            _baseEnemyStats = Stats.Create(
                hp: e["hp"].Value<int>(), maxHp: e["hp"].Value<int>(),
                atk: e["atk"].Value<int>(), def: e["def"].Value<int>(),
                crit: e["crit"]?.Value<float>() ?? 0f
            );
            var b = bs["boss"];
            _baseBossStats = Stats.Create(
                hp: b["hp"].Value<int>(), maxHp: b["hp"].Value<int>(),
                atk: b["atk"].Value<int>(), def: b["def"].Value<int>(),
                crit: b["crit"]?.Value<float>() ?? 0f
            );
        }

        public static EnemyTemplateData GetTemplate(string id)
        {
            EnsureLoaded();
            return _templates.FirstOrDefault(e => e.Id == id);
        }

        public static Stats GetScaledStats(Stats baseStats, int chapterLevel)
        {
            EnsureLoaded();
            float factor = Mathf.Pow(BattleDataTable.Data.Enemy.ScalingPerChapter, chapterLevel - 1);
            return baseStats.Multiply(factor);
        }

        public static Stats GetTowerScaledStats(Stats baseStats, int floor)
        {
            EnsureLoaded();
            float factor = Mathf.Pow(BattleDataTable.Data.Enemy.ScalingPerTowerFloor, floor - 1);
            return baseStats.Multiply(factor);
        }

        public static string GetRandomEnemyId()
        {
            EnsureLoaded();
            return _enemyPool[UnityEngine.Random.Range(0, _enemyPool.Length)];
        }

        public static string GetRandomEliteId()
        {
            EnsureLoaded();
            return _elitePool[UnityEngine.Random.Range(0, _elitePool.Length)];
        }

        public static string GetRandomBossId()
        {
            EnsureLoaded();
            return _bossPool[UnityEngine.Random.Range(0, _bossPool.Length)];
        }

        public static string[] GetChapterEnemyPool()
        {
            EnsureLoaded();
            return _enemyPool;
        }

        public static string[] GetChapterBossPool()
        {
            EnsureLoaded();
            return _bossPool;
        }

        public static Stats GetBaseEnemyStats()
        {
            EnsureLoaded();
            return _baseEnemyStats;
        }

        public static Stats GetBaseBossStats()
        {
            EnsureLoaded();
            return _baseBossStats;
        }
    }
}
