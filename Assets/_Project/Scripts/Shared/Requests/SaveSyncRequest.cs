using System;

namespace CatCatGo.Shared.Requests
{
    [Serializable]
    public class SaveSyncRequest
    {
        public string Data;
        public long ClientTimestamp;
        public int Version;
        public string Checksum;
    }
}
