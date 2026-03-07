using CatCatGo.Shared.Models;

namespace CatCatGo.Server.Core.Services;

public class StateDeltaBuilder
{
    private readonly StateDelta _delta = new();

    public StateDelta Build() => _delta;

    public StateDeltaBuilder SetResources(Dictionary<string, float> resources)
    {
        _delta.Resources = resources;
        return this;
    }

    public StateDeltaBuilder AddResource(string type, float amount)
    {
        _delta.Resources ??= new Dictionary<string, float>();
        if (_delta.Resources.ContainsKey(type))
            _delta.Resources[type] += amount;
        else
            _delta.Resources[type] = amount;
        return this;
    }

    public StateDeltaBuilder MergeResourceSnapshot(Dictionary<string, double> before, Dictionary<string, double> after)
    {
        _delta.Resources ??= new Dictionary<string, float>();
        var allKeys = before.Keys.Union(after.Keys).Distinct();
        foreach (var key in allKeys)
        {
            var oldVal = before.GetValueOrDefault(key, 0);
            var newVal = after.GetValueOrDefault(key, 0);
            if (Math.Abs(oldVal - newVal) > 0.001)
                _delta.Resources[key] = (float)newVal;
        }
        return this;
    }

    public StateDeltaBuilder SetResourceBalances(Dictionary<string, double> balances, IEnumerable<string> changedTypes)
    {
        _delta.Resources ??= new Dictionary<string, float>();
        foreach (var type in changedTypes)
        {
            if (balances.TryGetValue(type, out var val))
                _delta.Resources[type] = (float)val;
            else
                _delta.Resources[type] = 0;
        }
        return this;
    }

    public StateDeltaBuilder SetTalent(int atkLevel, int hpLevel, int defLevel)
    {
        _delta.Talent = new TalentDelta { AtkLevel = atkLevel, HpLevel = hpLevel, DefLevel = defLevel };
        return this;
    }

    public StateDeltaBuilder SetHeritage(string route, int level)
    {
        _delta.Heritage = new HeritageDelta { Route = route, Level = level };
        return this;
    }

    public StateDeltaBuilder AddEquipment(EquipmentDeltaData equipment)
    {
        _delta.AddedEquipments ??= new List<EquipmentDeltaData>();
        _delta.AddedEquipments.Add(equipment);
        return this;
    }

    public StateDeltaBuilder RemoveEquipment(string equipmentId)
    {
        _delta.RemovedEquipmentIds ??= new List<string>();
        _delta.RemovedEquipmentIds.Add(equipmentId);
        return this;
    }

    public StateDeltaBuilder UpgradeEquipment(string equipmentId, int newLevel, int newPromoteCount)
    {
        _delta.UpgradedEquipments ??= new List<EquipmentUpgradeDelta>();
        _delta.UpgradedEquipments.Add(new EquipmentUpgradeDelta
        {
            EquipmentId = equipmentId,
            NewLevel = newLevel,
            NewPromoteCount = newPromoteCount,
        });
        return this;
    }

    public StateDeltaBuilder ChangeEquipSlot(string slotType, int index, string? equipmentId, int slotLevel)
    {
        _delta.EquipmentSlotChanges ??= new List<EquipSlotDelta>();
        _delta.EquipmentSlotChanges.Add(new EquipSlotDelta
        {
            SlotType = slotType,
            Index = index,
            EquipmentId = equipmentId,
            SlotLevel = slotLevel,
        });
        return this;
    }

    public StateDeltaBuilder AddPet(PetDeltaData pet)
    {
        _delta.AddedPets ??= new List<PetDeltaData>();
        _delta.AddedPets.Add(pet);
        return this;
    }

    public StateDeltaBuilder UpdatePet(string petId, int? level = null, int? exp = null, string? grade = null)
    {
        _delta.UpdatedPets ??= new List<PetUpdateDelta>();
        _delta.UpdatedPets.Add(new PetUpdateDelta { PetId = petId, Level = level, Exp = exp, Grade = grade });
        return this;
    }

    public StateDeltaBuilder SetActivePet(string petId)
    {
        _delta.ActivePetId = petId;
        return this;
    }

    public StateDeltaBuilder SetClearedChapterMax(int value)
    {
        _delta.ClearedChapterMax = value;
        return this;
    }

    public StateDeltaBuilder SetBestSurvivalDays(Dictionary<string, int> days)
    {
        _delta.BestSurvivalDays = days;
        return this;
    }

    public StateDeltaBuilder AddClaimedMilestone(string milestone)
    {
        _delta.AddedClaimedMilestones ??= new List<string>();
        _delta.AddedClaimedMilestones.Add(milestone);
        return this;
    }

    public StateDeltaBuilder SetTower(int? currentFloor = null, int? currentStage = null)
    {
        _delta.Tower = new TowerDelta { CurrentFloor = currentFloor, CurrentStage = currentStage };
        return this;
    }

    public StateDeltaBuilder SetCatacomb(int? highestFloor = null, int? currentRunFloor = null, bool? isRunning = null)
    {
        _delta.Catacomb = new CatacombDelta { HighestFloor = highestFloor, CurrentRunFloor = currentRunFloor, IsRunning = isRunning };
        return this;
    }

    public StateDeltaBuilder SetDungeons(int? todayCount = null, Dictionary<string, int>? clearedStages = null)
    {
        _delta.Dungeons = new DungeonDelta { TodayCount = todayCount, ClearedStages = clearedStages };
        return this;
    }

    public StateDeltaBuilder SetGoblinOreCount(int count)
    {
        _delta.GoblinOreCount = count;
        return this;
    }

    public StateDeltaBuilder SetPityCount(int count)
    {
        _delta.PityCount = count;
        return this;
    }

    public StateDeltaBuilder AddMissionUpdate(string eventId, string missionId, int? current = null, bool? claimed = null)
    {
        _delta.MissionUpdates ??= new List<MissionDelta>();
        _delta.MissionUpdates.Add(new MissionDelta { EventId = eventId, MissionId = missionId, Current = current, Claimed = claimed });
        return this;
    }

    public StateDeltaBuilder SetAttendance(bool[] checkedDays, string lastCheckDate)
    {
        _delta.Attendance = new AttendanceDelta { CheckedDays = checkedDays, LastCheckDate = lastCheckDate };
        return this;
    }

    public StateDeltaBuilder SetChapterSession(ChapterSessionDelta session)
    {
        _delta.ChapterSession = session;
        return this;
    }
}
