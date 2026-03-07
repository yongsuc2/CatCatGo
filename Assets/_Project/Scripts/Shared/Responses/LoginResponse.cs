using System;

namespace CatCatGo.Shared.Responses
{
    [Serializable]
    public class LoginResponse
    {
        public string AccountId;
        public string AccessToken;
        public string RefreshToken;
        public long ExpiresAt;
        public bool IsNewAccount;
    }
}
