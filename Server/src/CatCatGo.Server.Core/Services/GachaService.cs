using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Models;

namespace CatCatGo.Server.Core.Services;

public class GachaService
{
    private readonly IGachaRepository _gachaRepo;
    private readonly IEquipmentRepository _equipmentRepo;
    private readonly ResourceService _resourceService;

    private const int PityThreshold = 180;
    private const double SinglePullCost = 300;
    private const double SRate = 0.03;
    private static readonly string[] SEligibleGrades = { "EPIC", "LEGENDARY", "MYTHIC" };

    private static readonly Dictionary<string, double> GradeWeights = new()
    {
        { "COMMON", 100 }, { "UNCOMMON", 50 }, { "RARE", 20 },
        { "EPIC", 5 }, { "LEGENDARY", 1 }, { "MYTHIC", 0.2 },
    };

    public GachaService(IGachaRepository gachaRepo, IEquipmentRepository equipmentRepo, ResourceService resourceService)
    {
        _gachaRepo = gachaRepo;
        _equipmentRepo = equipmentRepo;
        _resourceService = resourceService;
    }

    public async Task<ApiResponse<GachaPullResponse>> PullAsync(Guid accountId, int count = 1)
    {
        var totalCost = count == 10 ? SinglePullCost * 9 : SinglePullCost * count;
        var spent = await _resourceService.SpendAsync(accountId, "GEMS", totalCost, count > 1 ? "GACHA_PULL10" : "GACHA_PULL");
        if (!spent)
            return ApiResponse<GachaPullResponse>.Fail("INSUFFICIENT_GEMS");

        var pity = await GetOrCreatePityAsync(accountId, "EQUIPMENT");
        var deltaBuilder = new StateDeltaBuilder();
        var results = new List<EquipmentDeltaData>();

        for (int i = 0; i < count; i++)
        {
            pity.PityCount++;
            var equipment = GenerateEquipment(accountId, pity);
            await _equipmentRepo.CreateAsync(equipment);

            if (equipment.Grade == "MYTHIC")
                pity.PityCount = 0;

            var eqData = ToEquipmentDeltaData(equipment);
            results.Add(eqData);
            deltaBuilder.AddEquipment(eqData);
        }

        pity.UpdatedAt = DateTime.UtcNow;
        await _gachaRepo.UpsertPityAsync(pity);

        var gemsBalance = await _resourceService.GetBalanceAsync(accountId, "GEMS");
        deltaBuilder.AddResource("GEMS", (float)gemsBalance);
        deltaBuilder.SetPityCount(pity.PityCount);

        return ApiResponse<GachaPullResponse>.Ok(
            new GachaPullResponse { Results = results },
            deltaBuilder.Build());
    }

    public async Task<ApiResponse<GachaPullResponse>> Pull10Async(Guid accountId)
    {
        return await PullAsync(accountId, 10);
    }

    public async Task<GachaPityInfo> GetPityAsync(Guid accountId)
    {
        var pity = await GetOrCreatePityAsync(accountId, "EQUIPMENT");
        return new GachaPityInfo { PityCount = pity.PityCount, Threshold = PityThreshold };
    }

    private EquipmentEntry GenerateEquipment(Guid accountId, GachaPity pity)
    {
        var grade = pity.PityCount >= PityThreshold ? "MYTHIC" : RollGrade();
        var isS = SEligibleGrades.Contains(grade) && Random.Shared.NextDouble() < SRate;

        return new EquipmentEntry
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            TemplateId = $"equipment_{grade.ToLowerInvariant()}_{Random.Shared.Next(1, 100)}",
            Grade = grade,
            EnhancementLevel = 0,
            IsS = isS,
            SubStats = "[]",
            SlotIndex = -1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    private static EquipmentDeltaData ToEquipmentDeltaData(EquipmentEntry entry) => new()
    {
        Id = entry.Id.ToString(),
        Name = entry.TemplateId,
        Slot = "",
        Grade = entry.Grade,
        IsS = entry.IsS,
        Level = entry.EnhancementLevel,
        PromoteCount = 0,
        MergeLevel = 0,
        SubStats = new List<SubStatDeltaData>(),
    };

    private static string RollGrade()
    {
        var totalWeight = GradeWeights.Values.Sum();
        var roll = Random.Shared.NextDouble() * totalWeight;
        var cumulative = 0.0;

        foreach (var (grade, weight) in GradeWeights)
        {
            cumulative += weight;
            if (roll <= cumulative)
                return grade;
        }
        return "COMMON";
    }

    private async Task<GachaPity> GetOrCreatePityAsync(Guid accountId, string boxType)
    {
        var pity = await _gachaRepo.GetPityAsync(accountId, boxType);
        if (pity == null)
        {
            pity = new GachaPity
            {
                AccountId = accountId,
                BoxType = boxType,
                PityCount = 0,
                UpdatedAt = DateTime.UtcNow,
            };
            await _gachaRepo.UpsertPityAsync(pity);
        }
        return pity;
    }
}

public class GachaPullResponse
{
    public List<EquipmentDeltaData> Results { get; set; } = new();
}

public class GachaPityInfo
{
    public int PityCount { get; set; }
    public int Threshold { get; set; }
}
