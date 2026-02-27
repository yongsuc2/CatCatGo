using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Data;

namespace CatCatGo.Domain.Entities
{
    public class EquipmentSlot
    {
        public SlotType Type { get; }
        public int MaxCount { get; }
        public Equipment[] Equipped { get; }
        public int[] SlotLevels { get; }
        public int[] SlotPromoteCounts { get; }

        public EquipmentSlot(SlotType type)
        {
            Type = type;
            MaxCount = EquipmentTable.GetSlotMaxCount(type);
            Equipped = new Equipment[MaxCount];
            SlotLevels = new int[MaxCount];
            SlotPromoteCounts = new int[MaxCount];
        }

        public Result<EquipResult> Equip(Equipment equipment, int index = 0)
        {
            if (equipment.Slot != Type)
            {
                return Result.Fail<EquipResult>("Equipment slot type mismatch");
            }
            if (index < 0 || index >= MaxCount)
            {
                return Result.Fail<EquipResult>("Invalid slot index");
            }

            var replaced = Equipped[index];

            equipment.Level = SlotLevels[index];
            equipment.PromoteCount = SlotPromoteCounts[index];
            Equipped[index] = equipment;
            return Result.Ok(new EquipResult { Replaced = replaced });
        }

        public Result<UnequipResult> Unequip(int index)
        {
            if (index < 0 || index >= MaxCount)
            {
                return Result.Fail<UnequipResult>("Invalid slot index");
            }
            var equipment = Equipped[index];
            if (equipment == null)
            {
                return Result.Fail<UnequipResult>("No equipment in this slot");
            }
            Equipped[index] = null;
            return Result.Ok(new UnequipResult { Equipment = equipment });
        }

        public void SyncLevel(int index)
        {
            var eq = Equipped[index];
            if (eq != null)
            {
                SlotLevels[index] = eq.Level;
                SlotPromoteCounts[index] = eq.PromoteCount;
            }
        }

        public void InitFromEquipped()
        {
            for (int i = 0; i < MaxCount; i++)
            {
                var eq = Equipped[i];
                if (eq != null)
                {
                    SlotLevels[i] = eq.Level;
                    SlotPromoteCounts[i] = eq.PromoteCount;
                }
            }
        }

        public Stats GetTotalStats()
        {
            var total = Stats.Zero;
            foreach (var eq in Equipped)
            {
                if (eq != null)
                {
                    total = total.Add(eq.GetStats());
                }
            }
            return total;
        }

        public List<Equipment> GetEquipped()
        {
            return Equipped.Where(e => e != null).ToList();
        }

        public bool HasEmptySlot()
        {
            return Equipped.Any(e => e == null);
        }

        public int GetFirstEmptyIndex()
        {
            for (int i = 0; i < Equipped.Length; i++)
            {
                if (Equipped[i] == null) return i;
            }
            return -1;
        }
    }

    public struct EquipResult
    {
        public Equipment Replaced;
    }

    public struct UnequipResult
    {
        public Equipment Equipment;
    }
}
