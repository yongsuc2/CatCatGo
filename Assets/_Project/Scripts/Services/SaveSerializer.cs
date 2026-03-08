using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Data;
using CatCatGo.Domain.Meta;

namespace CatCatGo.Services
{
    [Serializable]
    public class EquipmentData
    {
        public string Id;
        public string Name;
        public SlotType Slot;
        public EquipmentGrade Grade;
        public bool IsS;
        public int Level;
        public int UpgradeCount;
        public int PromoteCount;
        public UniqueEffectData UniqueEffect;
        public WeaponSubType? WeaponSubType;
        public int MergeLevel;
        public List<SubStatData> SubStats;
    }

    [Serializable]
    public class UniqueEffectData
    {
        public string Description;
        public StatBonusData StatBonus;
    }

    [Serializable]
    public class StatBonusData
    {
        public int Hp;
        public int MaxHp;
        public int Atk;
        public int Def;
        public float Crit;
    }

    [Serializable]
    public class SubStatData
    {
        public string Stat;
        public float Value;
    }

    [Serializable]
    public class PetData
    {
        public string Id;
        public string Name;
        public PetTier Tier;
        public PetGrade Grade;
        public int Level;
        public StatBonusData BasePassiveBonus;
        public int Exp;
    }

    [Serializable]
    public class RewardData
    {
        public List<ResourceRewardData> Resources;
        public List<string> EquipmentIds;
        public List<string> SkillIds;
        public List<string> PetIds;
    }

    [Serializable]
    public class ResourceRewardData
    {
        public ResourceType Type;
        public int Amount;
    }

    [Serializable]
    public class MissionData
    {
        public string Id;
        public string Description;
        public int Target;
        public int Current;
        public RewardData Reward;
        public bool Claimed;
    }

    [Serializable]
    public class EventData
    {
        public string Id;
        public string Name;
        public EventType Type;
        public long StartTime;
        public long EndTime;
        public List<MissionData> Missions;
    }

    [Serializable]
    public class SaveState
    {
        public PlayerSaveData Player;
        public TowerSaveData Tower;
        public CatacombSaveData Catacomb;
        public DungeonsSaveData Dungeons;
        public GoblinMinerSaveData GoblinMiner;
        public EquipmentChestSaveData EquipmentChest;
        public List<string> Collection;
        public DailyResetSaveData DailyReset;
        public List<EventData> Events;
        public AttendanceSaveData Attendance;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public TalentSaveData Talent;
        public HeritageSaveData Heritage;
        public Dictionary<string, float> Resources;
        public Dictionary<string, List<EquipmentData>> EquipmentSlots;
        public Dictionary<string, List<int>> SlotLevels;
        public Dictionary<string, List<int>> SlotPromoteCounts;
        public List<EquipmentData> Inventory;
        public string ActivePetId;
        public List<PetData> OwnedPets;
        public int ClearedChapterMax;
        public Dictionary<string, int> BestSurvivalDays;
        public List<string> ClaimedMilestones;
    }

    [Serializable]
    public class TalentSaveData
    {
        public int AtkLevel;
        public int HpLevel;
        public int DefLevel;
    }

    [Serializable]
    public class HeritageSaveData
    {
        public HeritageRoute Route;
        public int Level;
    }

    [Serializable]
    public class TowerSaveData
    {
        public int CurrentFloor;
        public int CurrentStage;
    }

    [Serializable]
    public class CatacombSaveData
    {
        public int HighestFloor;
    }

    [Serializable]
    public class DungeonsSaveData
    {
        public int TodayCount;
        public Dictionary<string, int> ClearedStages;
    }


    [Serializable]
    public class GoblinMinerSaveData
    {
        public int OreCount;
    }

    [Serializable]
    public class EquipmentChestSaveData
    {
        public int PityCount;
    }

    [Serializable]
    public class DailyResetSaveData
    {
        public string LastResetDate;
    }

    [Serializable]
    public class AttendanceSaveData
    {
        public List<bool> CheckedDays;
        public string CycleStartDate;
        public string LastCheckDate;
    }

    public static class SaveSerializer
    {
        public static SaveState Serialize(GameManager game)
        {
            return Serialize(game.State);
        }

