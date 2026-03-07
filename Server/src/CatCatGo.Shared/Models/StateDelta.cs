namespace CatCatGo.Shared.Models;

public class StateDelta
{
    public Dictionary<string, float>? Resources { get; set; }

    public TalentDelta? Talent { get; set; }
    public HeritageDelta? Heritage { get; set; }

    public List<EquipmentDeltaData>? AddedEquipments { get; set; }
    public List<string>? RemovedEquipmentIds { get; set; }
    public List<EquipmentUpgradeDelta>? UpgradedEquipments { get; set; }
    public List<EquipSlotDelta>? EquipmentSlotChanges { get; set; }

    public List<PetDeltaData>? AddedPets { get; set; }
    public List<PetUpdateDelta>? UpdatedPets { get; set; }
    public string? ActivePetId { get; set; }

    public int? ClearedChapterMax { get; set; }
    public Dictionary<string, int>? BestSurvivalDays { get; set; }
    public List<string>? AddedClaimedMilestones { get; set; }

    public TowerDelta? Tower { get; set; }
    public CatacombDelta? Catacomb { get; set; }
    public DungeonDelta? Dungeons { get; set; }
    public int? GoblinOreCount { get; set; }
    public int? PityCount { get; set; }
    public List<string>? AddedCollectionIds { get; set; }
    public List<MissionDelta>? MissionUpdates { get; set; }
    public AttendanceDelta? Attendance { get; set; }
    public ChapterSessionDelta? ChapterSession { get; set; }

    public long ServerTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}

public class TalentDelta
{
    public int? AtkLevel { get; set; }
    public int? HpLevel { get; set; }
    public int? DefLevel { get; set; }
}

public class HeritageDelta
{
    public string? Route { get; set; }
    public int? Level { get; set; }
}

public class EquipmentDeltaData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slot { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public bool IsS { get; set; }
    public int Level { get; set; }
    public int PromoteCount { get; set; }
    public int MergeLevel { get; set; }
    public string? WeaponSubType { get; set; }
    public List<SubStatDeltaData>? SubStats { get; set; }
}

public class SubStatDeltaData
{
    public string Stat { get; set; } = string.Empty;
    public float Value { get; set; }
}

public class EquipmentUpgradeDelta
{
    public string EquipmentId { get; set; } = string.Empty;
    public int NewLevel { get; set; }
    public int NewPromoteCount { get; set; }
}

public class EquipSlotDelta
{
    public string SlotType { get; set; } = string.Empty;
    public int Index { get; set; }
    public string? EquipmentId { get; set; }
    public int SlotLevel { get; set; }
}

public class PetDeltaData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Exp { get; set; }
}

public class PetUpdateDelta
{
    public string PetId { get; set; } = string.Empty;
    public int? Level { get; set; }
    public int? Exp { get; set; }
    public string? Grade { get; set; }
}

public class TowerDelta
{
    public int? CurrentFloor { get; set; }
    public int? CurrentStage { get; set; }
}

public class CatacombDelta
{
    public int? HighestFloor { get; set; }
    public int? CurrentRunFloor { get; set; }
    public bool? IsRunning { get; set; }
}

public class DungeonDelta
{
    public int? TodayCount { get; set; }
    public Dictionary<string, int>? ClearedStages { get; set; }
}

public class MissionDelta
{
    public string EventId { get; set; } = string.Empty;
    public string MissionId { get; set; } = string.Empty;
    public int? Current { get; set; }
    public bool? Claimed { get; set; }
}

public class AttendanceDelta
{
    public bool[]? CheckedDays { get; set; }
    public string? LastCheckDate { get; set; }
}

public class ChapterSessionDelta
{
    public string? SessionId { get; set; }
    public int? CurrentDay { get; set; }
    public int? SessionCurrentHp { get; set; }
    public int? SessionMaxHp { get; set; }
    public int? SessionGold { get; set; }
    public int? JungbakCount { get; set; }
    public int? DaebakCount { get; set; }
    public int? SessionRerollsRemaining { get; set; }
    public List<string>? SessionSkillIds { get; set; }
    public bool? SessionEnded { get; set; }
}
