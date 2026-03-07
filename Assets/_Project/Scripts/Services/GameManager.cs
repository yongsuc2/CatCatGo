using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Chapter;
using CatCatGo.Domain.Content;
using CatCatGo.Domain.Economy;
using CatCatGo.Domain.Meta;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;
using GameResources = CatCatGo.Domain.Entities.Resources;

namespace CatCatGo.Services
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; }

        public Player Player
        {
            get => State.Player;
            set => State.Player = value;
        }

        public Chapter CurrentChapter
        {
            get => State.CurrentChapter;
            set => State.CurrentChapter = value;
        }

        public Tower Tower
        {
            get => State.Tower;
            set => State.Tower = value;
        }

        public CatacombDungeon Catacomb
        {
            get => State.Catacomb;
            set => State.Catacomb = value;
        }

        public DailyDungeonManager DungeonManager
        {
            get => State.DungeonManager;
            set => State.DungeonManager = value;
        }

        public GoblinMiner GoblinMinerSystem
        {
            get => State.GoblinMinerSystem;
            set => State.GoblinMinerSystem = value;
        }

        public TreasureChest EquipmentChestSystem
        {
            get => State.EquipmentChestSystem;
            set => State.EquipmentChestSystem = value;
        }

        public Collection CollectionSystem
        {
            get => State.CollectionSystem;
            set => State.CollectionSystem = value;
        }

        public DailyResetSystem DailyResetSystem
        {
            get => State.DailyResetSystem;
            set => State.DailyResetSystem = value;
        }

        public EventManager EventManagerSystem
        {
            get => State.EventManagerSystem;
            set => State.EventManagerSystem = value;
        }

        public DailyRoutineScheduler RoutineScheduler
        {
            get => State.RoutineScheduler;
            set => State.RoutineScheduler = value;
        }

        public ChapterTreasure ChapterTreasureSystem
        {
            get => State.ChapterTreasureSystem;
            set => State.ChapterTreasureSystem = value;
        }

        public AttendanceSystem AttendanceSystem
        {
            get => State.AttendanceSystem;
            set => State.AttendanceSystem = value;
        }

        public BattleManager BattleManagerService;
        public Forge ForgeService;
        public EquipmentManager EquipmentManagerService;
        public PetManager PetManagerService;

        public SeededRandom Rng;
        public SaveManager SaveManagerSystem;

        private float _lastTickTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
            Initialize();
        }

        private void Initialize()
        {
            State = new GameState();

            BattleManagerService = new BattleManager();
            ForgeService = new Forge();
            EquipmentManagerService = new EquipmentManager();
            PetManagerService = new PetManager();

            Rng = new SeededRandom(Environment.TickCount);
            SaveManagerSystem = new SaveManager();

            _lastTickTime = Time.realtimeSinceStartup;

            if (SaveManagerSystem.HasSave())
            {
                LoadGame();
                CheckDailyReset();
            }
            else
            {
                InitNewGame();
            }
        }

        private void Update()
        {
            if (Player == null) return;
            float now = Time.realtimeSinceStartup;
            float deltaMs = (now - _lastTickTime) * 1000f;
            _lastTickTime = now;
            Player.Resources.Tick(deltaMs);
        }

        private void InitNewGame()
        {
            Player.Resources.SetAmount(ResourceType.GOLD, 500);
            Player.Resources.SetAmount(ResourceType.GEMS, 500);
            Player.Resources.SetAmount(ResourceType.STAMINA, 100);
            Player.Resources.DailyReset();
            EventManagerSystem.CreateDailyQuests();
            EventManagerSystem.CreateWeeklyQuests();
        }

        public void StartChapter(int chapterId, ChapterType type)
        {
            int staminaCost = 5;
            if (!Player.Resources.CanAfford(ResourceType.STAMINA, staminaCost)) return;
            Player.Resources.Spend(ResourceType.STAMINA, staminaCost);
            CurrentChapter = new Chapter(chapterId, type, Rng.NextInt(0, 999999));
        }

        public Result<DungeonBattleResult> ChallengeDungeon(DungeonType type)
        {
            if (!DungeonManager.IsAvailable())
                return Result.Fail<DungeonBattleResult>("Daily limit reached");

            var dungeon = DungeonManager.GetDungeon(type);
            var stats = Player.ComputeStats();
            var petAbility = BattleManagerService.GetPetAbilitySkill(Player);
            var passives = new List<PassiveSkill>();
            if (petAbility != null) passives.Add(petAbility);
            var playerUnit = new BattleUnit("Capybara", stats, null, passives.ToArray(), true);

            var result = dungeon.CreateBattle(playerUnit);
            if (result.IsOk())
                DungeonManager.ConsumeEntry();

            return result;
        }

        public Reward OnDungeonBattleResult(DungeonType type, BattleState state)
        {
            if (state != BattleState.VICTORY) return null;
            var dungeon = DungeonManager.GetDungeon(type);
            var reward = dungeon.OnBattleVictory();
            foreach (var r in reward.Resources)
                Player.Resources.Add(r.Type, r.Amount);
            return reward;
        }

        public Result<SweepResult> SweepDungeon(DungeonType type)
        {
            if (!DungeonManager.IsAvailable())
                return Result.Fail<SweepResult>("Daily limit reached");

            var dungeon = DungeonManager.GetDungeon(type);
            if (dungeon.ClearedStage <= 0)
                return Result.Fail<SweepResult>("No cleared stages");

            DungeonManager.ConsumeEntry();
            var reward = dungeon.GetSweepReward();
            foreach (var r in reward.Resources)
                Player.Resources.Add(r.Type, r.Amount);

            return Result.Ok(new SweepResult { Reward = reward });
        }

        public PullResult PullGacha()
        {
            int cost = EquipmentChestSystem.GetCostPerPull();
            if (!Player.Resources.CanAfford(ResourceType.GEMS, cost))
                return null;

            Player.Resources.Spend(ResourceType.GEMS, cost);
            return EquipmentChestSystem.Pull(Rng);
        }

        public List<PullResult> PullGacha10()
        {
            int cost = EquipmentChestSystem.GetPull10Cost();
            if (!Player.Resources.CanAfford(ResourceType.GEMS, cost))
                return null;

            Player.Resources.Spend(ResourceType.GEMS, cost);
            return EquipmentChestSystem.Pull10(Rng);
        }

        public void UpdateQuestProgress(string missionId, int amount = 1)
        {
            foreach (var evt in EventManagerSystem.GetActiveEvents())
                evt.UpdateMissionProgress(missionId, amount);
        }

        public Result ClaimChapterTreasure(string milestoneId)
        {
            var milestone = ChapterTreasureTable.GetMilestoneById(milestoneId);
            if (milestone == null) return Result.Fail("\ub9c8\uc77c\uc2a4\ud1a4\uc744 \ucc3e\uc744 \uc218 \uc5c6\uc2b5\ub2c8\ub2e4");
            return ChapterTreasureSystem.Claim(milestone, Player);
        }

        public AttendanceClaimResult ClaimAttendance()
        {
            if (!AttendanceSystem.CanCheckIn()) return null;
            int day = AttendanceSystem.CheckIn();
            if (day < 1) return null;

            var rewardDef = AttendanceDataTable.GetReward(day);
            if (rewardDef == null) return null;

            Pet pet = null;
            Equipment equipment = null;

            if (rewardDef.Type == "RESOURCE" && rewardDef.Resources != null)
            {
                foreach (var r in rewardDef.Resources)
                    Player.Resources.Add(r.Type, r.Amount);
            }
            else if (rewardDef.Type == "PET")
            {
                var template = PetTable.GetRandomTemplate(Rng);
                pet = new Pet(
                    $"attendance_pet_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Rng.NextInt(0, 9999)}",
                    template.Name,
                    template.Tier,
                    rewardDef.PetGrade ?? PetGrade.EPIC,
                    template.MaxGrade,
                    1,
                    template.BasePassiveBonus);
                Player.OwnedPets.Add(pet);
            }
            else if (rewardDef.Type == "EQUIPMENT_GACHA")
            {
                var pullResult = EquipmentChestSystem.Pull(Rng);
                if (pullResult.Equipment != null)
                {
                    Player.AddToInventory(pullResult.Equipment);
                    equipment = pullResult.Equipment;
                }
                foreach (var r in pullResult.Resources)
                    Player.Resources.Add(r.Type, r.Amount);
            }

            SaveGame();
            return new AttendanceClaimResult { Day = day, Pet = pet, Equipment = equipment };
        }

        public void CheckDailyReset()
        {
            if (DailyResetSystem.NeedsReset())
            {
                DailyResetSystem.PerformReset(Player.Resources, DungeonManager);
                EventManagerSystem.CleanupExpired();
                EventManagerSystem.CreateDailyQuests();
                if (!EventManagerSystem.HasActiveWeeklyQuest())
                    EventManagerSystem.CreateWeeklyQuests();
                if (AttendanceSystem.IsComplete())
                    AttendanceSystem.ResetCycle();
            }
        }

        public bool SaveGame()
        {
            var saveState = SaveSerializer.Serialize(this);
            string json = JsonConvert.SerializeObject(saveState);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            var result = SaveManagerSystem.Save(dict);

            if (result.IsOk() && ServerSyncService.Instance != null)
                ServerSyncService.Instance.MarkSaveDirty();

            return result.IsOk();
        }

        public bool LoadGame()
        {
            var result = SaveManagerSystem.Load();
            if (result.IsFail() || result.Data == null) return false;
            try
            {
                string json = JsonConvert.SerializeObject(result.Data.PlayerData);
                var saveState = JsonConvert.DeserializeObject<SaveState>(json);
                SaveSerializer.Deserialize(saveState, this);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteSave()
        {
            return SaveManagerSystem.DeleteSave().IsOk();
        }

        public void ResetToNewGame()
        {
            SaveManagerSystem.DeleteSave();
            State.Reset();
            BattleManagerService = new BattleManager();
            ForgeService = new Forge();
            EquipmentManagerService = new EquipmentManager();
            PetManagerService = new PetManager();
            Rng = new SeededRandom(Environment.TickCount);
            InitNewGame();
        }

        public bool HasSave()
        {
            return SaveManagerSystem.HasSave();
        }

        public string ExportSave()
        {
            var saveState = SaveSerializer.Serialize(this);
            string json = JsonConvert.SerializeObject(saveState);
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
        }

        public bool ImportSave(string encoded)
        {
            try
            {
                string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var saveState = JsonConvert.DeserializeObject<SaveState>(json);
                SaveSerializer.Deserialize(saveState, this);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public long? GetLastSaveTime()
        {
            return SaveManagerSystem.GetLastSaveTime();
        }

        public Result<TalentUpgradeResult> TalentUpgrade(StatType statType)
        {
            var talent = Player.Talent;
            int gold = (int)Player.Resources.Gold;
            var result = talent.Upgrade(statType, gold);
            if (result.IsFail()) return result;
            Player.Resources.Spend(ResourceType.GOLD, result.Data.Cost);
            SaveGame();
            return result;
        }

        public Result ClaimTalentMilestone(int milestoneLevel)
        {
            string key = Player.Talent.GetMilestoneKey(milestoneLevel);
            if (Player.ClaimedMilestones.Contains(key))
                return Result.Fail("Already claimed");

            var milestones = TalentTable.GetAllMilestones();
            int idx = milestones.FindIndex(m => m.Level == milestoneLevel);
            if (idx < 0)
                return Result.Fail("Milestone not found");

            if (Player.Talent.GetTotalLevel() < milestoneLevel)
                return Result.Fail("Level not reached");

            var milestone = milestones[idx];
            Player.ClaimedMilestones.Add(key);
            if (milestone.RewardType != "GOLD_BOOST")
                Player.Resources.Add(ResourceType.GOLD, milestone.RewardAmount);

            SaveGame();
            return Result.Ok();
        }

        public Result<int> ClaimAllTalentMilestones()
        {
            var claimable = Player.Talent.GetClaimableMilestones(Player.ClaimedMilestones);
            if (claimable.Count == 0)
                return Result.Fail<int>("nothing to claim");

            foreach (var milestone in claimable)
            {
                string key = Player.Talent.GetMilestoneKey(milestone.Level);
                Player.ClaimedMilestones.Add(key);
                if (milestone.RewardType != "GOLD_BOOST")
                    Player.Resources.Add(ResourceType.GOLD, milestone.RewardAmount);
            }

            SaveGame();
            return Result.Ok(claimable.Count);
        }

        public Result UpgradeEquipment(string equipmentId)
        {
            Equipment equipment = null;
            SlotType foundSlotType = SlotType.WEAPON;
            int foundSlotIndex = -1;

            foreach (var kv in Player.EquipmentSlots)
            {
                var slot = kv.Value;
                for (int i = 0; i < slot.Equipped.Length; i++)
                {
                    if (slot.Equipped[i] != null && slot.Equipped[i].Id == equipmentId)
                    {
                        equipment = slot.Equipped[i];
                        foundSlotType = kv.Key;
                        foundSlotIndex = i;
                        break;
                    }
                }
                if (equipment != null) break;
            }

            if (equipment == null)
                return Result.Fail("Equipment not found in slots");

            int stones = (int)Player.Resources.EquipmentStones;
            var result = equipment.Upgrade(stones);
            if (result.IsFail()) return Result.Fail(result.Message);

            Player.Resources.Spend(ResourceType.EQUIPMENT_STONE, result.Data.Cost);
            var slot2 = Player.GetEquipmentSlot(foundSlotType);
            slot2.SyncLevel(foundSlotIndex);
            SaveGame();
            return Result.Ok();
        }

        public Result EquipItem(string equipmentId)
        {
            var equipped = Player.EquipFromInventory(equipmentId);
            if (equipped == null) return Result.Fail("Cannot equip");
            SaveGame();
            return Result.Ok();
        }

        public Result UnequipItem(SlotType slotType, int index)
        {
            Player.UnequipToInventory(slotType, index);
            SaveGame();
            return Result.Ok();
        }

        public Result SellEquipment(string equipmentId)
        {
            Player.SellEquipment(equipmentId);
            SaveGame();
            return Result.Ok();
        }

        public Result<ForgeResult> ForgeEquipment(List<string> equipmentIds)
        {
            var equipments = new List<Equipment>();
            foreach (var id in equipmentIds)
            {
                var eq = Player.Inventory.FirstOrDefault(e => e.Id == id);
                if (eq == null) return Result.Fail<ForgeResult>("Equipment not found");
                equipments.Add(eq);
            }

            var result = ForgeService.Merge(equipments, Rng);
            if (result.IsFail()) return result;

            foreach (var eq in equipments)
                Player.RemoveFromInventory(eq.Id);
            Player.AddToInventory(result.Data.Result);
            SaveGame();
            return result;
        }

        public Result<BulkForgeResult> BulkForge()
        {
            var candidates = ForgeService.FindMergeCandidates(Player.Inventory);
            if (candidates.Count == 0)
                return Result.Fail<BulkForgeResult>("No merge candidates");

            int merged = 0;
            foreach (var group in candidates)
            {
                var result = ForgeService.Merge(group, Rng);
                if (result.IsOk())
                {
                    foreach (var eq in group)
                        Player.RemoveFromInventory(eq.Id);
                    Player.AddToInventory(result.Data.Result);
                    merged++;
                }
            }

            SaveGame();
            return Result.Ok(new BulkForgeResult { MergedCount = merged });
        }

        public Result<Pet> HatchPet()
        {
            int eggs = (int)Player.Resources.Get(ResourceType.PET_EGG);
            if (eggs < 1) return Result.Fail<Pet>("Not enough eggs");

            Player.Resources.Spend(ResourceType.PET_EGG, 1);
            var template = PetTable.GetRandomTemplate(Rng);
            var pet = new Pet(
                $"hatch_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Rng.NextInt(0, 9999)}",
                template.Name,
                template.Tier,
                PetGrade.COMMON,
                template.MaxGrade,
                1,
                template.BasePassiveBonus);
            Player.AddPet(pet);

            if (Player.ActivePet == null)
                Player.SetActivePet(pet);

            SaveGame();
            return Result.Ok(pet);
        }

        public Result FeedPet(string petId, int amount)
        {
            var pet = Player.OwnedPets.FirstOrDefault(p => p.Id == petId);
            if (pet == null) return Result.Fail("Pet not found");

            int food = (int)Player.Resources.Get(ResourceType.PET_FOOD);
            if (food < amount) return Result.Fail("Not enough food");

            var result = pet.Feed(amount);
            if (result.IsFail()) return Result.Fail(result.Message);

            Player.Resources.Spend(ResourceType.PET_FOOD, amount);
            SaveGame();
            return Result.Ok();
        }

        public Result DeployPet(string petId)
        {
            var pet = Player.OwnedPets.FirstOrDefault(p => p.Id == petId);
            if (pet == null) return Result.Fail("Pet not found");
            Player.SetActivePet(pet);
            SaveGame();
            return Result.Ok();
        }

        public Result<TowerActionResult> TowerChallenge()
        {
            int tokens = (int)Player.Resources.ChallengeTokens;
            if (tokens < 1)
                return Result.Fail<TowerActionResult>("Not enough tokens");

            var playerUnit = BattleManagerService.CreatePlayerUnit(Player, null, new PassiveSkill[0]);
            var result = Tower.Challenge(playerUnit, tokens);
            if (result.IsFail())
                return Result.Fail<TowerActionResult>(result.Message);

            var battle = result.Data.Battle;
            battle.RunToCompletion(50);

            var battleResult = Tower.OnBattleResult(battle.State);
            if (battleResult.TokenConsumed)
                Player.Resources.Spend(ResourceType.CHALLENGE_TOKEN, 1);

            Reward reward = null;
            if (battleResult.Advanced && battleResult.Reward != null)
            {
                reward = battleResult.Reward;
                foreach (var r in reward.Resources)
                    Player.Resources.Add(r.Type, r.Amount);
            }

            SaveGame();
            return Result.Ok(new TowerActionResult
            {
                BattleState = battle.State,
                Reward = reward,
                Advanced = battleResult.Advanced,
            });
        }

        public Result<DungeonChallengeResult> DungeonChallenge(DungeonType type)
        {
            var challengeResult = ChallengeDungeon(type);
            if (challengeResult.IsFail())
                return Result.Fail<DungeonChallengeResult>(challengeResult.Message);

            var battle = challengeResult.Data.Battle;
            battle.RunToCompletion(50);
            var reward = OnDungeonBattleResult(type, battle.State);

            SaveGame();
            return Result.Ok(new DungeonChallengeResult
            {
                BattleState = battle.State,
                Reward = reward,
            });
        }

        public Result<SweepResult> DungeonSweep(DungeonType type)
        {
            var result = SweepDungeon(type);
            if (result.IsOk()) SaveGame();
            return result;
        }

        public Result<GoblinMineResult> GoblinMine()
        {
            int pickaxes = (int)Player.Resources.Pickaxes;
            var result = GoblinMinerSystem.Mine(pickaxes);
            if (result.IsFail())
                return Result.Fail<GoblinMineResult>(result.Message);

            Player.Resources.Spend(ResourceType.PICKAXE, 1);
            SaveGame();
            return Result.Ok(new GoblinMineResult { OreGained = result.Data.OreGained, TotalOre = GoblinMinerSystem.OreCount });
        }

        public Result<Reward> GoblinCart()
        {
            var result = GoblinMinerSystem.UseCart(Rng);
            if (result.IsFail())
                return Result.Fail<Reward>(result.Message);

            foreach (var r in result.Data.Reward.Resources)
                Player.Resources.Add(r.Type, r.Amount);

            SaveGame();
            return Result.Ok(result.Data.Reward);
        }

        public Result CatacombStart()
        {
            Catacomb.StartRun();
            SaveGame();
            return Result.Ok();
        }

        public Result<CatacombRunResult> CatacombBattle()
        {
            var playerUnit = BattleManagerService.CreatePlayerUnit(Player, null, new PassiveSkill[0]);
            var battle = Catacomb.GetNextBattle(playerUnit);
            if (battle == null)
                return Result.Fail<CatacombRunResult>("No battle available");

            battle.RunToCompletion(50);
            var result = Catacomb.OnBattleResult(battle.State);

            if (!result.ContinueRun)
            {
                foreach (var r in result.Reward.Resources)
                    Player.Resources.Add(r.Type, r.Amount);
            }

            SaveGame();
            return Result.Ok(new CatacombRunResult
            {
                ContinueRun = result.ContinueRun,
                Reward = result.Reward,
                CurrentFloor = Catacomb.CurrentRunFloor,
                BattleIndex = Catacomb.CurrentBattleIndex,
            });
        }

        public Result<Reward> CatacombEnd()
        {
            var reward = Catacomb.EndRun();
            foreach (var r in reward.Resources)
                Player.Resources.Add(r.Type, r.Amount);
            SaveGame();
            return Result.Ok(reward);
        }

        public Result ClaimQuestReward(string eventId, string missionId)
        {
            GameEvent evt = null;
            foreach (var e in EventManagerSystem.GetActiveEvents())
            {
                if (e.Id == eventId) { evt = e; break; }
            }
            if (evt == null)
                return Result.Fail("Event not found");

            var reward = evt.ClaimMissionReward(missionId);
            if (reward == null)
                return Result.Fail("Cannot claim");

            foreach (var r in reward.Resources)
                Player.Resources.Add(r.Type, r.Amount);

            SaveGame();
            return Result.Ok();
        }

        public Result ClaimAllQuestRewards(string eventId)
        {
            GameEvent evt = null;
            foreach (var e in EventManagerSystem.GetActiveEvents())
            {
                if (e.Id == eventId) { evt = e; break; }
            }
            if (evt == null)
                return Result.Fail("Event not found");

            bool claimed = false;
            foreach (var mission in evt.Missions)
            {
                if (mission.Current >= mission.Target && !mission.Claimed)
                {
                    var reward = evt.ClaimMissionReward(mission.Id);
                    if (reward != null)
                    {
                        foreach (var r in reward.Resources)
                            Player.Resources.Add(r.Type, r.Amount);
                        claimed = true;
                    }
                }
            }

            if (!claimed)
                return Result.Fail("No claimable rewards");

            SaveGame();
            return Result.Ok();
        }

        public Result<HeritageUpgradeResult> UpgradeHeritage(string route)
        {
            if (!Player.IsHeritageUnlocked())
                return Result.Fail<HeritageUpgradeResult>("Heritage not unlocked");

            if (!Enum.TryParse<HeritageRoute>(route, out var parsedRoute))
                return Result.Fail<HeritageUpgradeResult>("Invalid route");

            if (Player.Heritage.Route != parsedRoute)
                return Result.Fail<HeritageUpgradeResult>("Wrong route");

            var bookType = Player.Heritage.GetRequiredBookType();
            int books = (int)Player.Resources.Get(bookType);
            var result = Player.Heritage.Upgrade(books);
            if (result.IsFail())
                return Result.Fail<HeritageUpgradeResult>(result.Message);

            Player.Resources.Spend(bookType, result.Data.Cost);
            SaveGame();
            return result;
        }
    }

    public class SweepResult
    {
        public Reward Reward;
    }

    public class AttendanceClaimResult
    {
        public int Day;
        public Pet Pet;
        public Equipment Equipment;
    }

    public class BulkForgeResult
    {
        public int MergedCount;
    }

    public class TowerActionResult
    {
        public BattleState BattleState;
        public Reward Reward;
        public bool Advanced;
    }

    public class DungeonChallengeResult
    {
        public BattleState BattleState;
        public Reward Reward;
    }

    public class GoblinMineResult
    {
        public int OreGained;
        public int TotalOre;
    }

    public class CatacombRunResult
    {
        public bool ContinueRun;
        public Reward Reward;
        public int CurrentFloor;
        public int BattleIndex;
    }
}