        public static SaveState Serialize(GameState state)
        {
            var player = state.Player;

            var equipmentSlots = new Dictionary<string, List<EquipmentData>>();
            var slotLevels = new Dictionary<string, List<int>>();
            var slotPromoteCounts = new Dictionary<string, List<int>>();
            foreach (var kv in player.EquipmentSlots)
            {
                string key = kv.Key.ToString();
                var slot = kv.Value;
                equipmentSlots[key] = slot.Equipped.Select(eq => eq != null ? SerializeEquipment(eq) : null).ToList();
                slotLevels[key] = slot.SlotLevels.ToList();
                slotPromoteCounts[key] = slot.SlotPromoteCounts.ToList();
            }

            var clearedStages = new Dictionary<string, int>();
            foreach (var kv in state.DungeonManager.Dungeons)
            {
                clearedStages[kv.Key.ToString()] = kv.Value.ClearedStage;
            }

            var collectionIds = new List<string>();
            foreach (var entry in state.CollectionSystem.Entries.Values)
            {
                if (entry.Acquired) collectionIds.Add(entry.Id);
            }

            var events = state.EventManagerSystem.Events.Select(evt => new EventData
            {
                Id = evt.Id,
                Name = evt.Name,
                Type = evt.Type,
                StartTime = evt.StartTime,
                EndTime = evt.EndTime,
                Missions = evt.Missions.Select(m => new MissionData
                {
                    Id = m.Id,
                    Description = m.Description,
                    Target = m.Target,
                    Current = m.Current,
                    Reward = SerializeReward(m.Reward),
                    Claimed = m.Claimed,
                }).ToList(),
            }).ToList();

            return new SaveState
            {
                Player = new PlayerSaveData
                {
                    Talent = new TalentSaveData
                    {
                        AtkLevel = player.Talent.AtkLevel,
                        HpLevel = player.Talent.HpLevel,
                        DefLevel = player.Talent.DefLevel,
                    },
                    Heritage = new HeritageSaveData
                    {
                        Route = player.Heritage.Route,
                        Level = player.Heritage.Level,
                    },
                    Resources = player.Resources.ToJSON(),
                    EquipmentSlots = equipmentSlots,
                    SlotLevels = slotLevels,
                    SlotPromoteCounts = slotPromoteCounts,
                    Inventory = player.Inventory.Select(SerializeEquipment).ToList(),
                    ActivePetId = player.ActivePet?.Id,
                    OwnedPets = player.OwnedPets.Select(SerializePet).ToList(),
                    ClearedChapterMax = player.ClearedChapterMax,
                    BestSurvivalDays = player.BestSurvivalDays.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
                    ClaimedMilestones = player.ClaimedMilestones.ToList(),
                },
                Tower = new TowerSaveData
                {
                    CurrentFloor = state.Tower.CurrentFloor,
                    CurrentStage = state.Tower.CurrentStage,
                },
                Catacomb = new CatacombSaveData { HighestFloor = state.Catacomb.HighestFloor },
                Dungeons = new DungeonsSaveData
                {
                    TodayCount = state.DungeonManager.TodayCount,
                    ClearedStages = clearedStages,
                },
                GoblinMiner = new GoblinMinerSaveData { OreCount = state.GoblinMinerSystem.OreCount },
                EquipmentChest = new EquipmentChestSaveData { PityCount = state.EquipmentChestSystem.PityCount },
                Collection = collectionIds,
                DailyReset = new DailyResetSaveData { LastResetDate = state.DailyResetSystem.GetLastResetDate() },
                Events = events,
                Attendance = new AttendanceSaveData
                {
                    CheckedDays = state.AttendanceSystem.CheckedDays.ToList(),
                    CycleStartDate = state.AttendanceSystem.CycleStartDate,
                    LastCheckDate = state.AttendanceSystem.LastCheckDate,
                },
            };
        }

        public static void Deserialize(SaveState data, GameManager game)
        {
            Deserialize(data, game.State);
        }

