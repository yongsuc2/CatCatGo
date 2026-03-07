using System;

namespace CatCatGo.Shared.Responses
{
    [Serializable]
    public class BattleStartResponse
    {
        public string BattleId;
        public int Seed;
    }

    [Serializable]
    public class BattleReportResponse
    {
        public bool Verified;
        public string RewardsJson;
        public string Error;
    }
}
