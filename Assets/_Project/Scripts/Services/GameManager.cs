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
using CatCatGo.Network;
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

        public TreasureChest AdventurerChestSystem
        {
            get => State.AdventurerChestSystem;
            set => State.AdventurerChestSystem = value;
        }

        public TreasureChest HeroChestSystem
        {
            get => State.HeroChestSystem;
            set => State.HeroChestSystem = value;
        }

        public TreasureChest GetChestSystem(ChestType type)
        {
            switch (type)
            {
                case ChestType.EQUIPMENT: return EquipmentChestSystem;
                case ChestType.ADVENTURER: return AdventurerChestSystem;
                case ChestType.HERO: return HeroChestSystem;
                default: return null;
            }
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

        private NetworkMode _networkMode = NetworkMode.OFFLINE;
        private int _consecutiveFailures;
        private const int MaxConsecutiveFailures = 3;
        private string _chapterSessionId;
        public string ChapterSessionId => _chapterSessionId;

        public NetworkMode CurrentNetworkMode => _networkMode;
        public bool IsOnline => _networkMode == NetworkMode.ONLINE;

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
            var cfg = BattleDataTable.Data.NewGameResources;
            Player.Resources.SetAmount(ResourceType.GOLD, cfg.Gold);
            Player.Resources.SetAmount(ResourceType.GEMS, cfg.Gems);
            Player.Resources.SetAmount(ResourceType.STAMINA, cfg.Stamina);
            Player.Resources.DailyReset();
            EventManagerSystem.CreateDailyQuests();
            EventManagerSystem.CreateWeeklyQuests();
        }

        public void StartChapter(int chapterId, ChapterType type)
        {
            int staminaCost = BattleDataTable.Data.ChapterStaminaCost;
            if (!Player.Resources.CanAfford(ResourceType.STAMINA, staminaCost)) return;
            Player.Resources.Spend(ResourceType.STAMINA, staminaCost);
            CurrentChapter = new Chapter(chapterId, type, Rng.NextInt(0, 999999));
            GameLog.I("Game", $"StartChapter id={chapterId} type={type} stamina=-{staminaCost}");
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
            return PullChest(ChestType.EQUIPMENT);
        }

        public List<PullResult> PullGacha10()
        {
            return PullChest10(ChestType.EQUIPMENT);
        }

        public PullResult PullChest(ChestType chestType)
        {
            var chest = GetChestSystem(chestType);
            if (chest == null) return null;

            int cost = chest.GetCostPerPull();
            var currency = chest.GetCostCurrency();
            if (!Player.Resources.CanAfford(currency, cost))
                return null;

            Player.Resources.Spend(currency, cost);
            var result = chest.Pull(Rng);
            if (result != null)
            {
                if (result.Equipment != null)
                    Player.AddToInventory(result.Equipment);
                foreach (var r in result.Resources)
                    Player.Resources.Add(r.Type, r.Amount);
                SaveGame();
                GameLog.I("Game", $"PullChest({chestType}) {currency}=-{cost} eq={result.Equipment?.Name ?? "none"}({result.Equipment?.Grade})");
            }
            return result;
        }

        public List<PullResult> PullChest10(ChestType chestType)
        {
            var chest = GetChestSystem(chestType);
            if (chest == null) return null;

            int cost = chest.GetPull10Cost();
            var currency = chest.GetCostCurrency();
            if (!Player.Resources.CanAfford(currency, cost))
                return null;

            Player.Resources.Spend(currency, cost);
            var results = chest.Pull10(Rng);
            if (results != null)
            {
                foreach (var result in results)
                {
                    if (result.Equipment != null)
                        Player.AddToInventory(result.Equipment);
                    foreach (var r in result.Resources)
                        Player.Resources.Add(r.Type, r.Amount);
                }
                SaveGame();
            }
            return results;
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

            if (result.IsFail())
                GameLog.W("Game", $"SaveGame failed: {result.Message}");

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

            if (IsOnline)
            {
                var sync = ServerSyncService.Instance;
                if (sync != null)
                    sync.ClearPendingSave();

                AuthApi.ResetData(response =>
                {
                    if (response.IsSuccess)
                        GameLog.I("Game", "Server data reset successful");
                    else
                        GameLog.W("Game", $"Server data reset failed: {response.ErrorMessage}");
                });
            }
        }

        public void DeleteAccountAsync(Action<bool> callback)
        {
            AuthApi.DeleteAccount(response =>
            {
                if (response.IsSuccess)
                    ResetToNewGame();
                callback(response.IsSuccess);
            });
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

        public void SetNetworkMode(NetworkMode mode)
        {
            if (_networkMode == mode) return;
            _networkMode = mode;
            _consecutiveFailures = 0;
            GameLog.I("Game", $"NetworkMode -> {mode}");
            EventBus.Publish(new NetworkModeChangedEvent { Mode = mode });
        }

        private void OnApiSuccess()
        {
            _consecutiveFailures = 0;
        }

        private void OnApiFailed()
        {
            OnApiFailed(null);
        }

        private void OnApiFailed(string errorCode)
        {
            string toastMessage = ErrorCodeMessages.GetMessage(errorCode);
            if (toastMessage != null)
                EventBus.Publish(new ErrorToastEvent { Message = toastMessage });

            if (!string.IsNullOrEmpty(errorCode))
            {
                _consecutiveFailures = 0;
                return;
            }

            _consecutiveFailures++;
            GameLog.W("Game", $"ApiFailed consecutive={_consecutiveFailures}/{MaxConsecutiveFailures} errorCode={errorCode ?? "network"}");
            if (_consecutiveFailures >= MaxConsecutiveFailures && _networkMode == NetworkMode.ONLINE)
            {
                SetNetworkMode(NetworkMode.OFFLINE);
                SaveGame();
            }
        }

        private void ApplyServerDelta(StateDelta delta)
        {
            if (delta != null)
                State.ApplyDelta(delta);
        }

        public Result<TalentUpgradeResult> TalentUpgrade(StatType statType)
        {
            var talent = Player.Talent;
            int gold = (int)Player.Resources.Gold;
            var result = talent.Upgrade(statType, gold);
            if (result.IsFail()) return result;
            Player.Resources.Spend(ResourceType.GOLD, result.Data.Cost);
            SaveGame();
            GameLog.I("Game", $"TalentUpgrade {statType} gold=-{result.Data.Cost}");
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
            GameLog.I("Game", $"UpgradeEquipment id={equipmentId} lv={equipment.Level} stone=-{result.Data.Cost}");
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

        #region ONLINE API Methods

        public void TalentUpgradeAsync(StatType statType, Action<Result<TalentUpgradeResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(TalentUpgrade(statType)); return; }

            string statName = statType == StatType.ATK ? "ATK" : statType == StatType.HP ? "HP" : "DEF";
            TalentApi.Upgrade(statName, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<TalentUpgradeResult>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok(new TalentUpgradeResult()));
            });
        }

        public void ClaimTalentMilestoneAsync(int milestoneLevel, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(ClaimTalentMilestone(milestoneLevel)); return; }

            TalentApi.ClaimMilestone(milestoneLevel, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void ClaimAllTalentMilestonesAsync(Action<Result<int>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(ClaimAllTalentMilestones()); return; }

            TalentApi.ClaimAllMilestones(response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<int>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                int count = response.Data.Data?.ClaimedCount ?? 0;
                callback(Result.Ok(count));
            });
        }

        public void UpgradeEquipmentAsync(string equipmentId, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(UpgradeEquipment(equipmentId)); return; }

            EquipmentApi.Upgrade(equipmentId, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void EquipItemAsync(string equipmentId, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(EquipItem(equipmentId)); return; }

            EquipmentApi.Equip(equipmentId, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void UnequipItemAsync(SlotType slotType, int index, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(UnequipItem(slotType, index)); return; }

            EquipmentApi.Unequip(slotType.ToString(), index, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void SellEquipmentAsync(string equipmentId, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(SellEquipment(equipmentId)); return; }

            EquipmentApi.Sell(equipmentId, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void ForgeEquipmentAsync(List<string> equipmentIds, Action<Result<ForgeResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(ForgeEquipment(equipmentIds)); return; }

            EquipmentApi.Forge(equipmentIds, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<ForgeResult>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok(new ForgeResult()));
            });
        }

        public void BulkForgeAsync(Action<Result<BulkForgeResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(BulkForge()); return; }

            EquipmentApi.BulkForge(response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<BulkForgeResult>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                int count = response.Data.Data?.MergedCount ?? 0;
                callback(Result.Ok(new BulkForgeResult { MergedCount = count }));
            });
        }

        public void HatchPetAsync(Action<Result<Pet>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(HatchPet()); return; }

            PetApi.Hatch(response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<Pet>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                Pet pet = null;
                if (response.Data.Delta?.AddedPets != null && response.Data.Delta.AddedPets.Count > 0)
                    pet = Player.OwnedPets.LastOrDefault();
                callback(Result.Ok(pet));
            });
        }

        public void FeedPetAsync(string petId, int amount, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(FeedPet(petId, amount)); return; }

            PetApi.Feed(petId, amount, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void DeployPetAsync(string petId, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(DeployPet(petId)); return; }

            PetApi.Deploy(petId, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void PullGachaAsync(Action<PullResult> callback)
        {
            PullChestAsync(ChestType.EQUIPMENT, callback);
        }

        public void PullGacha10Async(Action<List<PullResult>> callback)
        {
            PullChest10Async(ChestType.EQUIPMENT, callback);
        }

        public void PullChestAsync(ChestType chestType, Action<PullResult> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(PullChest(chestType)); return; }

            GachaApi.Pull(response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(null);
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(null);
            });
        }

        public void PullChest10Async(ChestType chestType, Action<List<PullResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(PullChest10(chestType)); return; }

            GachaApi.Pull10(response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(null);
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(null);
            });
        }

        public void TowerChallengeAsync(Action<Result<TowerActionResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(TowerChallenge()); return; }

            ContentApi.TowerChallenge(response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<TowerActionResult>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                var data = response.Data.Data;
                var battleState = data?.BattleResult == "VICTORY" ? BattleState.VICTORY : BattleState.DEFEAT;
                Reward reward = ConvertRewardData(data?.Reward);
                callback(Result.Ok(new TowerActionResult
                {
                    BattleState = battleState,
                    Reward = reward,
                    Advanced = battleState == BattleState.VICTORY,
                }));
            });
        }

        public void DungeonChallengeAsync(DungeonType type, Action<Result<DungeonChallengeResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(DungeonChallenge(type)); return; }

            ContentApi.DungeonChallenge(type.ToString(), response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<DungeonChallengeResult>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                var data = response.Data.Data;
                var battleState = data?.BattleResult == "VICTORY" ? BattleState.VICTORY : BattleState.DEFEAT;
                callback(Result.Ok(new DungeonChallengeResult
                {
                    BattleState = battleState,
                    Reward = ConvertRewardData(data?.Reward),
                }));
            });
        }

        public void DungeonSweepAsync(DungeonType type, Action<Result<SweepResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(DungeonSweep(type)); return; }

            ContentApi.DungeonSweep(type.ToString(), response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<SweepResult>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok(new SweepResult { Reward = ConvertRewardData(response.Data.Data?.Reward) }));
            });
        }

        public void GoblinMineAsync(Action<Result<GoblinMineResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(GoblinMine()); return; }

            ContentApi.GoblinMine(response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<GoblinMineResult>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                int oreGained = response.Data.Data?.OreGained ?? 0;
                callback(Result.Ok(new GoblinMineResult { OreGained = oreGained, TotalOre = GoblinMinerSystem.OreCount }));
            });
        }

        public void GoblinCartAsync(Action<Result<Reward>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(GoblinCart()); return; }

            ContentApi.GoblinCart(response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<Reward>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok(ConvertRewardData(response.Data.Data?.Reward)));
            });
        }

        public void CatacombStartAsync(Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(CatacombStart()); return; }

            ContentApi.CatacombStart(response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void CatacombBattleAsync(Action<Result<CatacombRunResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(CatacombBattle()); return; }

            ContentApi.CatacombBattle(response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<CatacombRunResult>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                var data = response.Data.Data;
                callback(Result.Ok(new CatacombRunResult
                {
                    ContinueRun = data?.ContinueRun ?? false,
                    Reward = ConvertRewardData(data?.Reward),
                    CurrentFloor = Catacomb.CurrentRunFloor,
                    BattleIndex = Catacomb.CurrentBattleIndex,
                }));
            });
        }

        public void CatacombEndAsync(Action<Result<Reward>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(CatacombEnd()); return; }

            ContentApi.CatacombEnd(response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<Reward>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok(ConvertRewardData(response.Data.Data?.Reward)));
            });
        }

        public void ClaimQuestRewardAsync(string eventId, string missionId, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(ClaimQuestReward(eventId, missionId)); return; }

            DailyApi.ClaimQuest(eventId, missionId, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void ClaimAllQuestRewardsAsync(string eventId, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(ClaimAllQuestRewards(eventId)); return; }

            DailyApi.ClaimAllQuests(eventId, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void UpgradeHeritageAsync(string route, Action<Result<HeritageUpgradeResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(UpgradeHeritage(route)); return; }

            HeritageApi.Upgrade(route, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<HeritageUpgradeResult>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok(new HeritageUpgradeResult()));
            });
        }

        public void ClaimChapterTreasureAsync(string milestoneId, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(ClaimChapterTreasure(milestoneId)); return; }

            TreasureApi.Claim(milestoneId, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void ClaimAttendanceAsync(Action<AttendanceClaimResult> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE) { callback(ClaimAttendance()); return; }

            DailyApi.ClaimAttendance(response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(null);
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                int day = response.Data.Data?.Day ?? 0;
                callback(new AttendanceClaimResult { Day = day });
            });
        }

        public void ChapterStartAsync(int chapterId, ChapterType type, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE)
            {
                StartChapter(chapterId, type);
                callback(CurrentChapter != null ? Result.Ok() : Result.Fail("Failed to start chapter"));
                return;
            }

            ChapterApi.Start(chapterId, type.ToString(), response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                _chapterSessionId = response.Data.Data?.SessionId;
                callback(Result.Ok());
            });
        }

        public void ChapterAdvanceDayAsync(Action<Result<ChapterAdvanceDayResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE)
            {
                var chapter = CurrentChapter;
                if (chapter == null) { callback(Result.Fail<ChapterAdvanceDayResult>("No active chapter")); return; }
                var enc = chapter.AdvanceDay();
                callback(Result.Ok(new ChapterAdvanceDayResult
                {
                    Encounter = enc,
                    BattleRequired = chapter.IsBossDay() ? "BOSS" : chapter.IsEliteDay() ? "ELITE" : chapter.IsMidBossDay() ? "MIDBOSS" : chapter.IsOptionalEliteDay() ? "ELITE" : null,
                }));
                return;
            }

            ChapterApi.AdvanceDay(_chapterSessionId, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<ChapterAdvanceDayResult>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                var data = response.Data.Data;
                callback(Result.Ok(new ChapterAdvanceDayResult
                {
                    BattleRequired = data?.BattleRequired,
                    BattleSeed = data?.BattleSeed ?? 0,
                    EnemyTemplateId = data?.EnemyTemplateId,
                    ServerEncounter = data?.Encounter,
                }));
            });
        }

        public void ChapterResolveEncounterAsync(int choiceIndex, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE)
            {
                var chapter = CurrentChapter;
                if (chapter == null) { callback(Result.Fail("No active chapter")); return; }
                var result = chapter.ResolveEncounter(choiceIndex, chapter.SessionCurrentHp, chapter.SessionMaxHp);
                if (result?.Reward != null)
                {
                    float goldMult = Player.GetGoldMultiplier();
                    foreach (var r in result.Reward.Resources)
                    {
                        int amt = r.Type == ResourceType.GOLD ? Mathf.FloorToInt(r.Amount * goldMult) : r.Amount;
                        Player.Resources.Add(r.Type, amt);
                    }
                }
                callback(Result.Ok());
                return;
            }

            ChapterApi.ResolveEncounter(_chapterSessionId, choiceIndex, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void ChapterSelectSkillAsync(string skillId, Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE)
            {
                var chapter = CurrentChapter;
                if (chapter == null) { callback(Result.Fail("No active chapter")); return; }
                var skill = EncounterGenerator.FindSkillById(skillId);
                if (skill != null)
                {
                    int existing = chapter.SessionSkills.FindIndex(s => s.Id == skill.Id);
                    if (existing >= 0)
                        chapter.SessionSkills[existing] = skill;
                    else
                        chapter.SessionSkills.Add(skill);
                    chapter.RecalcSessionMaxHp();
                }
                callback(Result.Ok());
                return;
            }

            ChapterApi.SelectSkill(_chapterSessionId, skillId, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok());
            });
        }

        public void ChapterRerollAsync(Action<Result<ChapterRerollResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE)
            {
                var chapter = CurrentChapter;
                if (chapter == null) { callback(Result.Fail<ChapterRerollResult>("No active chapter")); return; }
                var newEnc = chapter.RerollEncounter();
                callback(Result.Ok(new ChapterRerollResult { Encounter = newEnc }));
                return;
            }

            ChapterApi.Reroll(_chapterSessionId, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<ChapterRerollResult>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                callback(Result.Ok(new ChapterRerollResult { ServerEncounter = response.Data.Data?.Encounter }));
            });
        }

        public void ChapterBattleResultAsync(int battleSeed, string result, int turnCount, int playerRemainingHp,
            Action<Result<ChapterBattleResultResult>> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE)
            {
                callback(Result.Ok(new ChapterBattleResultResult { Verified = true }));
                return;
            }

            ChapterApi.BattleResult(_chapterSessionId, battleSeed, result, turnCount, playerRemainingHp, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail<ChapterBattleResultResult>(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                var data = response.Data.Data;
                if (data != null && data.SessionEnded)
                    _chapterSessionId = null;
                callback(Result.Ok(new ChapterBattleResultResult
                {
                    Verified = data?.Verified ?? false,
                    GoldEarned = data?.GoldEarned ?? 0,
                    SessionEnded = data?.SessionEnded ?? false,
                }));
            });
        }

        public void ChapterAbandonAsync(Action<Result> callback)
        {
            if (_networkMode == NetworkMode.OFFLINE)
            {
                var chapter = CurrentChapter;
                if (chapter != null)
                    Player.UpdateBestSurvivalDay(chapter.Id, chapter.CurrentDay, false);
                CurrentChapter = null;
                SaveGame();
                callback(Result.Ok());
                return;
            }

            ChapterApi.Abandon(_chapterSessionId, response =>
            {
                if (!response.IsSuccess || response.Data == null || !response.Data.Success)
                {
                    OnApiFailed(response.Data?.ErrorCode);
                    callback(Result.Fail(response.Data?.ErrorCode ?? "SERVER_ERROR"));
                    return;
                }
                OnApiSuccess();
                ApplyServerDelta(response.Data.Delta);
                _chapterSessionId = null;
                callback(Result.Ok());
            });
        }

        private Reward ConvertRewardData(CatCatGo.Network.RewardData rewardData)
        {
            if (rewardData?.Resources == null) return null;
            var rewardResources = new List<ResourceReward>();
            foreach (var r in rewardData.Resources)
            {
                if (Enum.TryParse<ResourceType>(r.Type, out var resType))
                    rewardResources.Add(new ResourceReward(resType, r.Amount));
            }
            return new Reward(rewardResources);
        }

        #endregion
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

    public class ChapterAdvanceDayResult
    {
        public Encounter Encounter;
        public string BattleRequired;
        public int BattleSeed;
        public string EnemyTemplateId;
        public EncounterDeltaData ServerEncounter;
    }

    public class ChapterRerollResult
    {
        public Encounter Encounter;
        public EncounterDeltaData ServerEncounter;
    }

    public class ChapterBattleResultResult
    {
        public bool Verified;
        public int GoldEarned;
        public bool SessionEnded;
    }
}