        public static void Deserialize(SaveState data, GameState state)
        {
            var player = state.Player;

            player.Talent = new Talent(
                data.Player.Talent.AtkLevel,
                data.Player.Talent.HpLevel,
                data.Player.Talent.DefLevel);

            player.Heritage.Route = data.Player.Heritage.Route;
            player.Heritage.Level = data.Player.Heritage.Level;

            player.Resources = CatCatGo.Domain.Entities.Resources.FromJSON(data.Player.Resources);

            var migratedSlots = MigrateAccessoryToNecklace(data.Player.EquipmentSlots);
            foreach (var kv in migratedSlots)
            {
                if (!Enum.TryParse<SlotType>(kv.Key, out var slotType)) continue;
                if (!player.EquipmentSlots.TryGetValue(slotType, out var slot)) continue;

                var equippedArr = kv.Value;
                for (int i = 0; i < equippedArr.Count && i < slot.MaxCount; i++)
                {
                    var eqData = equippedArr[i];
                    slot.Equipped[i] = eqData != null ? DeserializeEquipment(eqData) : null;
                }

                if (data.Player.SlotLevels != null && data.Player.SlotLevels.TryGetValue(kv.Key, out var levels))
                {
                    for (int i = 0; i < levels.Count && i < slot.MaxCount; i++)
                        slot.SlotLevels[i] = levels[i];
                }

                if (data.Player.SlotPromoteCounts != null && data.Player.SlotPromoteCounts.TryGetValue(kv.Key, out var counts))
                {
                    for (int i = 0; i < counts.Count && i < slot.MaxCount; i++)
                        slot.SlotPromoteCounts[i] = counts[i];
                }

                if (data.Player.SlotLevels == null)
                    slot.InitFromEquipped();
            }

            player.Inventory = data.Player.Inventory.Select(eqData =>
            {
                if (eqData.Slot.ToString() == "ACCESSORY")
                    eqData.Slot = SlotType.NECKLACE;
                return DeserializeEquipment(eqData);
            }).ToList();

            player.OwnedPets = data.Player.OwnedPets.Select(DeserializePet).ToList();
            if (data.Player.ActivePetId != null)
                player.ActivePet = player.OwnedPets.FirstOrDefault(p => p.Id == data.Player.ActivePetId);
            else
                player.ActivePet = null;

            player.ClearedChapterMax = data.Player.ClearedChapterMax;

            player.BestSurvivalDays = new Dictionary<int, int>();
            if (data.Player.BestSurvivalDays != null)
            {
                foreach (var kv in data.Player.BestSurvivalDays)
                {
                    if (int.TryParse(kv.Key, out int key))
                        player.BestSurvivalDays[key] = kv.Value;
                }
            }

            player.ClaimedMilestones = new HashSet<string>(data.Player.ClaimedMilestones ?? new List<string>());

            state.Tower.CurrentFloor = data.Tower.CurrentFloor;
            state.Tower.CurrentStage = data.Tower.CurrentStage;

            state.Catacomb.HighestFloor = data.Catacomb.HighestFloor;

            if (data.Dungeons != null)
            {
                state.DungeonManager.TodayCount = data.Dungeons.TodayCount;
                if (data.Dungeons.ClearedStages != null)
                {
                    foreach (var kv in data.Dungeons.ClearedStages)
                    {
                        if (Enum.TryParse<DungeonType>(kv.Key, out var dungeonType))
                        {
                            if (state.DungeonManager.Dungeons.TryGetValue(dungeonType, out var dungeon))
                                dungeon.ClearedStage = kv.Value;
                        }
                    }
                }
            }

            state.GoblinMinerSystem.OreCount = data.GoblinMiner.OreCount;

            if (data.EquipmentChest != null)
                state.EquipmentChestSystem.PityCount = data.EquipmentChest.PityCount;

            foreach (var id in data.Collection)
                state.CollectionSystem.Acquire(id);

            state.DailyResetSystem.SetLastResetDate(data.DailyReset.LastResetDate);

            state.EventManagerSystem.Events = data.Events.Select(eventData =>
            {
                var missions = eventData.Missions.Select(m => new EventMission
                {
                    Id = m.Id,
                    Description = m.Description,
                    Target = m.Target,
                    Current = m.Current,
                    Reward = DeserializeReward(m.Reward),
                    Claimed = m.Claimed,
                }).ToList();
                return new GameEvent(
                    eventData.Id, eventData.Name, eventData.Type,
                    eventData.StartTime, eventData.EndTime, missions);
            }).ToList();

            if (data.Attendance != null)
            {
                state.AttendanceSystem.CheckedDays = data.Attendance.CheckedDays.ToArray();
                state.AttendanceSystem.CycleStartDate = data.Attendance.CycleStartDate;
                state.AttendanceSystem.LastCheckDate = data.Attendance.LastCheckDate;
            }
        }

        private static EquipmentData SerializeEquipment(Equipment eq)
        {
            return new EquipmentData
            {
                Id = eq.Id,
                Name = eq.Name,
                Slot = eq.Slot,
                Grade = eq.Grade,
                IsS = eq.IsS,
                Level = eq.Level,
                PromoteCount = eq.PromoteCount,
                UniqueEffect = eq.UniqueEffectValue.HasValue ? new UniqueEffectData
                {
                    Description = eq.UniqueEffectValue.Value.Description,
                    StatBonus = new StatBonusData
                    {
                        Hp = eq.UniqueEffectValue.Value.StatBonus.Hp,
                        MaxHp = eq.UniqueEffectValue.Value.StatBonus.MaxHp,
                        Atk = eq.UniqueEffectValue.Value.StatBonus.Atk,
                        Def = eq.UniqueEffectValue.Value.StatBonus.Def,
                        Crit = eq.UniqueEffectValue.Value.StatBonus.Crit,
                    },
                } : null,
                WeaponSubType = eq.WeaponSubTypeValue,
                MergeLevel = eq.MergeLevel,
                SubStats = eq.SubStats.Select(s => new SubStatData { Stat = s.Stat, Value = s.Value }).ToList(),
            };
        }

