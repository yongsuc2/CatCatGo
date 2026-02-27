using System.Collections.Generic;
using System.Linq;
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
        private static readonly (string id, string name, Stats stats)[] CollectionData =
        {
            ("col_sword_1", "Ancient Sword", Stats.Create(atk: 5)),
            ("col_shield_1", "Iron Shield", Stats.Create(def: 3)),
            ("col_ring_1", "Mystic Ring", Stats.Create(atk: 3, crit: 0.01f)),
            ("col_armor_1", "Dragon Scale", Stats.Create(maxHp: 50, def: 2)),
            ("col_gem_1", "Star Gem", Stats.Create(atk: 2, maxHp: 20)),
            ("col_crown_1", "Golden Crown", Stats.Create(atk: 8)),
            ("col_amulet_1", "Shadow Amulet", Stats.Create(crit: 0.02f)),
            ("col_boots_1", "Wind Boots", Stats.Create(atk: 4, def: 1)),
            ("col_cape_1", "Phoenix Cape", Stats.Create(maxHp: 80)),
            ("col_orb_1", "Thunder Orb", Stats.Create(atk: 6)),
        };

        public Dictionary<string, CollectionEntry> Entries;

        public Collection()
        {
            Entries = new Dictionary<string, CollectionEntry>();
            foreach (var data in CollectionData)
            {
                Entries[data.id] = new CollectionEntry
                {
                    Id = data.id,
                    Name = data.name,
                    BonusStats = data.stats,
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
