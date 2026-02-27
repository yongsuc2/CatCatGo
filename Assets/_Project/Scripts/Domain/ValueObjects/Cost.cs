using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Domain.ValueObjects
{
    public struct CostEntry
    {
        public ResourceType Type;
        public int Amount;

        public CostEntry(ResourceType type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }

    public class Cost
    {
        public List<CostEntry> Entries { get; }

        public Cost(List<CostEntry> entries)
        {
            Entries = entries;
        }

        public static Cost Single(ResourceType type, int amount)
        {
            return new Cost(new List<CostEntry> { new CostEntry(type, amount) });
        }

        public static Cost Free()
        {
            return new Cost(new List<CostEntry>());
        }

        public bool IsEmpty()
        {
            return Entries.Count == 0;
        }

        public int GetAmount(ResourceType type)
        {
            var entry = Entries.FirstOrDefault(e => e.Type == type);
            return entry.Amount;
        }
    }
}
