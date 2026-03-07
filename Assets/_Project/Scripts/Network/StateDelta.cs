using System.Collections.Generic;

namespace CatCatGo.Network
{
    public class StateDelta
    {
        public Dictionary<string, float> Resources;

        public TalentDelta Talent;
        public HeritageDelta Heritage;

        public List<EquipmentDeltaData> AddedEquipments;
        public List<string> RemovedEquipmentIds;
        public List<EquipmentUpgradeDelta> UpgradedEquipments;
        public List<EquipSlotDelta> EquipmentSlotChanges;

        public List<PetDeltaData> AddedPets;
        public List<PetUpdateDelta> UpdatedPets;
        public string ActivePetId;

        public int? ClearedChapterMax;
        public Dictionary<string, int> BestSurvivalDays;
        public List<string> AddedClaimedMilestones;

        public TowerDelta Tower;
        public CatacombDelta Catacomb;
        public DungeonDelta Dungeons;
        public int? GoblinOreCount;
        public int? PityCount;
        public List<string> AddedCollectionIds;
        public List<MissionDelta> MissionUpdates;
        public AttendanceDelta Attendance;
        public ChapterSessionDelta ChapterSession;

        public long ServerTimestamp;
    }

    public class TalentDelta
    {
        public int? AtkLevel;
        public int? HpLevel;
        public int? DefLevel;
    }

    public class HeritageDelta
    {
        public string Route;
        public int? Level;
    }

    public class EquipmentDeltaData
    {
        public string Id;
        public string Name;
        public string Slot;
        public string Grade;
        public bool IsS;
        public int Level;
        public int PromoteCount;
        public int MergeLevel;
        public string WeaponSubType;
        public List<SubStatDeltaData> SubStats;
    }

    public class SubStatDeltaData
    {
        public string Stat;
        public float Value;
    }

    public class EquipmentUpgradeDelta
    {
        public string EquipmentId;
        public int NewLevel;
        public int NewPromoteCount;
    }

    public class EquipSlotDelta
    {
        public string SlotType;
        public int Index;
        public string EquipmentId;
        public int SlotLevel;
    }

    public class PetDeltaData
    {
        public string Id;
        public string Name;
        public string Tier;
        public string Grade;
        public int Level;
        public int Exp;
    }

    public class PetUpdateDelta
    {
        public string PetId;
        public int? Level;
        public int? Exp;
        public string Grade;
    }

    public class TowerDelta
    {
        public int? CurrentFloor;
        public int? CurrentStage;
    }

    public class CatacombDelta
    {
        public int? HighestFloor;
        public int? CurrentRunFloor;
        public bool? IsRunning;
    }

    public class DungeonDelta
    {
        public int? TodayCount;
        public Dictionary<string, int> ClearedStages;
    }

    public class MissionDelta
    {
        public string EventId;
        public string MissionId;
        public int? Current;
        public bool? Claimed;
    }

    public class AttendanceDelta
    {
        public bool[] CheckedDays;
        public string LastCheckDate;
    }

    public class ChapterSessionDelta
    {
        public string SessionId;
        public int? CurrentDay;
        public int? SessionCurrentHp;
        public int? SessionMaxHp;
        public int? SessionGold;
        public int? JungbakCount;
        public int? DaebakCount;
        public int? SessionRerollsRemaining;
        public List<string> SessionSkillIds;
        public bool? SessionEnded;
    }
}
