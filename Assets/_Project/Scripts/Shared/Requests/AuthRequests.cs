using System;

namespace CatCatGo.Shared.Requests
{
    [Serializable]
    public class RegisterRequest
    {
        public string DeviceId;
        public string DisplayName;
    }

    [Serializable]
    public class LoginRequest
    {
        public string DeviceId;
        public string SocialToken;
        public string SocialType;
    }

    [Serializable]
    public class RefreshTokenRequest
    {
        public string RefreshToken;
    }
}
