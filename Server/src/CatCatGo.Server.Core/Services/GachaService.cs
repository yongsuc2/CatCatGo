using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;

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

    public async Task<GachaResult> PullAsync(Guid accountId)
    {
        var spent = await _resourceService.SpendAsync(accountId, "GEMS", SinglePullCost, "GACHA_PULL");
        if (!spent)
            return new GachaResult { Success = false, Error = "INSUFFICIENT_GEMS" };

        var pity = await GetOrCreatePityAsync(accountId, "EQUIPMENT");
        pity.PityCount++;

        var equipment = GenerateEquipment(accountId, pity);
        await _equipmentRepo.CreateAsync(equipment);

        if (equipment.Grade == "MYTHIC")
            pity.PityCount = 0;

        pity.UpdatedAt = DateTime.UtcNow;
        await _gachaRepo.UpsertPityAsync(pity);

        return new GachaResult { Success = true, Items = new List<EquipmentEntry> { equipment } };
    }

    public async Task<GachaResult> Pull10Async(Guid accountId)
    {
        var totalCost = SinglePullCost * 9;
        var spent = await _resourceService.SpendAsync(accountId, "GEMS", totalCost, "GACHA_PULL10");
        if (!spent)
            return new GachaResult { Success = false, Error = "INSUFFICIENT_GEMS" };

        var pity = await GetOrCreatePityAsync(accountId, "EQUIPMENT");
        var items = new List<EquipmentEntry>();

        for (int i = 0; i < 10; i++)
        {
            pity.PityCount++;
            var equipment = GenerateEquipment(accountId, pity);
            await _equipmentRepo.CreateAsync(equipment);

            if (equipment.Grade == "MYTHIC")
                pity.PityCount = 0;

            items.Add(equipment);
        }

        pity.UpdatedAt = DateTime.UtcNow;
        await _gachaRepo.UpsertPityAsync(pity);

        return new GachaResult { Success = true, Items = items };
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

public class GachaResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<EquipmentEntry> Items { get; set; } = new();
}

public class GachaPityInfo
{
    public int PityCount { get; set; }
    public int Threshold { get; set; }
}
