using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Data;

namespace CatCatGo.Domain.Entities
{
    public struct StatsBreakdown
    {
        public Stats Base;
        public Stats Talent;
        public Stats Grade;
        public Stats Equipment;
        public Stats Heritage;
        public Stats Pet;
        public Stats Total;
    }

    public struct CombatPassives
    {
        public float CritDamage;
        public float LifestealRate;
        public float EvasionRate;
        public float CounterChance;
    }

    public class Player
    {
        public static Stats BaseStats
        {
            get
            {
                var cfg = BattleDataTable.Data.PlayerBaseStats;
                return Stats.Create(hp: cfg.Hp, maxHp: cfg.Hp, atk: cfg.Atk, def: cfg.Def, crit: 0f);
            }
        }


        public Talent Talent;
        public Heritage Heritage;
        public Dictionary<SlotType, EquipmentSlot> EquipmentSlots;
        public List<Equipment> Inventory;
        public Pet ActivePet;
        public List<Pet> OwnedPets;
        public Resources Resources;
        public int ClearedChapterMax;
        public Dictionary<int, int> BestSurvivalDays;
        public HashSet<string> ClaimedMilestones;

        public Player()
        {
            Talent = new Talent();
            Heritage = new Heritage();
            EquipmentSlots = new Dictionary<SlotType, EquipmentSlot>
            {
                { SlotType.WEAPON, new EquipmentSlot(SlotType.WEAPON) },
                { SlotType.ARMOR, new EquipmentSlot(SlotType.ARMOR) },
                { SlotType.RING, new EquipmentSlot(SlotType.RING) },
                { SlotType.NECKLACE, new EquipmentSlot(SlotType.NECKLACE) },
                { SlotType.SHOES, new EquipmentSlot(SlotType.SHOES) },
                { SlotType.GLOVES, new EquipmentSlot(SlotType.GLOVES) },
                { SlotType.HAT, new EquipmentSlot(SlotType.HAT) },
            };
            Inventory = new List<Equipment>();
            ActivePet = null;
            OwnedPets = new List<Pet>();
            Resources = new Resources();
            ClearedChapterMax = 0;
            BestSurvivalDays = new Dictionary<int, int>();
            ClaimedMilestones = new HashSet<string>();
        }

        public Stats ComputeStats()
        {
            Stats stats = BaseStats;

            stats = stats.Add(Talent.GetStats());
            stats = stats.Add(TalentTable.GetStatBonus(Talent.GetTotalLevel()));

            foreach (var slot in EquipmentSlots.Values)
            {
                stats = stats.Add(slot.GetTotalStats());
            }

            if (IsHeritageUnlocked())
            {
                stats = stats.Add(Heritage.GetPassiveBonus());
            }

            if (ActivePet != null)
            {
                stats = stats.Add(ActivePet.GetGlobalBonus());
            }

            foreach (var pet in OwnedPets)
            {
                if (pet != ActivePet)
                {
                    var bonus = pet.GetGlobalBonus();
                    float rate = PetTable.InactiveBonusRate;
                    var passiveOnly = Stats.Create(
                        atk: Mathf.FloorToInt(bonus.Atk * rate),
                        maxHp: Mathf.FloorToInt(bonus.MaxHp * rate)
                    );
                    stats = stats.Add(passiveOnly);
                }
            }

            stats = stats.WithHp(stats.MaxHp);

            return stats;
        }

        public StatsBreakdown GetStatsBreakdown()
        {
            Stats baseStat = BaseStats;
            Stats talent = Talent.GetStats();
            Stats grade = TalentTable.GetStatBonus(Talent.GetTotalLevel());

            Stats equipment = Stats.Zero;
            foreach (var slot in EquipmentSlots.Values)
            {
                equipment = equipment.Add(slot.GetTotalStats());
            }

            Stats heritage = IsHeritageUnlocked() ? Heritage.GetPassiveBonus() : Stats.Zero;

            Stats pet = Stats.Zero;
            if (ActivePet != null)
            {
                pet = pet.Add(ActivePet.GetGlobalBonus());
            }
            foreach (var p in OwnedPets)
            {
                if (p != ActivePet)
                {
                    var bonus = p.GetGlobalBonus();
                    pet = pet.Add(Stats.Create(
                        atk: Mathf.FloorToInt(bonus.Atk * 0.1f),
                        maxHp: Mathf.FloorToInt(bonus.MaxHp * 0.1f)
                    ));
                }
            }

            Stats total = baseStat.Add(talent).Add(grade).Add(equipment).Add(heritage).Add(pet);
            total = total.WithHp(total.MaxHp);

            return new StatsBreakdown
            {
                Base = baseStat,
                Talent = talent,
                Grade = grade,
                Equipment = equipment,
                Heritage = heritage,
                Pet = pet,
                Total = total,
            };
        }

