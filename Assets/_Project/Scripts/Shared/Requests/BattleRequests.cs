using System;
using System.Collections.Generic;

namespace CatCatGo.Shared.Requests
{
    [Serializable]
    public class BattleStartRequest
    {
        public int ChapterId;
        public int Day;
        public string EncounterType;
    }

    [Serializable]
    public class BattleReportRequest
    {
        public string BattleId;
        public int Seed;
        public string Result;
        public int TurnCount;
        public List<string> PlayerSkillIds;
        public string EnemyTemplateId;
        public int GoldReward;
    }
}
