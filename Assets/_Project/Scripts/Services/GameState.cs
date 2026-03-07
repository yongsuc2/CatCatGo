using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Chapter;
using CatCatGo.Domain.Content;
using CatCatGo.Domain.Economy;
using CatCatGo.Domain.Meta;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;
using CatCatGo.Network;

namespace CatCatGo.Services
{
    public class GameState
    {
        public Player Player;
        public Chapter CurrentChapter;

        public Tower Tower;
        public CatacombDungeon Catacomb;
        public DailyDungeonManager DungeonManager;
        public GoblinMiner GoblinMinerSystem;

        public TreasureChest EquipmentChestSystem;
        public Collection CollectionSystem;
        public DailyResetSystem DailyResetSystem;
        public EventManager EventManagerSystem;
        public DailyRoutineScheduler RoutineScheduler;

        public ChapterTreasure ChapterTreasureSystem;
        public AttendanceSystem AttendanceSystem;

        public GameState()
        {
            Player = new Player();
            CurrentChapter = null;

            Tower = new Tower();
            Catacomb = new CatacombDungeon();
            DungeonManager = new DailyDungeonManager();
            GoblinMinerSystem = new GoblinMiner();

            EquipmentChestSystem = new TreasureChest(ChestType.EQUIPMENT);
            CollectionSystem = new Collection();
            DailyResetSystem = new DailyResetSystem();
            EventManagerSystem = new EventManager();
            RoutineScheduler = new DailyRoutineScheduler();
            ChapterTreasureSystem = new ChapterTreasure();
            AttendanceSystem = new AttendanceSystem();
        }

        public void Reset()
        {
            Player = new Player();
            CurrentChapter = null;
            Tower = new Tower();
            Catacomb = new CatacombDungeon();
            DungeonManager = new DailyDungeonManager();
            GoblinMinerSystem = new GoblinMiner();
            EquipmentChestSystem = new TreasureChest(ChestType.EQUIPMENT);
            CollectionSystem = new Collection();
            DailyResetSystem = new DailyResetSystem();
            EventManagerSystem = new EventManager();
            RoutineScheduler = new DailyRoutineScheduler();
            ChapterTreasureSystem = new ChapterTreasure();
            AttendanceSystem = new AttendanceSystem();
        }

