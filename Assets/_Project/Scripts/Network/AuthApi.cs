using System;
using UnityEngine;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;

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
