using System;
using System.Collections.Generic;

namespace CatCatGo.Shared.Responses
{
    [Serializable]
    public class ArenaMatchResponse
    {
        public string MatchId;
        public List<ArenaOpponentDto> Opponents;
    }

    [Serializable]
    public class ArenaOpponentDto
    {
        public string AccountId;
        public string DisplayName;
        public string Tier;
        public int Points;
        public string PlayerDataJson;
    }

    [Serializable]
    public class ArenaRankingResponse
    {
        public List<ArenaRankEntry> Rankings;
        public int MyRank;
        public string MyTier;
        public int MyPoints;
    }

    [Serializable]
    public class ArenaRankEntry
    {
        public int Rank;
        public string DisplayName;
        public string Tier;
        public int Points;
    }
}
