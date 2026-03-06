using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace CatCatGo.Editor
{
    public static class ResourceValidator
    {
        private const string RESOURCES_ROOT = "Assets/_Project/Resources";
        private const string DATA_JSON_ROOT = "Assets/_Project/Data/Json";
        private const string CHARS_PATH = RESOURCES_ROOT + "/Chars";
        private const string ICONS_PATH = RESOURCES_ROOT + "/Icons";
        private const string STATUS_EFFECTS_PATH = RESOURCES_ROOT + "/StatusEffects";
        private const string SKILL_ICONS_PATH = ICONS_PATH + "/skill";
        private const string EQUIP_ICONS_PATH = ICONS_PATH + "/equip";

        private static readonly string[] REQUIRED_ANIM_TYPES = { "idle", "attack" };
        private static readonly string[] OPTIONAL_ANIM_TYPES = { "walk" };

        private static readonly string[] STATUS_EFFECT_NAMES =
        {
            "status_poison", "status_burn", "status_regen",
            "status_atk_up", "status_atk_down",
            "status_def_up", "status_def_down",
            "status_crit_up", "status_stun",
        };

        private static readonly string[] SLOT_TYPES = { "armor", "gloves", "hat", "necklace", "ring", "shoes", "weapon" };
        private static readonly string[] EQUIP_GRADES = { "common", "uncommon", "rare", "epic", "legendary", "mythic" };

        [MenuItem("Tools/Resource Validator/Run All Checks")]
        public static void RunAll()
        {
            var results = new List<ValidationResult>();
            results.AddRange(ValidateCharacterResources());
            results.AddRange(ValidateStatusEffectIcons());
            results.AddRange(ValidateEquipmentIcons());
            results.AddRange(ValidateSkillIcons());
            results.AddRange(ValidateJsonDataFiles());
            results.AddRange(ValidateUnusedCharResources());
            PrintResults("All Checks", results);
        }

        [MenuItem("Tools/Resource Validator/Character Resources")]
        public static void RunCharacterCheck()
        {
            PrintResults("Character Resources", ValidateCharacterResources());
        }

        [MenuItem("Tools/Resource Validator/Status Effect Icons")]
        public static void RunStatusEffectCheck()
        {
            PrintResults("Status Effect Icons", ValidateStatusEffectIcons());
        }

        [MenuItem("Tools/Resource Validator/Equipment Icons")]
        public static void RunEquipmentCheck()
        {
            PrintResults("Equipment Icons", ValidateEquipmentIcons());
        }

        [MenuItem("Tools/Resource Validator/Skill Icons")]
        public static void RunSkillIconCheck()
        {
            PrintResults("Skill Icons", ValidateSkillIcons());
        }

        [MenuItem("Tools/Resource Validator/JSON Data Files")]
        public static void RunJsonDataCheck()
        {
            PrintResults("JSON Data Files", ValidateJsonDataFiles());
        }

        [MenuItem("Tools/Resource Validator/Unused Character Resources")]
        public static void RunUnusedCharCheck()
        {
            PrintResults("Unused Character Resources", ValidateUnusedCharResources());
        }

        private static List<ValidationResult> ValidateCharacterResources()
        {
            var results = new List<ValidationResult>();
            var enemyIds = LoadEnemyIds();
            if (enemyIds == null)
            {
                results.Add(ValidationResult.Error("enemy.data.json", "enemy.data.json load failed"));
                return results;
            }

            var allCharIds = new HashSet<string>(enemyIds) { "player" };

            foreach (var id in allCharIds)
            {
                string charDir = Path.Combine(CHARS_PATH, id).Replace("\\", "/");
                if (!AssetDatabase.IsValidFolder(charDir))
                {
                    results.Add(ValidationResult.Error(charDir, $"Character folder missing: {id}"));
                    continue;
                }

                foreach (var animType in REQUIRED_ANIM_TYPES)
                {
                    string animDir = Path.Combine(charDir, animType).Replace("\\", "/");
                    if (!AssetDatabase.IsValidFolder(animDir))
                    {
                        results.Add(ValidationResult.Error(animDir, $"Required anim folder missing: {id}/{animType}"));
                        continue;
                    }

                    int frameCount = CountFrames(animDir);
                    if (frameCount == 0)
                        results.Add(ValidationResult.Error(animDir, $"No animation frames: {id}/{animType}"));
                    else if (frameCount < 3)
                        results.Add(ValidationResult.Warn(animDir, $"Very few frames ({frameCount}): {id}/{animType}"));
                }

                foreach (var animType in OPTIONAL_ANIM_TYPES)
                {
                    string animDir = Path.Combine(charDir, animType).Replace("\\", "/");
                    if (!AssetDatabase.IsValidFolder(animDir))
                        results.Add(ValidationResult.Info(animDir, $"Optional anim folder missing: {id}/{animType}"));
                }

                ValidateFrameSequence(charDir, results, id);
            }

            return results;
        }

        private static void ValidateFrameSequence(string charDir, List<ValidationResult> results, string id)
        {
            foreach (var animType in REQUIRED_ANIM_TYPES)
            {
                string animDir = Path.Combine(charDir, animType).Replace("\\", "/");
                if (!AssetDatabase.IsValidFolder(animDir)) continue;

                var pngGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { animDir });
                var frameNumbers = new List<int>();
                foreach (var guid in pngGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    if (fileName.StartsWith("frame_") && int.TryParse(fileName.Substring(6), out int num))
                        frameNumbers.Add(num);
                }

                if (frameNumbers.Count == 0) continue;

                frameNumbers.Sort();
                if (frameNumbers[0] != 1)
                    results.Add(ValidationResult.Warn(animDir, $"Frame sequence doesn't start at 1 (starts at {frameNumbers[0]}): {id}/{animType}"));

                for (int i = 1; i < frameNumbers.Count; i++)
                {
                    if (frameNumbers[i] != frameNumbers[i - 1] + 1)
                    {
                        results.Add(ValidationResult.Warn(animDir, $"Frame gap: frame_{frameNumbers[i - 1]:D4} -> frame_{frameNumbers[i]:D4}: {id}/{animType}"));
                        break;
                    }
                }
            }
        }

        private static List<ValidationResult> ValidateStatusEffectIcons()
        {
            var results = new List<ValidationResult>();
            foreach (var name in STATUS_EFFECT_NAMES)
            {
                string path = $"{STATUS_EFFECTS_PATH}/{name}.png";
                if (!File.Exists(path))
                    results.Add(ValidationResult.Error(path, $"Status effect icon missing: {name}"));
            }
            return results;
        }

        private static List<ValidationResult> ValidateEquipmentIcons()
        {
            var results = new List<ValidationResult>();
            foreach (var slot in SLOT_TYPES)
            {
                foreach (var grade in EQUIP_GRADES)
                {
                    string fileName = $"equip_{slot}_{grade}";
                    string path = $"{EQUIP_ICONS_PATH}/{fileName}.png";
                    if (!File.Exists(path))
                        results.Add(ValidationResult.Error(path, $"Equipment icon missing: {fileName}"));
                }
            }
            return results;
        }

        private static List<ValidationResult> ValidateSkillIcons()
        {
            var results = new List<ValidationResult>();
            var skillIds = LoadActiveSkillIds();
            if (skillIds == null)
            {
                results.Add(ValidationResult.Warn(DATA_JSON_ROOT, "active-skill-tier.data.json load failed, skipping skill icon check"));
                return results;
            }

            foreach (var skillId in skillIds)
            {
                string fileName = $"icon_skill_{skillId}";
                string path = $"{SKILL_ICONS_PATH}/{fileName}.png";
                if (!File.Exists(path))
                    results.Add(ValidationResult.Warn(path, $"Skill icon missing: {fileName}"));
            }

            return results;
        }

        private static List<ValidationResult> ValidateJsonDataFiles()
        {
            var results = new List<ValidationResult>();
            if (!Directory.Exists(DATA_JSON_ROOT))
            {
                results.Add(ValidationResult.Error(DATA_JSON_ROOT, "Data/Json directory not found"));
                return results;
            }

            string resourcesCopy = "Assets/Resources/_Project/Data/Json";

            var jsonFiles = Directory.GetFiles(DATA_JSON_ROOT, "*.data.json");
            foreach (var file in jsonFiles)
            {
                string fileName = Path.GetFileName(file);
                string copyPath = Path.Combine(resourcesCopy, fileName).Replace("\\", "/");

                if (!File.Exists(copyPath))
                {
                    results.Add(ValidationResult.Error(copyPath, $"Resources copy missing: {fileName}"));
                    continue;
                }

                string original = File.ReadAllText(file).Replace("\r\n", "\n").Trim();
                string copy = File.ReadAllText(copyPath).Replace("\r\n", "\n").Trim();
                if (original != copy)
                    results.Add(ValidationResult.Warn(copyPath, $"Resources copy out of sync with original: {fileName}"));
            }

            var resourceFiles = Directory.Exists(resourcesCopy)
                ? Directory.GetFiles(resourcesCopy, "*.data.json")
                : Array.Empty<string>();
            var originalNames = new HashSet<string>(jsonFiles.Select(Path.GetFileName));
            foreach (var rf in resourceFiles)
            {
                string rfName = Path.GetFileName(rf);
                if (!originalNames.Contains(rfName))
                    results.Add(ValidationResult.Warn(rf, $"Resources copy has no original: {rfName}"));
            }

            return results;
        }

        private static List<ValidationResult> ValidateUnusedCharResources()
        {
            var results = new List<ValidationResult>();
            var enemyIds = LoadEnemyIds();
            if (enemyIds == null) return results;

            var referencedIds = new HashSet<string>(enemyIds) { "player" };

            if (!Directory.Exists(CHARS_PATH)) return results;

            var charDirs = Directory.GetDirectories(CHARS_PATH)
                .Select(d => Path.GetFileName(d))
                .Where(n => !n.EndsWith(".meta"));

            foreach (var dirName in charDirs)
            {
                if (!referencedIds.Contains(dirName))
                    results.Add(ValidationResult.Warn($"{CHARS_PATH}/{dirName}", $"Character folder not referenced in enemy.data.json: {dirName}"));
            }

            return results;
        }

        private static HashSet<string> LoadEnemyIds()
        {
            string path = Path.Combine(DATA_JSON_ROOT, "enemy.data.json").Replace("\\", "/");
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                var obj = JObject.Parse(json);
                var templates = obj["templates"] as JArray;
                if (templates == null) return null;

                var ids = new HashSet<string>();
                foreach (var t in templates)
                {
                    string id = t["id"]?.ToString();
                    if (!string.IsNullOrEmpty(id))
                        ids.Add(id);
                }
                return ids;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ResourceValidator] Failed to parse enemy.data.json: {e.Message}");
                return null;
            }
        }

        private static HashSet<string> LoadActiveSkillIds()
        {
            string path = Path.Combine(DATA_JSON_ROOT, "active-skill-tier.data.json").Replace("\\", "/");
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                var arr = JArray.Parse(json);
                var ids = new HashSet<string>();
                foreach (var item in arr)
                {
                    string skill = item["skill"]?.ToString();
                    if (!string.IsNullOrEmpty(skill))
                        ids.Add(skill);
                }
                return ids;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ResourceValidator] Failed to parse active-skill-tier.data.json: {e.Message}");
                return null;
            }
        }

        private static int CountFrames(string animDir)
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { animDir });
            return guids.Count(guid =>
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                return Path.GetFileNameWithoutExtension(p).StartsWith("frame_");
            });
        }

        private static void PrintResults(string category, List<ValidationResult> results)
        {
            int errors = results.Count(r => r.Level == ValidationLevel.Error);
            int warnings = results.Count(r => r.Level == ValidationLevel.Warning);
            int infos = results.Count(r => r.Level == ValidationLevel.Info);

            foreach (var r in results)
            {
                switch (r.Level)
                {
                    case ValidationLevel.Error:
                        Debug.LogError($"[ResourceValidator] {r.Message} | {r.Path}");
                        break;
                    case ValidationLevel.Warning:
                        Debug.LogWarning($"[ResourceValidator] {r.Message} | {r.Path}");
                        break;
                    case ValidationLevel.Info:
                        Debug.Log($"[ResourceValidator] {r.Message} | {r.Path}");
                        break;
                }
            }

            string summary = errors == 0 && warnings == 0
                ? $"[ResourceValidator] {category}: All passed."
                : $"[ResourceValidator] {category}: {errors} errors, {warnings} warnings, {infos} infos";
            Debug.Log(summary);
        }

        private enum ValidationLevel { Error, Warning, Info }

        private struct ValidationResult
        {
            public ValidationLevel Level;
            public string Path;
            public string Message;

            public static ValidationResult Error(string path, string message)
                => new ValidationResult { Level = ValidationLevel.Error, Path = path, Message = message };
            public static ValidationResult Warn(string path, string message)
                => new ValidationResult { Level = ValidationLevel.Warning, Path = path, Message = message };
            public static ValidationResult Info(string path, string message)
                => new ValidationResult { Level = ValidationLevel.Info, Path = path, Message = message };
        }
    }
}
