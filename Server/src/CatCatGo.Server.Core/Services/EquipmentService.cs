using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Services;

public class EquipmentService
{
    private readonly IEquipmentRepository _equipmentRepo;
    private readonly ResourceService _resourceService;

    private const int MaxEquipSlots = 3;
    private static readonly Dictionary<string, double> SellPrices = new()
    {
        { "COMMON", 10 }, { "UNCOMMON", 30 }, { "RARE", 100 },
        { "EPIC", 300 }, { "LEGENDARY", 1000 }, { "MYTHIC", 3000 },
    };

    public EquipmentService(IEquipmentRepository equipmentRepo, ResourceService resourceService)
    {
        _equipmentRepo = equipmentRepo;
        _resourceService = resourceService;
    }

    public async Task<EquipmentResult> EnhanceAsync(Guid accountId, Guid equipmentId)
    {
        var equipment = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (equipment == null || equipment.AccountId != accountId)
            return new EquipmentResult { Success = false, Error = "EQUIPMENT_NOT_FOUND" };

        var goldCost = CalculateEnhanceCost(equipment.EnhancementLevel, equipment.Grade);
        var stoneCost = 1.0;

        var goldSpent = await _resourceService.SpendAsync(accountId, "GOLD", goldCost, "EQUIPMENT_ENHANCE", equipmentId.ToString());
        if (!goldSpent)
            return new EquipmentResult { Success = false, Error = "INSUFFICIENT_GOLD" };

        var stoneSpent = await _resourceService.SpendAsync(accountId, "EQUIPMENT_STONE", stoneCost, "EQUIPMENT_ENHANCE", equipmentId.ToString());
        if (!stoneSpent)
        {
            await _resourceService.GrantAsync(accountId, "GOLD", goldCost, "EQUIPMENT_ENHANCE_REFUND", equipmentId.ToString());
            return new EquipmentResult { Success = false, Error = "INSUFFICIENT_EQUIPMENT_STONE" };
        }

        equipment.EnhancementLevel++;
        equipment.UpdatedAt = DateTime.UtcNow;
        await _equipmentRepo.UpdateAsync(equipment);

        return new EquipmentResult { Success = true, Equipment = equipment };
    }

    public async Task<EquipmentResult> ForgeAsync(Guid accountId, List<Guid> materialIds)
    {
        if (materialIds.Count < 3)
            return new EquipmentResult { Success = false, Error = "INSUFFICIENT_MATERIALS" };

        var materials = new List<EquipmentEntry>();
        foreach (var id in materialIds)
        {
            var mat = await _equipmentRepo.GetByIdAsync(id);
            if (mat == null || mat.AccountId != accountId)
                return new EquipmentResult { Success = false, Error = "MATERIAL_NOT_FOUND" };
            if (mat.SlotIndex >= 0)
                return new EquipmentResult { Success = false, Error = "MATERIAL_IS_EQUIPPED" };
            if (mat.IsS)
                return new EquipmentResult { Success = false, Error = "CANNOT_FORGE_S_GRADE" };
            materials.Add(mat);
        }

        var baseGrade = materials[0].Grade;
        if (!materials.All(m => m.Grade == baseGrade))
            return new EquipmentResult { Success = false, Error = "GRADE_MISMATCH" };

        var nextGrade = GetNextGrade(baseGrade);
        if (nextGrade == null)
            return new EquipmentResult { Success = false, Error = "MAX_GRADE" };

        var mergedSubStats = MergeSubStats(materials);

        var totalRefundStones = materials.Sum(m => (double)m.EnhancementLevel);
        foreach (var mat in materials)
            await _equipmentRepo.DeleteAsync(mat.Id);

        if (totalRefundStones > 0)
            await _resourceService.GrantAsync(accountId, "EQUIPMENT_STONE", totalRefundStones, "FORGE_REFUND");

        var newEquipment = new EquipmentEntry
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            TemplateId = materials[0].TemplateId,
            Grade = nextGrade,
            EnhancementLevel = 0,
            SubStats = mergedSubStats,
            SlotIndex = -1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _equipmentRepo.CreateAsync(newEquipment);

        return new EquipmentResult { Success = true, Equipment = newEquipment };
    }

