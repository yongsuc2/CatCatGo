using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public class CollectionEntryData
    {
        public string Id;
        public string Name;
        public Stats BonusStats;
    }

    public static class CollectionDataTable
    {
        private static List<CollectionEntryData> _entries;

        private static void EnsureLoaded()
        {
            if (_entries != null) return;

            var data = JsonDataLoader.LoadJObject("collection.data.json");
            if (data == null) return;

            _entries = new List<CollectionEntryData>();
            foreach (var e in data["entries"])
            {
                _entries.Add(new CollectionEntryData
                {
                    Id = e["id"].ToString(),
                    Name = e["name"].ToString(),
                    BonusStats = Stats.Create(
                        atk: e["atk"].Value<int>(),
                        def: e["def"].Value<int>(),
                        maxHp: e["maxHp"].Value<int>(),
                        crit: e["crit"].Value<float>()
                    ),
                });
            }
        }

        public static List<CollectionEntryData> GetAllEntries()
        {
            EnsureLoaded();
            return _entries;
        }
    }
}
