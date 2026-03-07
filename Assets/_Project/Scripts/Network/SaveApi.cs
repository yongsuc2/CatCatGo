using System;
using System.Security.Cryptography;
using System.Text;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Network
{
    public static class SaveApi
    {
        public static void Load(Action<ApiResponse<SaveSyncResponse>> callback)
        {
            ApiClient.Instance.Get("api/save", callback);
        }

        public static void Sync(string saveDataJson, int version, Action<ApiResponse<SaveSyncResponse>> callback)
        {
            var request = new SaveSyncRequest
            {
                Data = saveDataJson,
                ClientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Version = version,
                Checksum = ComputeChecksum(saveDataJson)
            };
            ApiClient.Instance.Post("api/save/sync", request, callback);
        }

        private static string ComputeChecksum(string data)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