        public void ApplyDelta(StateDelta delta)
        {
            if (delta == null) return;

            bool resourcesChanged = false;
            bool talentChanged = false;
            bool heritageChanged = false;
            bool equipmentChanged = false;
            bool inventoryChanged = false;
            bool petChanged = false;
            bool statsChanged = false;
            bool towerChanged = false;
            bool catacombChanged = false;
            bool dungeonChanged = false;
            bool questChanged = false;
            bool attendanceChanged = false;
            bool chapterChanged = false;

            if (delta.Resources != null)
            {
                foreach (var kv in delta.Resources)
                {
                    if (Enum.TryParse<ResourceType>(kv.Key, out var resType))
                        Player.Resources.SetAmount(resType, (int)kv.Value);
                }
                resourcesChanged = true;
            }

            if (delta.Talent != null)
            {
                Player.Talent = new Talent(
                    delta.Talent.AtkLevel ?? Player.Talent.AtkLevel,
                    delta.Talent.HpLevel ?? Player.Talent.HpLevel,
                    delta.Talent.DefLevel ?? Player.Talent.DefLevel);
                talentChanged = true;
                statsChanged = true;
            }

            if (delta.Heritage != null)
            {
                if (delta.Heritage.Route != null && Enum.TryParse<HeritageRoute>(delta.Heritage.Route, out var route))
                    Player.Heritage.Route = route;
                if (delta.Heritage.Level.HasValue)
                    Player.Heritage.Level = delta.Heritage.Level.Value;
                heritageChanged = true;
                statsChanged = true;
            }

            if (delta.AddedEquipments != null && delta.AddedEquipments.Count > 0)
            {
                foreach (var eqData in delta.AddedEquipments)
                {
                    var equipment = DeserializeEquipmentDelta(eqData);
                    if (equipment != null)
                        Player.AddToInventory(equipment);
                }
                inventoryChanged = true;
            }

            if (delta.RemovedEquipmentIds != null && delta.RemovedEquipmentIds.Count > 0)
            {
                foreach (var id in delta.RemovedEquipmentIds)
                    Player.RemoveFromInventory(id);
                inventoryChanged = true;
            }

            if (delta.UpgradedEquipments != null && delta.UpgradedEquipments.Count > 0)
            {
                foreach (var upgrade in delta.UpgradedEquipments)
                    ApplyEquipmentUpgrade(upgrade);
                equipmentChanged = true;
                statsChanged = true;
            }

            if (delta.EquipmentSlotChanges != null && delta.EquipmentSlotChanges.Count > 0)
            {
                foreach (var slotChange in delta.EquipmentSlotChanges)
                    ApplyEquipSlotChange(slotChange);
                equipmentChanged = true;
                statsChanged = true;
            }

            if (delta.AddedPets != null && delta.AddedPets.Count > 0)
            {
                foreach (var petData in delta.AddedPets)
                {
                    var pet = DeserializePetDelta(petData);
                    if (pet != null)
                        Player.AddPet(pet);
                }
                petChanged = true;
                statsChanged = true;
            }

            if (delta.UpdatedPets != null && delta.UpdatedPets.Count > 0)
            {
                foreach (var update in delta.UpdatedPets)
                    ApplyPetUpdate(update);
                petChanged = true;
                statsChanged = true;
            }

            if (delta.ActivePetId != null)
            {
                var pet = Player.OwnedPets.FirstOrDefault(p => p.Id == delta.ActivePetId);
                Player.SetActivePet(pet);
                petChanged = true;
                statsChanged = true;
            }

            if (delta.ClearedChapterMax.HasValue)
            {
                Player.ClearedChapterMax = delta.ClearedChapterMax.Value;
            }

            if (delta.BestSurvivalDays != null)
            {
                foreach (var kv in delta.BestSurvivalDays)
                {
                    if (int.TryParse(kv.Key, out int chapterId))
                        Player.BestSurvivalDays[chapterId] = kv.Value;
                }
            }

            if (delta.AddedClaimedMilestones != null)
            {
                foreach (var key in delta.AddedClaimedMilestones)
                    Player.ClaimedMilestones.Add(key);
            }

            if (delta.Tower != null)
            {
                if (delta.Tower.CurrentFloor.HasValue)
                    Tower.CurrentFloor = delta.Tower.CurrentFloor.Value;
                if (delta.Tower.CurrentStage.HasValue)
                    Tower.CurrentStage = delta.Tower.CurrentStage.Value;
                towerChanged = true;
            }

            if (delta.Catacomb != null)
            {
                if (delta.Catacomb.HighestFloor.HasValue)
                    Catacomb.HighestFloor = delta.Catacomb.HighestFloor.Value;
                if (delta.Catacomb.CurrentRunFloor.HasValue)
                    Catacomb.CurrentRunFloor = delta.Catacomb.CurrentRunFloor.Value;
                if (delta.Catacomb.IsRunning.HasValue)
                    Catacomb.IsRunning = delta.Catacomb.IsRunning.Value;
                catacombChanged = true;
            }

            if (delta.Dungeons != null)
            {
                if (delta.Dungeons.TodayCount.HasValue)
                    DungeonManager.TodayCount = delta.Dungeons.TodayCount.Value;
                if (delta.Dungeons.ClearedStages != null)
                {
                    foreach (var kv in delta.Dungeons.ClearedStages)
                    {
                        if (Enum.TryParse<DungeonType>(kv.Key, out var dungeonType))
                        {
                            if (DungeonManager.Dungeons.TryGetValue(dungeonType, out var dungeon))
                                dungeon.ClearedStage = kv.Value;
                        }
                    }
                }
                dungeonChanged = true;
            }

            if (delta.GoblinOreCount.HasValue)
                GoblinMinerSystem.OreCount = delta.GoblinOreCount.Value;

            if (delta.PityCount.HasValue)
                EquipmentChestSystem.PityCount = delta.PityCount.Value;

            if (delta.AddedCollectionIds != null)
            {
                foreach (var id in delta.AddedCollectionIds)
                    CollectionSystem.Acquire(id);
            }

            if (delta.MissionUpdates != null && delta.MissionUpdates.Count > 0)
            {
                foreach (var missionUpdate in delta.MissionUpdates)
                    ApplyMissionUpdate(missionUpdate);
                questChanged = true;
            }

            if (delta.Attendance != null)
            {
                if (delta.Attendance.CheckedDays != null)
                    AttendanceSystem.CheckedDays = delta.Attendance.CheckedDays;
                if (delta.Attendance.LastCheckDate != null)
                    AttendanceSystem.LastCheckDate = delta.Attendance.LastCheckDate;
                attendanceChanged = true;
            }

            if (delta.ChapterSession != null)
            {
                chapterChanged = true;
            }

            if (resourcesChanged) EventBus.Publish(new ResourcesChangedEvent());
            if (talentChanged) EventBus.Publish(new TalentChangedEvent());
            if (heritageChanged) EventBus.Publish(new HeritageChangedEvent());
            if (equipmentChanged) EventBus.Publish(new EquipmentChangedEvent());
            if (inventoryChanged) EventBus.Publish(new InventoryChangedEvent());
            if (petChanged) EventBus.Publish(new PetChangedEvent());
            if (towerChanged) EventBus.Publish(new TowerChangedEvent());
            if (catacombChanged) EventBus.Publish(new CatacombChangedEvent());
            if (dungeonChanged) EventBus.Publish(new DungeonChangedEvent());
            if (questChanged) EventBus.Publish(new QuestChangedEvent());
            if (attendanceChanged) EventBus.Publish(new AttendanceChangedEvent());
            if (chapterChanged) EventBus.Publish(new ChapterStateChangedEvent());
            if (statsChanged) EventBus.Publish(new PlayerStatsChangedEvent());
        }

