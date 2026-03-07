using System;
using UnityEngine;

namespace CatCatGo.Network
{
    public class TokenStore
    {
        private const string AccessTokenKey = "catcatgo_access_token";
        private const string RefreshTokenKey = "catcatgo_refresh_token";
        private const string AccountIdKey = "catcatgo_account_id";
        private const string ExpiresAtKey = "catcatgo_expires_at";

        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public string AccountId { get; private set; }
        public long ExpiresAt { get; private set; }

        public bool HasValidToken
        {
            get
            {
                if (string.IsNullOrEmpty(AccessToken)) return false;
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return now < ExpiresAt - (long)ServerConfig.Instance.TokenRefreshBufferSeconds;
            }
        }

        public bool HasRefreshToken => !string.IsNullOrEmpty(RefreshToken);

        public bool NeedsRefresh
        {
            get
            {
                if (string.IsNullOrEmpty(AccessToken)) return false;
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return now >= ExpiresAt - (long)ServerConfig.Instance.TokenRefreshBufferSeconds;
            }
        }

        public void Save(string accountId, string accessToken, string refreshToken, long expiresAt)
        {
            AccountId = accountId;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;

            PlayerPrefs.SetString(AccountIdKey, accountId);
            PlayerPrefs.SetString(AccessTokenKey, accessToken);
            PlayerPrefs.SetString(RefreshTokenKey, refreshToken);
            PlayerPrefs.SetString(ExpiresAtKey, expiresAt.ToString());
            PlayerPrefs.Save();
        }

        public void LoadFromDisk()
        {
            AccountId = PlayerPrefs.GetString(AccountIdKey, "");
            AccessToken = PlayerPrefs.GetString(AccessTokenKey, "");
            RefreshToken = PlayerPrefs.GetString(RefreshTokenKey, "");
            string expiresStr = PlayerPrefs.GetString(ExpiresAtKey, "0");
            long.TryParse(expiresStr, out long expires);
            ExpiresAt = expires;
        }

        public void Clear()
        {
            AccountId = null;
            AccessToken = null;
            RefreshToken = null;
            ExpiresAt = 0;

            PlayerPrefs.DeleteKey(AccountIdKey);
            PlayerPrefs.DeleteKey(AccessTokenKey);
            PlayerPrefs.DeleteKey(RefreshTokenKey);
            PlayerPrefs.DeleteKey(ExpiresAtKey);
            PlayerPrefs.Save();
        }
    }
}
