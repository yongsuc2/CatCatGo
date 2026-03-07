using UnityEngine;

namespace CatCatGo.Network
{
    [CreateAssetMenu(fileName = "ServerConfig", menuName = "CatCatGo/ServerConfig")]
    public class ServerConfig : ScriptableObject
    {
        public string BaseUrl = "http://localhost:5000";
        public float RequestTimeoutSeconds = 10f;
        public int MaxRetryCount = 2;
        public float RetryDelaySeconds = 1f;
        public float TokenRefreshBufferSeconds = 60f;

        private static ServerConfig _instance;

        public static ServerConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ServerConfig>("ServerConfig");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<ServerConfig>();
                    }
                }
                return _instance;
            }
        }
    }
}
