using System;
using System.Security.Cryptography;
using System.Text;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Network
{
    public static class SyncApi
    {
        public static void Load(Action<ApiResponse<SaveSyncResponse>> callback)
        {
            ApiClient.Instance.Get("api/save", callback);
        }

        public static void Sync(string data, long clientTimestamp, Action<ApiResponse<SaveSyncResponse>> callback)
        {
            var request = new SaveSyncRequest
            {
                Data = data,
                ClientTimestamp = clientTimestamp,
                Checksum = ComputeChecksum(data),
            };
            ApiClient.Instance.Post("api/save/sync", request, callback);
        }

        private static string ComputeChecksum(string data)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
