using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public static class ResourceDataTable
    {
        private static Dictionary<ResourceType, string> _labels;
        private static Dictionary<ResourceType, string> _shortLabels;
        private static Dictionary<ResourceType, string> _colors;
        private static Dictionary<DungeonType, string> _dungeonLabels;

        private static void EnsureLoaded()
        {
            if (_labels != null) return;
            var data = JsonDataLoader.LoadJObject("resource-labels.data.json");

            _labels = ParseEnumDict<ResourceType>(data["labels"] as JObject);
            _shortLabels = ParseEnumDict<ResourceType>(data["shortLabels"] as JObject);
            _colors = ParseEnumDict<ResourceType>(data["colors"] as JObject);
            _dungeonLabels = ParseEnumDict<DungeonType>(data["dungeonLabels"] as JObject);
        }

        private static Dictionary<T, string> ParseEnumDict<T>(JObject obj) where T : struct, Enum
        {
            var dict = new Dictionary<T, string>();
            if (obj == null) return dict;
            foreach (var kv in obj)
            {
                if (Enum.TryParse<T>(kv.Key, out var key))
                    dict[key] = kv.Value.ToString();
            }
            return dict;
        }

        public static string GetLabel(ResourceType type)
        {
            EnsureLoaded();
            return _labels.TryGetValue(type, out var v) ? v : type.ToString();
        }

        public static string GetShortLabel(ResourceType type)
        {
            EnsureLoaded();
            return _shortLabels.TryGetValue(type, out var v) ? v : type.ToString();
        }

        public static string GetColor(ResourceType type)
        {
            EnsureLoaded();
            return _colors.TryGetValue(type, out var v) ? v : "#888";
        }

        public static string GetDungeonLabel(DungeonType type)
        {
            EnsureLoaded();
            return _dungeonLabels.TryGetValue(type, out var v) ? v : type.ToString();
        }
    }
}
