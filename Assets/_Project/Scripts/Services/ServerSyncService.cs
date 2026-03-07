using System;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using CatCatGo.Infrastructure;
using CatCatGo.Network;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Services
{
    public enum ConnectionState
    {
        Offline,
        Connecting,
        Online
    }

    public class ServerSyncService : MonoBehaviour
    {
        public static ServerSyncService Instance { get; private set; }

        public ConnectionState State { get; private set; } = ConnectionState.Offline;
        public bool IsAuthenticated => ApiClient.Instance != null && ApiClient.Instance.TokenStore.HasValidToken;

        public event Action<ConnectionState> OnConnectionStateChanged;

        private const float AutoSyncIntervalSeconds = 120f;
        private float _lastSyncTime;
        private bool _pendingSave;
        private int _saveVersion;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(InitializeConnection());
        }

        private void Update()
        {
            if (State != ConnectionState.Online) return;
            if (!_pendingSave) return;
            if (Time.realtimeSinceStartup - _lastSyncTime < AutoSyncIntervalSeconds) return;

            SyncSaveToServer();
        }

        private IEnumerator InitializeConnection()
        {
            if (ApiClient.Instance == null) yield break;

            SetState(ConnectionState.Connecting);

            bool authComplete = false;
            bool authSuccess = false;

            AuthApi.AutoLogin((success, isNew) =>
            {
                authSuccess = success;
                authComplete = true;

                if (isNew)
                    GameLog.I("Sync", "New account registered");
            });

            while (!authComplete)
                yield return null;

            if (!authSuccess)
            {
                SetState(ConnectionState.Offline);
                yield break;
            }

            bool syncComplete = false;
            bool syncSuccess = false;

            LoadFullSync(success =>
            {
                syncSuccess = success;
                syncComplete = true;
            });

            while (!syncComplete)
                yield return null;

            SetState(syncSuccess ? ConnectionState.Online : ConnectionState.Offline);
        }

        private void LoadFullSync(Action<bool> onComplete)
        {
            SyncApi.Load(response =>
            {
                if (!response.IsSuccess || response.Data == null)
                {
                    onComplete(true);
                    return;
                }

                if (string.IsNullOrEmpty(response.Data.Data))
                {
                    onComplete(true);
                    return;
                }

                var game = GameManager.Instance;
                if (game == null)
                {
                    onComplete(true);
                    return;
                }

                try
                {
                    var serverState = JsonConvert.DeserializeObject<SaveState>(response.Data.Data);
                    game.State.ApplyFullSync(serverState);
                    _saveVersion = response.Data.Version;
                    GameLog.I("Sync", "Full sync applied from server");
                }
                catch (Exception ex)
                {
                    GameLog.W("Sync", $"Failed to parse server save: {ex.Message}");
                }

                onComplete(true);
            });
        }

        public void MarkSaveDirty()
        {
            _pendingSave = true;
        }

        public void ClearPendingSave()
        {
            _pendingSave = false;
        }

        public void SyncSaveToServer()
        {
            if (State != ConnectionState.Online) return;

            var game = GameManager.Instance;
            if (game == null) return;

            var saveState = SaveSerializer.Serialize(game);
            string json = JsonConvert.SerializeObject(saveState);
            long clientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            _saveVersion++;
            int syncVersion = _saveVersion;
            SyncApi.Sync(json, clientTimestamp, syncVersion, response =>
            {
                if (response.IsSuccess && response.Data != null)
                {
                    if (response.Data.Action == "ACCEPTED")
                    {
                        _pendingSave = false;
                        _lastSyncTime = Time.realtimeSinceStartup;
                        _saveVersion = response.Data.Version;
                        GameLog.I("Sync", "Save synced to server");
                    }
                    else
                    {
                        GameLog.I("Sync", "Server has newer data, loading server state");
                        LoadFullSync(_ => { });
                    }
                }
                else if (response.IsOffline)
                {
                    _lastSyncTime = Time.realtimeSinceStartup;
                    SetState(ConnectionState.Offline);
                    GameLog.I("Sync", "Offline - save kept locally");
                }
                else
                {
                    _lastSyncTime = Time.realtimeSinceStartup;
                    GameLog.W("Sync", $"Sync failed: {response.ErrorMessage}");
                }
            });
        }

        public void RequestArenaMatch(Action<ArenaMatchResponse> onSuccess, Action<string> onFail)
        {
            if (State != ConnectionState.Online)
            {
                onFail("Offline");
                return;
            }

            ArenaApi.RequestMatch(response =>
            {
                if (response.IsSuccess && response.Data != null)
                    onSuccess(response.Data);
                else
                    onFail(response.IsOffline ? "Offline" : response.ErrorMessage);
            });
        }

        public void SubmitArenaResult(string matchId, int rank, Action<bool> onComplete)
        {
            if (State != ConnectionState.Online)
            {
                onComplete(false);
                return;
            }

            ArenaApi.SubmitResult(matchId, rank, response => onComplete(response.IsSuccess));
        }

        public void GetArenaRanking(int season, Action<ArenaRankingResponse> onSuccess, Action<string> onFail)
        {
            if (State != ConnectionState.Online)
            {
                onFail("Offline");
                return;
            }

            ArenaApi.GetRanking(season, response =>
            {
                if (response.IsSuccess && response.Data != null)
                    onSuccess(response.Data);
                else
                    onFail(response.IsOffline ? "Offline" : response.ErrorMessage);
            });
        }

        public void GetShopCatalog(Action<ShopCatalogResponse> onSuccess, Action<string> onFail)
        {
            if (State != ConnectionState.Online)
            {
                onFail("Offline");
                return;
            }

            ShopApi.GetCatalog(response =>
            {
                if (response.IsSuccess && response.Data != null)
                    onSuccess(response.Data);
                else
                    onFail(response.IsOffline ? "Offline" : response.ErrorMessage);
            });
        }

        public void VerifyPurchase(string productId, string store, string receiptId, string receiptData,
            Action<PurchaseResponse> onSuccess, Action<string> onFail)
        {
            if (State != ConnectionState.Online)
            {
                onFail("Offline");
                return;
            }

            ShopApi.Purchase(productId, store, receiptId, receiptData, response =>
            {
                if (response.IsSuccess && response.Data != null)
                    onSuccess(response.Data);
                else
                    onFail(response.IsOffline ? "Offline" : response.ErrorMessage);
            });
        }

        public void StartBattleSession(int chapterId, int day, string encounterType,
            Action<BattleStartResponse> onSuccess, Action onOffline)
        {
            if (State != ConnectionState.Online)
            {
                onOffline();
                return;
            }

            BattleApi.StartBattle(chapterId, day, encounterType, response =>
            {
                if (response.IsSuccess && response.Data != null)
                    onSuccess(response.Data);
                else
                    onOffline();
            });
        }

        public void ReportBattleResult(string battleId, int seed, string result, int turnCount,
            System.Collections.Generic.List<string> playerSkillIds, string enemyTemplateId, int goldReward,
            Action<BattleReportResponse> onSuccess, Action onOffline)
        {
            if (State != ConnectionState.Online)
            {
                onOffline();
                return;
            }

            BattleApi.ReportResult(battleId, seed, result, turnCount, playerSkillIds, enemyTemplateId, goldReward,
                response =>
                {
                    if (response.IsSuccess && response.Data != null)
                        onSuccess(response.Data);
                    else
                        onOffline();
                });
        }

        public void RetryConnection()
        {
            if (State == ConnectionState.Connecting) return;
            StartCoroutine(InitializeConnection());
        }

        private void SetState(ConnectionState newState)
        {
            if (State == newState) return;
            State = newState;
            OnConnectionStateChanged?.Invoke(newState);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetNetworkMode(newState == ConnectionState.Online
                    ? Infrastructure.NetworkMode.ONLINE
                    : Infrastructure.NetworkMode.OFFLINE);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && _pendingSave)
                SyncSaveToServer();
            else if (!pauseStatus && State == ConnectionState.Offline)
                RetryConnection();
        }
    }
}
