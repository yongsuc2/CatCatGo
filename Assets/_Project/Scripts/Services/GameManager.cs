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

        public Player Player;
        public Chapter CurrentChapter;

        public Tower Tower;
        public CatacombDungeon Catacomb;
        public DailyDungeonManager DungeonManager;
        public Arena ArenaSystem;
        public Travel TravelSystem;
        public GoblinMiner GoblinMinerSystem;

        public TreasureChest EquipmentChestSystem;
        public Collection CollectionSystem;
        public DailyResetSystem DailyResetSystem;
        public EventManager EventManagerSystem;
        public DailyRoutineScheduler RoutineScheduler;
        public OfflineRewardCalculator OfflineCalc;

        public ChapterTreasure ChapterTreasureSystem;
        public BattleManager BattleManagerService;
        public Forge ForgeService;
        public EquipmentManager EquipmentManagerService;
        public PetManager PetManagerService;
        public ResourceAllocator ResourceAllocatorService;

        public AttendanceSystem AttendanceSystem;

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
            Initialize();
        }

        private void Initialize()
        {
            Player = new Player();
            CurrentChapter = null;

            Tower = new Tower();
            Catacomb = new CatacombDungeon();
            DungeonManager = new DailyDungeonManager();
            ArenaSystem = new Arena();
            TravelSystem = new Travel();
            GoblinMinerSystem = new GoblinMiner();

            EquipmentChestSystem = new TreasureChest(ChestType.EQUIPMENT);
            CollectionSystem = new Collection();
            DailyResetSystem = new DailyResetSystem();
            EventManagerSystem = new EventManager();
            RoutineScheduler = new DailyRoutineScheduler();
            OfflineCalc = new OfflineRewardCalculator();

            ChapterTreasureSystem = new ChapterTreasure();
            BattleManagerService = new BattleManager();
            ForgeService = new Forge();
            EquipmentManagerService = new EquipmentManager();
            PetManagerService = new PetManager();
            ResourceAllocatorService = new ResourceAllocator();

            AttendanceSystem = new AttendanceSystem();

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

        public Result<TravelRunResult> TravelRun(int stamina)
        {
            var result = TravelSystem.Run(stamina, (int)Player.Resources.Stamina);
            if (result.IsOk() && result.Data != null)
            {
                Player.Resources.Spend(ResourceType.STAMINA, result.Data.StaminaSpent);
                foreach (var r in result.Data.Reward.Resources)
                    Player.Resources.Add(r.Type, r.Amount);
            }
            return result;
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
                DailyResetSystem.PerformReset(Player.Resources, DungeonManager, ArenaSystem);
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
            var state = SaveSerializer.Serialize(this);
            string json = JsonConvert.SerializeObject(state);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            var result = SaveManagerSystem.Save(dict);
            return result.IsOk();
        }

        public bool LoadGame()
        {
            var result = SaveManagerSystem.Load();
            if (result.IsFail() || result.Data == null) return false;
            try
            {
                string json = JsonConvert.SerializeObject(result.Data.PlayerData);
                var state = JsonConvert.DeserializeObject<SaveState>(json);
                SaveSerializer.Deserialize(state, this);
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
            Player = new Player();
            CurrentChapter = null;
            Tower = new Tower();
            Catacomb = new CatacombDungeon();
            DungeonManager = new DailyDungeonManager();
            ArenaSystem = new Arena();
            TravelSystem = new Travel();
            GoblinMinerSystem = new GoblinMiner();
            EquipmentChestSystem = new TreasureChest(ChestType.EQUIPMENT);
            CollectionSystem = new Collection();
            DailyResetSystem = new DailyResetSystem();
            EventManagerSystem = new EventManager();
            RoutineScheduler = new DailyRoutineScheduler();
            OfflineCalc = new OfflineRewardCalculator();
            ChapterTreasureSystem = new ChapterTreasure();
            BattleManagerService = new BattleManager();
            ForgeService = new Forge();
            EquipmentManagerService = new EquipmentManager();
            PetManagerService = new PetManager();
            ResourceAllocatorService = new ResourceAllocator();
            AttendanceSystem = new AttendanceSystem();
            Rng = new SeededRandom(Environment.TickCount);
            InitNewGame();
        }

        public bool HasSave()
        {
            return SaveManagerSystem.HasSave();
        }

        public string ExportSave()
        {
            var state = SaveSerializer.Serialize(this);
            string json = JsonConvert.SerializeObject(state);
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
        }

        public bool ImportSave(string encoded)
        {
            try
            {
                string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var state = JsonConvert.DeserializeObject<SaveState>(json);
                SaveSerializer.Deserialize(state, this);
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
}