        public void ApplyFullSync(SaveState state)
        {
            if (state == null) return;
            SaveSerializer.Deserialize(state, this);
        }

        private void ApplyEquipmentUpgrade(EquipmentUpgradeDelta upgrade)
        {
            foreach (var slot in Player.EquipmentSlots.Values)
            {
                for (int i = 0; i < slot.Equipped.Length; i++)
                {
                    if (slot.Equipped[i] != null && slot.Equipped[i].Id == upgrade.EquipmentId)
                    {
                        slot.Equipped[i].Level = upgrade.NewLevel;
                        slot.Equipped[i].PromoteCount = upgrade.NewPromoteCount;
                        return;
                    }
                }
            }

            var invEquip = Player.Inventory.FirstOrDefault(e => e.Id == upgrade.EquipmentId);
            if (invEquip != null)
            {
                invEquip.Level = upgrade.NewLevel;
                invEquip.PromoteCount = upgrade.NewPromoteCount;
            }
        }

        private void ApplyEquipSlotChange(EquipSlotDelta slotChange)
        {
            if (!Enum.TryParse<SlotType>(slotChange.SlotType, out var slotType)) return;
            if (!Player.EquipmentSlots.TryGetValue(slotType, out var slot)) return;
            if (slotChange.Index < 0 || slotChange.Index >= slot.MaxCount) return;

            if (slotChange.EquipmentId == null)
            {
                var unequipped = slot.Equipped[slotChange.Index];
                slot.Equipped[slotChange.Index] = null;
                slot.SlotLevels[slotChange.Index] = 0;
                if (unequipped != null)
                    Player.AddToInventory(unequipped);
            }
            else
            {
                var equipment = Player.RemoveFromInventory(slotChange.EquipmentId);
                if (equipment != null)
                {
                    var replaced = slot.Equipped[slotChange.Index];
                    slot.Equipped[slotChange.Index] = equipment;
                    slot.SlotLevels[slotChange.Index] = slotChange.SlotLevel;
                    if (replaced != null)
                        Player.AddToInventory(replaced);
                }
            }
        }

        private void ApplyPetUpdate(PetUpdateDelta update)
        {
            var pet = Player.OwnedPets.FirstOrDefault(p => p.Id == update.PetId);
            if (pet == null) return;

            if (update.Level.HasValue) pet.Level = update.Level.Value;
            if (update.Exp.HasValue) pet.Exp = update.Exp.Value;
            if (update.Grade != null && Enum.TryParse<PetGrade>(update.Grade, out var grade))
                pet.Grade = grade;
        }

        private void ApplyMissionUpdate(MissionDelta missionUpdate)
        {
            foreach (var evt in EventManagerSystem.Events)
            {
                if (missionUpdate.EventId != null && evt.Id != missionUpdate.EventId)
                    continue;

                var mission = evt.Missions.FirstOrDefault(m => m.Id == missionUpdate.MissionId);
                if (mission != null)
                {
                    if (missionUpdate.Current.HasValue) mission.Current = missionUpdate.Current.Value;
                    if (missionUpdate.Claimed.HasValue) mission.Claimed = missionUpdate.Claimed.Value;
                    return;
                }
            }
        }

        private Equipment DeserializeEquipmentDelta(EquipmentDeltaData data)
        {
            if (!Enum.TryParse<SlotType>(data.Slot, out var slot)) return null;
            if (!Enum.TryParse<EquipmentGrade>(data.Grade, out var grade)) return null;

            var subStats = data.SubStats?.Select(s => new SubStat(s.Stat, s.Value)).ToList()
                ?? new List<SubStat>();

            return new Equipment(
                data.Id, data.Name, slot, grade, data.IsS,
                data.Level, data.PromoteCount, null,
                null, data.MergeLevel, subStats);
        }

        private Pet DeserializePetDelta(PetDeltaData data)
        {
            if (!Enum.TryParse<PetTier>(data.Tier, out var tier)) return null;
            if (!Enum.TryParse<PetGrade>(data.Grade, out var grade)) return null;

            var template = PetTable.GetAllTemplates()
                .FirstOrDefault(t => t.Name == data.Name);
            var maxGrade = template?.MaxGrade ?? PetGrade.IMMORTAL;

            return new Pet(
                data.Id, data.Name, tier, grade, maxGrade, data.Level,
                template?.BasePassiveBonus ?? Stats.Zero,
                data.Exp);
        }
    }
}
