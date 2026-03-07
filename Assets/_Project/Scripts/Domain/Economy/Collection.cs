using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Data;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Domain.Economy
{
    public class CollectionEntry
    {
        public string Id;
        public string Name;
        public Stats BonusStats;
        public bool Acquired;
    }

    public class Collection
    {
        public Dictionary<string, CollectionEntry> Entries;

        public Collection()
        {
            Entries = new Dictionary<string, CollectionEntry>();
            foreach (var data in CollectionDataTable.GetAllEntries())
            {
                Entries[data.Id] = new CollectionEntry
                {
                    Id = data.Id,
                    Name = data.Name,
                    BonusStats = data.BonusStats,
                    Acquired = false,
                };
            }
        }

        public bool Acquire(string id)
        {
            if (!Entries.TryGetValue(id, out var entry) || entry.Acquired) return false;
            entry.Acquired = true;
            return true;
        }

        public bool IsAcquired(string id)
        {
            return Entries.TryGetValue(id, out var entry) && entry.Acquired;
        }

        public Stats GetTotalBonus()
        {
            var total = Stats.Zero;
            foreach (var entry in Entries.Values)
            {
                if (entry.Acquired)
                    total = total.Add(entry.BonusStats);
            }
            return total;
        }

        public int GetAcquiredCount()
        {
            int count = 0;
            foreach (var entry in Entries.Values)
            {
                if (entry.Acquired) count++;
            }
            return count;
        }

        public int GetTotalCount()
        {
            return Entries.Count;
        }

        public float GetProgress()
        {
            return GetTotalCount() > 0 ? (float)GetAcquiredCount() / GetTotalCount() : 0;
        }

        public List<CollectionEntry> GetAllEntries()
        {
            return Entries.Values.ToList();
        }
    }
}