        public float GetGoldMultiplier()
        {
            float boost = 0;
            foreach (var m in TalentTable.GetAllMilestones())
            {
                if (m.RewardType != "GOLD_BOOST") continue;
                if (ClaimedMilestones.Contains(Talent.GetMilestoneKey(m.Level))) boost += m.RewardAmount;
            }
            return 1f + boost / 100f;
        }

        public CombatPassives GetCombatPassives()
        {
            return new CombatPassives
            {
                CritDamage = BattleDataTable.Data.Damage.CritMultiplier,
                LifestealRate = 0f,
                EvasionRate = 0f,
                CounterChance = 0f,
            };
        }

        public bool IsHeritageUnlocked()
        {
            return Entities.Heritage.IsUnlocked(Talent.Grade);
        }

        public EquipmentSlot GetEquipmentSlot(SlotType slotType)
        {
            return EquipmentSlots[slotType];
        }

        public void SetActivePet(Pet pet)
        {
            ActivePet = pet;
        }

        public void AddPet(Pet pet)
        {
            OwnedPets.Add(pet);
        }

        public void AddToInventory(Equipment equipment)
        {
            Inventory.Add(equipment);
        }

        public Equipment RemoveFromInventory(string id)
        {
            int index = Inventory.FindIndex(e => e.Id == id);
            if (index == -1) return null;
            var eq = Inventory[index];
            Inventory.RemoveAt(index);
            return eq;
        }

        public int SellEquipment(string id)
        {
            var eq = Inventory.Find(e => e.Id == id);
            if (eq == null || eq.IsS) return 0;
            int price = EquipmentDataTable.GetSellPrice(eq.Grade);
            RemoveFromInventory(id);
            Resources.Add(ResourceType.GOLD, price);
            return price;
        }

        public Equipment EquipFromInventory(string id)
        {
            var eq = RemoveFromInventory(id);
            if (eq == null) return null;

            var slot = GetEquipmentSlot(eq.Slot);

            int emptyIndex = slot.GetFirstEmptyIndex();
            if (emptyIndex >= 0)
            {
                slot.Equip(eq, emptyIndex);
                return null;
            }

            int worstIndex = 0;
            for (int i = 1; i < slot.MaxCount; i++)
            {
                var current = slot.Equipped[i];
                var worst = slot.Equipped[worstIndex];
                if (current != null && worst != null && worst.IsBetterThan(current))
                {
                    worstIndex = i;
                }
            }

            var result = slot.Equip(eq, worstIndex);
            if (result.IsOk() && result.Data.Replaced != null)
            {
                AddToInventory(result.Data.Replaced);
                return result.Data.Replaced;
            }
            return null;
        }

        public bool UnequipToInventory(SlotType slotType, int index)
        {
            var slot = GetEquipmentSlot(slotType);
            var result = slot.Unequip(index);
            if (result.IsFail()) return false;
            AddToInventory(result.Data.Equipment);
            return true;
        }

        public TalentGrade GetTalentGrade()
        {
            return Talent.Grade;
        }

        public void UpdateBestSurvivalDay(int chapterId, int day, bool cleared)
        {
            int effectiveDay = cleared
                ? ChapterTreasureTable.GetClearSentinelDay(chapterId)
                : day;
            int current = BestSurvivalDays.TryGetValue(chapterId, out var val) ? val : 0;
            if (effectiveDay > current)
            {
                BestSurvivalDays[chapterId] = effectiveDay;
            }
        }
    }
}
