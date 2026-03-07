using System;
using UnityEngine;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;
using CatCatGo.Services;

namespace CatCatGo.Network
{
    public static class AuthApi
    {
        public static void Register(string deviceId, string displayName, Action<ApiResponse<LoginResponse>> callback)
        {
            var request = new RegisterRequest
            {
                DeviceId = deviceId,
                DisplayName = displayName
            };
            ApiClient.Instance.Post("api/auth/register", request, callback, false);
        }

        public static void Login(string deviceId, Action<ApiResponse<LoginResponse>> callback)
        {
            var request = new LoginRequest { DeviceId = deviceId };
            ApiClient.Instance.Post("api/auth/login", request, callback, false);
        }

        public static string GetDeviceId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public static void ResetData(Action<ApiResponse<object>> callback)
        {
            ApiClient.Instance.PostNoResponse("api/auth/reset-data", null, response =>
            {
                if (response.IsSuccess && GameManager.Instance != null)
                    GameManager.Instance.ResetToNewGame();
                callback(response);
            });
        }

        public static void DeleteAccount(Action<ApiResponse<object>> callback)
        {
            ApiClient.Instance.StartCoroutine(DeleteAccountCoroutine(callback));
        }

        private static System.Collections.IEnumerator DeleteAccountCoroutine(Action<ApiResponse<object>> callback)
        {
            var url = ServerConfig.Instance.BaseUrl + "/api/auth/account";
            var request = UnityEngine.Networking.UnityWebRequest.Delete(url);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            var token = ApiClient.Instance.TokenStore.AccessToken;
            if (!string.IsNullOrEmpty(token))
                request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                ApiClient.Instance.TokenStore.Clear();
                if (GameManager.Instance != null)
                    GameManager.Instance.ResetToNewGame();
                callback(ApiResponse<object>.Success(null, (int)request.responseCode));
            }
            else
            {
                callback(ApiResponse<object>.Fail((int)request.responseCode, request.error));
            }
            request.Dispose();
        }

        public static void AutoLogin(Action<bool, bool> onComplete)
        {
            string deviceId = GetDeviceId();
            Login(deviceId, loginResponse =>
            {
                if (loginResponse.IsSuccess && loginResponse.Data != null)
                {
                    ApiClient.Instance.TokenStore.Save(
                        loginResponse.Data.AccountId,
                        loginResponse.Data.AccessToken,
                        loginResponse.Data.RefreshToken,
                        loginResponse.Data.ExpiresAt);
                    onComplete(true, false);
                    return;
                }

                if (loginResponse.IsOffline)
                {
                    onComplete(false, false);
                    return;
                }

                Register(deviceId, null, registerResponse =>
                {
                    if (registerResponse.IsSuccess && registerResponse.Data != null)
                    {
                        ApiClient.Instance.TokenStore.Save(
                            registerResponse.Data.AccountId,
                            registerResponse.Data.AccessToken,
                            registerResponse.Data.RefreshToken,
                            registerResponse.Data.ExpiresAt);
                        onComplete(true, true);
                    }
                    else
                    {
                        onComplete(false, false);
                    }
                });
            });
        }
    }
}
