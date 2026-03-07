using System;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
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
                    Debug.Log("[ServerSync] New account registered");
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
            SyncApi.GetFull(response =>
            {
                if (!response.IsSuccess || response.Data == null || response.Data.Data == null)
                {
                    onComplete(response.IsSuccess);
                    return;
                }

                var serverData = response.Data.Data;
                if (string.IsNullOrEmpty(serverData.SaveState))
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
                    var serverState = JsonConvert.DeserializeObject<SaveState>(serverData.SaveState);
                    game.State.ApplyFullSync(serverState);
                    Debug.Log("[ServerSync] Full sync applied from server");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ServerSync] Failed to parse server save: {ex.Message}");
                }

                onComplete(true);
            });
        }

        public void MarkSaveDirty()
        {
            _pendingSave = true;
        }

        public void SyncSaveToServer()
        {
            if (State != ConnectionState.Online) return;

            var game = GameManager.Instance;
            if (game == null) return;

            var saveState = SaveSerializer.Serialize(game);
            string json = JsonConvert.SerializeObject(saveState);
            long clientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            SyncApi.Push(json, clientTimestamp, response =>
            {
                if (response.IsSuccess && response.Data?.Data != null)
                {
                    if (response.Data.Data.Accepted)
                    {
                        _pendingSave = false;
                        _lastSyncTime = Time.realtimeSinceStartup;
                        Debug.Log("[ServerSync] Save synced to server");
                    }
                    else
                    {
                        Debug.Log("[ServerSync] Push rejected, loading server state");
                        LoadFullSync(_ => { });
                    }
                }
                else if (response.IsOffline)
                {
                    SetState(ConnectionState.Offline);
                    Debug.Log("[ServerSync] Offline - save kept locally");
                }
                else
                {
                    Debug.LogWarning($"[ServerSync] Sync failed: {response.ErrorMessage}");
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
