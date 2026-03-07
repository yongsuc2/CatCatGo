using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Network
{
    public class ApiClient : MonoBehaviour
    {
        public static ApiClient Instance { get; private set; }

        public TokenStore TokenStore { get; private set; }
        public bool IsOnline { get; private set; }

        private ServerConfig _config;
        private bool _isRefreshing;
        private Coroutine _connectivityCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _config = ServerConfig.Instance;
            TokenStore = new TokenStore();
            TokenStore.LoadFromDisk();
        }

        private void OnEnable()
        {
            _connectivityCoroutine = StartCoroutine(CheckConnectivityLoop());
        }

        private void OnDisable()
        {
            if (_connectivityCoroutine != null)
                StopCoroutine(_connectivityCoroutine);
        }

        private IEnumerator CheckConnectivityLoop()
        {
            while (true)
            {
                yield return PingServer(result => IsOnline = result);
                yield return new WaitForSeconds(30f);
            }
        }

        private IEnumerator PingServer(Action<bool> callback)
        {
            using (var request = UnityWebRequest.Head(_config.BaseUrl))
            {
                request.timeout = 5;
                yield return request.SendWebRequest();
                callback(request.result == UnityWebRequest.Result.Success);
            }
        }

        public void Get<T>(string endpoint, Action<ApiResponse<T>> callback)
        {
            StartCoroutine(SendRequest<T>("GET", endpoint, null, true, callback));
        }

        public void Post<T>(string endpoint, object body, Action<ApiResponse<T>> callback, bool requiresAuth = true)
        {
            StartCoroutine(SendRequest<T>("POST", endpoint, body, requiresAuth, callback));
        }

        public void PostNoResponse(string endpoint, object body, Action<ApiResponse<object>> callback, bool requiresAuth = true)
        {
            StartCoroutine(SendRequest<object>("POST", endpoint, body, requiresAuth, callback));
        }

        private IEnumerator SendRequest<T>(string method, string endpoint, object body, bool requiresAuth, Action<ApiResponse<T>> callback, int retryCount = 0)
        {
            if (requiresAuth && TokenStore.NeedsRefresh && TokenStore.HasRefreshToken && !_isRefreshing)
            {
                yield return RefreshTokenCoroutine();
            }

            if (requiresAuth && !TokenStore.HasValidToken && !TokenStore.HasRefreshToken)
            {
                callback(ApiResponse<T>.Offline("Not authenticated"));
                yield break;
            }

            string url = _config.BaseUrl + "/" + endpoint;
            Debug.Log($"[Net] {method} {url}");
            UnityWebRequest request;

            if (method == "GET")
            {
                request = UnityWebRequest.Get(url);
            }
            else
            {
                string json = body != null ? JsonConvert.SerializeObject(body) : "{}";
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request = new UnityWebRequest(url, "POST");
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
            }

            request.timeout = (int)_config.RequestTimeoutSeconds;

            if (requiresAuth && !string.IsNullOrEmpty(TokenStore.AccessToken))
            {
                request.SetRequestHeader("Authorization", "Bearer " + TokenStore.AccessToken);
            }

            using (request)
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    IsOnline = false;
                    Debug.LogWarning($"[Net] {method} {endpoint} FAILED: {request.error} (retry {retryCount}/{_config.MaxRetryCount})");
                    if (retryCount < _config.MaxRetryCount)
                    {
                        yield return new WaitForSeconds(_config.RetryDelaySeconds);
                        yield return SendRequest<T>(method, endpoint, body, requiresAuth, callback, retryCount + 1);
                        yield break;
                    }
                    callback(ApiResponse<T>.Offline(request.error));
                    yield break;
                }

                IsOnline = true;
                int statusCode = (int)request.responseCode;

                if (statusCode == 401 && requiresAuth && TokenStore.HasRefreshToken && retryCount == 0)
                {
                    yield return RefreshTokenCoroutine();
                    if (TokenStore.HasValidToken)
                    {
                        yield return SendRequest<T>(method, endpoint, body, requiresAuth, callback, retryCount + 1);
                        yield break;
                    }
                    callback(ApiResponse<T>.Fail(401, "Authentication failed after refresh"));
                    yield break;
                }

                Debug.Log($"[Net] {method} {endpoint} → {statusCode}");

                if (statusCode >= 200 && statusCode < 300)
                {
                    string responseText = request.downloadHandler.text;
                    if (string.IsNullOrEmpty(responseText) || typeof(T) == typeof(object))
                    {
                        callback(ApiResponse<T>.Success(default, statusCode));
                    }
                    else
                    {
                        try
                        {
                            T data = JsonConvert.DeserializeObject<T>(responseText);
                            callback(ApiResponse<T>.Success(data, statusCode));
                        }
                        catch (Exception ex)
                        {
                            callback(ApiResponse<T>.Fail(statusCode, "JSON parse error: " + ex.Message));
                        }
                    }
                }
                else
                {
                    string errorBody = request.downloadHandler?.text ?? "";
                    callback(ApiResponse<T>.Fail(statusCode, $"HTTP {statusCode}: {errorBody}"));
                }
            }
        }

        private IEnumerator RefreshTokenCoroutine()
        {
            if (_isRefreshing) yield break;
            _isRefreshing = true;

            var refreshRequest = new RefreshTokenRequest { RefreshToken = TokenStore.RefreshToken };
            bool done = false;

            yield return SendRequest<LoginResponse>("POST", "api/auth/refresh", refreshRequest, false,
                response =>
                {
                    if (response.IsSuccess && response.Data != null)
                    {
                        TokenStore.Save(
                            response.Data.AccountId,
                            response.Data.AccessToken,
                            response.Data.RefreshToken,
                            response.Data.ExpiresAt);
                    }
                    else
                    {
                        TokenStore.Clear();
                    }
                    done = true;
                });

            while (!done)
                yield return null;

            _isRefreshing = false;
        }
    }
}
