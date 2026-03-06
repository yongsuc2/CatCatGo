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

    public class ChapterTheme
    {
        public int MinChapter;
        public int MaxChapter;
        public string[] Enemy;
        public string[] Elite;
        public string[] Boss;
        public List<BossRotationEntry> BossRotation;
    }

    public class BossRotationEntry
    {
        public string Elite;
        public string MidBoss;
        public string Boss;
    }

    public static class EnemyTable
    {
        private static List<EnemyTemplateData> _templates;
        private static string[] _allEnemyIds;
        private static string[] _allBossIds;
        private static List<ChapterTheme> _chapterThemes;

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

            _allEnemyIds = _templates.Where(t => !t.IsBoss && !t.Id.StartsWith("dungeon_")).Select(t => t.Id).ToArray();
            _allBossIds = _templates.Where(t => t.IsBoss && !t.Id.StartsWith("dungeon_")).Select(t => t.Id).ToArray();

            _chapterThemes = new List<ChapterTheme>();
            var themesToken = data["chapterThemes"];
            if (themesToken != null)
            {
                foreach (var th in themesToken)
                {
                    var theme = new ChapterTheme
                    {
                        MinChapter = th["minChapter"].Value<int>(),
                        MaxChapter = th["maxChapter"].Value<int>(),
                        Enemy = th["enemy"].Select(s => s.ToString()).ToArray(),
                        Elite = th["elite"].Select(s => s.ToString()).ToArray(),
                        Boss = th["boss"].Select(s => s.ToString()).ToArray(),
                        BossRotation = new List<BossRotationEntry>(),
                    };
                    foreach (var rot in th["bossRotation"])
                    {
                        theme.BossRotation.Add(new BossRotationEntry
                        {
                            Elite = rot["elite"].ToString(),
                            MidBoss = rot["midBoss"].ToString(),
                            Boss = rot["boss"].ToString(),
                        });
                    }
                    _chapterThemes.Add(theme);
                }
            }
        }

        private static ChapterTheme GetThemeForChapter(int chapterId)
        {
            EnsureLoaded();
            if (_chapterThemes == null) return null;
            return _chapterThemes.FirstOrDefault(t => chapterId >= t.MinChapter && chapterId <= t.MaxChapter);
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

        public static string[] GetEnemyPoolForChapter(int chapterId)
        {
            var theme = GetThemeForChapter(chapterId);
            if (theme != null) return theme.Enemy;
            EnsureLoaded();
            return _allEnemyIds;
        }

        public static string[] GetElitePoolForChapter(int chapterId)
        {
            var theme = GetThemeForChapter(chapterId);
            if (theme != null) return theme.Elite;
            EnsureLoaded();
            return _allEnemyIds;
        }

        public static string[] GetBossPoolForChapter(int chapterId)
        {
            var theme = GetThemeForChapter(chapterId);
            if (theme != null) return theme.Boss;
            EnsureLoaded();
            return _allBossIds;
        }

        public static BossRotationEntry GetBossAssignmentForChapter(int chapterId)
        {
            var theme = GetThemeForChapter(chapterId);
            if (theme != null && theme.BossRotation.Count > 0)
            {
                int themeLocalIndex = chapterId - theme.MinChapter;
                return theme.BossRotation[themeLocalIndex % theme.BossRotation.Count];
            }
            EnsureLoaded();
            if (_chapterThemes != null && _chapterThemes.Count > 0 && _chapterThemes[0].BossRotation.Count > 0)
            {
                return _chapterThemes[0].BossRotation[(chapterId - 1) % _chapterThemes[0].BossRotation.Count];
            }
            return new BossRotationEntry
            {
                Elite = _allEnemyIds[0],
                MidBoss = _allBossIds[0],
                Boss = _allBossIds[0],
            };
        }

        public static string GetRandomEnemyId()
        {
            EnsureLoaded();
            return _allEnemyIds[UnityEngine.Random.Range(0, _allEnemyIds.Length)];
        }

        public static string GetRandomBossId()
        {
            EnsureLoaded();
            return _allBossIds[UnityEngine.Random.Range(0, _allBossIds.Length)];
        }
    }
}
