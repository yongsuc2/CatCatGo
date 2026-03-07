using System;

namespace CatCatGo.Shared.Responses
{
    [Serializable]
    public class SaveSyncResponse
    {
        public string Action;
        public string Data;
        public long ServerTimestamp;
        public int Version;
    }
}
