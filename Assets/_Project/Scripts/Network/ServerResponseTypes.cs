using System.Collections.Generic;

namespace CatCatGo.Network
{
    public class TalentMilestoneResponseData
    {
        public string RewardType;
        public int RewardAmount;
    }

    public class ClaimAllMilestonesResponseData
    {
        public int ClaimedCount;
    }

    public class PetHatchResponseData
    {
        public PetDeltaData Pet;
    }

    public class GachaPullResponseData
    {
        public List<GachaPullResultData> Results;
    }

    public class GachaPullResultData
    {
        public EquipmentDeltaData Equipment;
        public Dictionary<string, float> Resources;
    }

    public class TowerChallengeResponseData
    {
        public string BattleResult;
        public RewardData Reward;
    }

    public class DungeonChallengeResponseData
    {
        public string BattleResult;
        public RewardData Reward;
    }

    public class DungeonSweepResponseData
    {
        public RewardData Reward;
    }

    public class GoblinMineResponseData
    {
        public int OreGained;
    }

    public class GoblinCartResponseData
    {
        public RewardData Reward;
    }

    public class CatacombBattleResponseData
    {
        public string BattleResult;
        public bool ContinueRun;
        public RewardData Reward;
    }

    public class CatacombEndResponseData
    {
        public RewardData Reward;
    }

    public class BulkForgeResponseData
    {
        public int MergedCount;
    }

    public class QuestClaimAllResponseData
    {
        public int ClaimedCount;
    }

    public class AttendanceClaimResponseData
    {
        public int Day;
        public string RewardType;
    }

    public class RewardData
    {
        public List<RewardResourceData> Resources;
    }

    public class RewardResourceData
    {
        public string Type;
        public int Amount;
    }

    public class SyncFullResponseData
    {
        public string SaveState;
        public long ServerTimestamp;
    }

    public class SyncPushResponseData
    {
        public bool Accepted;
        public long ServerTimestamp;
    }
}
