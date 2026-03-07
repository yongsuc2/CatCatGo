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

    public class ChapterStartResponseData
    {
        public string SessionId;
        public int Seed;
    }

    public class ChapterAdvanceDayResponseData
    {
        public EncounterDeltaData Encounter;
        public string BattleRequired;
        public int BattleSeed;
        public string EnemyTemplateId;
    }

    public class EncounterDeltaData
    {
        public string Type;
        public List<EncounterOptionDeltaData> Options;
    }

    public class EncounterOptionDeltaData
    {
        public string Label;
        public string Description;
        public string SkillId;
        public RewardData Reward;
    }

    public class ChapterResolveEncounterResponseData
    {
        public RewardData Reward;
    }

    public class ChapterRerollResponseData
    {
        public EncounterDeltaData Encounter;
    }

    public class ChapterSelectSkillResponseData
    {
        public string SkillId;
    }

    public class ChapterBattleResultResponseData
    {
        public bool Verified;
        public int GoldEarned;
        public bool SessionEnded;
        public bool IsVictory;
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