    public async Task<EquipmentResult> EquipAsync(Guid accountId, Guid equipmentId, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxEquipSlots)
            return new EquipmentResult { Success = false, Error = "INVALID_SLOT" };

        var equipment = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (equipment == null || equipment.AccountId != accountId)
            return new EquipmentResult { Success = false, Error = "EQUIPMENT_NOT_FOUND" };

        var equipped = await _equipmentRepo.GetEquippedAsync(accountId);
        var slotOccupant = equipped.FirstOrDefault(e => e.SlotIndex == slotIndex);
        if (slotOccupant != null)
        {
            slotOccupant.SlotIndex = -1;
            slotOccupant.UpdatedAt = DateTime.UtcNow;
            await _equipmentRepo.UpdateAsync(slotOccupant);
        }

        equipment.SlotIndex = slotIndex;
        equipment.UpdatedAt = DateTime.UtcNow;
        await _equipmentRepo.UpdateAsync(equipment);

        return new EquipmentResult { Success = true, Equipment = equipment };
    }

    public async Task<EquipmentResult> UnequipAsync(Guid accountId, Guid equipmentId)
    {
        var equipment = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (equipment == null || equipment.AccountId != accountId)
            return new EquipmentResult { Success = false, Error = "EQUIPMENT_NOT_FOUND" };

        equipment.SlotIndex = -1;
        equipment.UpdatedAt = DateTime.UtcNow;
        await _equipmentRepo.UpdateAsync(equipment);

        return new EquipmentResult { Success = true, Equipment = equipment };
    }

    public async Task<EquipmentResult> SellAsync(Guid accountId, Guid equipmentId)
    {
        var equipment = await _equipmentRepo.GetByIdAsync(equipmentId);
        if (equipment == null || equipment.AccountId != accountId)
            return new EquipmentResult { Success = false, Error = "EQUIPMENT_NOT_FOUND" };
        if (equipment.IsS)
            return new EquipmentResult { Success = false, Error = "CANNOT_SELL_S_GRADE" };
        if (equipment.SlotIndex >= 0)
            return new EquipmentResult { Success = false, Error = "CANNOT_SELL_EQUIPPED" };

        var price = SellPrices.GetValueOrDefault(equipment.Grade, 10);
        await _equipmentRepo.DeleteAsync(equipmentId);
        await _resourceService.GrantAsync(accountId, "GOLD", price, "EQUIPMENT_SELL", equipmentId.ToString());

        return new EquipmentResult { Success = true, SellPrice = price };
    }

    private static double CalculateEnhanceCost(int currentLevel, string grade)
    {
        var gradeMultiplier = grade switch
        {
            "COMMON" => 1.0, "UNCOMMON" => 1.15, "RARE" => 1.25,
            "EPIC" => 1.35, "LEGENDARY" => 1.45, "MYTHIC" => 1.55,
            _ => 1.0,
        };
        return Math.Floor(50 * (1 + currentLevel * 0.5) * gradeMultiplier);
    }

    private static string MergeSubStats(List<EquipmentEntry> materials)
    {
        var allStats = new List<string>();
        foreach (var mat in materials)
        {
            var stats = System.Text.Json.JsonSerializer.Deserialize<List<string>>(mat.SubStats);
            if (stats != null)
                allStats.AddRange(stats);
        }
        var maxSubStats = 5;
        var merged = allStats.Distinct().Take(maxSubStats).ToList();
        return System.Text.Json.JsonSerializer.Serialize(merged);
    }

    private static string? GetNextGrade(string grade) => grade switch
    {
        "COMMON" => "UNCOMMON",
        "UNCOMMON" => "RARE",
        "RARE" => "EPIC",
        "EPIC" => "LEGENDARY",
        "LEGENDARY" => "MYTHIC",
        _ => null,
    };
}

public class EquipmentResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public EquipmentEntry? Equipment { get; set; }
    public double SellPrice { get; set; }
}
