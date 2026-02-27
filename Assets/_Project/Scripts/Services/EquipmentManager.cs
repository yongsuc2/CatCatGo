using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Services
{
    public class EquipmentManager
    {
        private static readonly SlotType[] AllSlotTypes =
        {
            SlotType.WEAPON, SlotType.ARMOR, SlotType.RING,
            SlotType.NECKLACE, SlotType.SHOES, SlotType.GLOVES, SlotType.HAT,
        };

        public int CompareEquipment(Equipment a, Equipment b)
        {
            if (a.GetGradeIndex() != b.GetGradeIndex())
                return a.GetGradeIndex() - b.GetGradeIndex();

            if (a.IsS != b.IsS)
                return a.IsS ? 1 : -1;

            var statsA = a.GetStats();
            var statsB = b.GetStats();
            return (statsA.Atk + statsA.MaxHp) - (statsB.Atk + statsB.MaxHp);
        }

        public List<Equipment> AutoEquipBest(Player player, List<Equipment> inventory)
        {
            var replaced = new List<Equipment>();

            foreach (var slotType in AllSlotTypes)
            {
                var slot = player.GetEquipmentSlot(slotType);
                var candidates = inventory.Where(e => e.Slot == slotType).ToList();

                candidates.Sort((a, b) => CompareEquipment(b, a));

                for (int i = 0; i < slot.MaxCount && i < candidates.Count; i++)
                {
                    var candidate = candidates[i];
                    var current = slot.Equipped[i];

                    if (current == null || candidate.IsBetterThan(current))
                    {
                        var result = slot.Equip(candidate, i);
                        if (result.IsOk() && result.Data.Replaced != null)
                        {
                            replaced.Add(result.Data.Replaced);
                        }
                    }
                }
            }

            return replaced;
        }

        public (SlotType Slot, int Index)? GetUpgradePriority(Player player)
        {
            var priorities = new List<(SlotType slot, int index, float score)>();

            var prioritySlots = new[]
            {
                SlotType.WEAPON, SlotType.RING, SlotType.GLOVES,
                SlotType.ARMOR, SlotType.NECKLACE, SlotType.SHOES, SlotType.HAT,
            };

            foreach (var slotType in prioritySlots)
            {
                var slot = player.GetEquipmentSlot(slotType);
                for (int i = 0; i < slot.MaxCount; i++)
                {
                    var eq = slot.Equipped[i];
                    if (eq != null && !eq.NeedsPromote())
                    {
                        float baseScore = slotType == SlotType.WEAPON ? 7
                            : slotType == SlotType.RING ? 6
                            : slotType == SlotType.GLOVES ? 5
                            : slotType == SlotType.ARMOR ? 4
                            : slotType == SlotType.NECKLACE ? 3
                            : slotType == SlotType.SHOES ? 2
                            : 1;
                        float gradeBonus = eq.IsS ? 2 : 0;
                        priorities.Add((slotType, i, baseScore + gradeBonus - eq.Level * 0.01f));
                    }
                }
            }

            priorities.Sort((a, b) => b.score.CompareTo(a.score));
            if (priorities.Count > 0)
                return (priorities[0].slot, priorities[0].index);

            return null;
        }
    }
}