        private static WeaponSubType? MigrateWeaponSubType(EquipmentData data)
        {
            if (data.Slot != SlotType.WEAPON) return null;
            if (data.WeaponSubType.HasValue) return data.WeaponSubType;

            var weaponSubTypes = new[] { Domain.Enums.WeaponSubType.SWORD, Domain.Enums.WeaponSubType.STAFF, Domain.Enums.WeaponSubType.BOW };
            int hash = 0;
            foreach (char c in data.Id)
            {
                hash = ((hash << 5) - hash + c);
            }
            return weaponSubTypes[Math.Abs(hash) % 3];
        }

        private static Equipment DeserializeEquipment(EquipmentData data)
        {
            UniqueEffect? uniqueEffect = null;
            if (data.UniqueEffect != null)
            {
                uniqueEffect = new UniqueEffect(
                    data.UniqueEffect.Description,
                    Stats.Create(
                        hp: data.UniqueEffect.StatBonus.Hp,
                        maxHp: data.UniqueEffect.StatBonus.MaxHp,
                        atk: data.UniqueEffect.StatBonus.Atk,
                        def: data.UniqueEffect.StatBonus.Def,
                        crit: data.UniqueEffect.StatBonus.Crit));
            }

            var weaponSubType = MigrateWeaponSubType(data);
            var subStats = data.SubStats?.Select(s => new SubStat(s.Stat, s.Value)).ToList() ?? new List<SubStat>();

            return new Equipment(
                data.Id, data.Name, data.Slot, data.Grade, data.IsS,
                data.Level, data.PromoteCount, uniqueEffect,
                weaponSubType, data.MergeLevel,
                subStats);
        }

        private static PetData SerializePet(Pet pet)
        {
            return new PetData
            {
                Id = pet.Id,
                Name = pet.Name,
                Tier = pet.Tier,
                Grade = pet.Grade,
                Level = pet.Level,
                BasePassiveBonus = new StatBonusData
                {
                    Hp = pet.BasePassiveBonus.Hp,
                    MaxHp = pet.BasePassiveBonus.MaxHp,
                    Atk = pet.BasePassiveBonus.Atk,
                    Def = pet.BasePassiveBonus.Def,
                    Crit = pet.BasePassiveBonus.Crit,
                },
                Exp = pet.Exp,
            };
        }

        private static Pet DeserializePet(PetData data)
        {
            var template = PetTable.GetAllTemplates()
                .FirstOrDefault(t => t.Name == data.Name);
            var maxGrade = template?.MaxGrade ?? PetGrade.IMMORTAL;

            return new Pet(
                data.Id, data.Name, data.Tier, data.Grade, maxGrade, data.Level,
                Stats.Create(
                    hp: data.BasePassiveBonus.Hp,
                    maxHp: data.BasePassiveBonus.MaxHp,
                    atk: data.BasePassiveBonus.Atk,
                    def: data.BasePassiveBonus.Def,
                    crit: data.BasePassiveBonus.Crit),
                data.Exp);
        }

        private static RewardData SerializeReward(Reward reward)
        {
            return new RewardData
            {
                Resources = reward.Resources.Select(r => new ResourceRewardData { Type = r.Type, Amount = r.Amount }).ToList(),
                EquipmentIds = reward.EquipmentIds.ToList(),
                SkillIds = reward.SkillIds.ToList(),
                PetIds = reward.PetIds.ToList(),
            };
        }

        private static Reward DeserializeReward(RewardData data)
        {
            return new Reward(
                data.Resources.Select(r => new ResourceReward(r.Type, r.Amount)).ToList(),
                data.EquipmentIds,
                data.SkillIds,
                data.PetIds);
        }

        private static Dictionary<string, List<EquipmentData>> MigrateAccessoryToNecklace(
            Dictionary<string, List<EquipmentData>> slots)
        {
            if (!slots.ContainsKey("ACCESSORY")) return slots;

            var result = new Dictionary<string, List<EquipmentData>>(slots);
            var accessoryItems = result["ACCESSORY"];
            result.Remove("ACCESSORY");

            if (!result.ContainsKey("NECKLACE"))
            {
                result["NECKLACE"] = accessoryItems.Select(eq =>
                {
                    if (eq != null) eq.Slot = SlotType.NECKLACE;
                    return eq;
                }).ToList();
            }

            return result;
        }
    }
}
