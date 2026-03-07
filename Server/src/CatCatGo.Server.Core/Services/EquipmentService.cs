using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Models;

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

    public async Task<ApiResponse<object>> UpgradeAsync(Guid accountId, string equipmentId)
    {
        if (!Guid.TryParse(equipmentId, out var eqGuid))
            return ApiResponse<object>.Fail("INVALID_EQUIPMENT_ID");

        var equipment = await _equipmentRepo.GetByIdAsync(eqGuid);
        if (equipment == null || equipment.AccountId != accountId)
            return ApiResponse<object>.Fail("EQUIPMENT_NOT_FOUND");

        var stoneCost = 1.0;
        var stoneSpent = await _resourceService.SpendAsync(accountId, "EQUIPMENT_STONE", stoneCost, "EQUIPMENT_UPGRADE", equipmentId);
        if (!stoneSpent)
            return ApiResponse<object>.Fail("INSUFFICIENT_EQUIPMENT_STONE");

        equipment.EnhancementLevel++;
        equipment.UpdatedAt = DateTime.UtcNow;
        await _equipmentRepo.UpdateAsync(equipment);

        var stoneBalance = await _resourceService.GetBalanceAsync(accountId, "EQUIPMENT_STONE");
        var delta = new StateDeltaBuilder()
            .AddResource("EQUIPMENT_STONE", (float)stoneBalance)
            .UpgradeEquipment(equipmentId, equipment.EnhancementLevel, 0)
            .Build();

        if (equipment.SlotIndex >= 0)
        {
            delta.EquipmentSlotChanges = new List<EquipSlotDelta>
            {
                new() { SlotType = equipment.TemplateId, Index = equipment.SlotIndex, EquipmentId = equipmentId, SlotLevel = equipment.EnhancementLevel }
            };
        }

        return ApiResponse<object>.Ok(delta: delta);
    }

    public async Task<ApiResponse<object>> EquipAsync(Guid accountId, string equipmentId)
    {
        if (!Guid.TryParse(equipmentId, out var eqGuid))
            return ApiResponse<object>.Fail("INVALID_EQUIPMENT_ID");

        var equipment = await _equipmentRepo.GetByIdAsync(eqGuid);
        if (equipment == null || equipment.AccountId != accountId)
            return ApiResponse<object>.Fail("EQUIPMENT_NOT_FOUND");

        var equipped = await _equipmentRepo.GetEquippedAsync(accountId);
        var slotIndex = FindAvailableSlot(equipped);
        if (slotIndex < 0)
            return ApiResponse<object>.Fail("NO_AVAILABLE_SLOT");

        equipment.SlotIndex = slotIndex;
        equipment.UpdatedAt = DateTime.UtcNow;
        await _equipmentRepo.UpdateAsync(equipment);

        var delta = new StateDeltaBuilder()
            .ChangeEquipSlot(equipment.TemplateId, slotIndex, equipmentId, equipment.EnhancementLevel)
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }

    public async Task<ApiResponse<object>> UnequipAsync(Guid accountId, string slotType, int slotIndex)
    {
        var equipped = await _equipmentRepo.GetEquippedAsync(accountId);
        var equipment = equipped.FirstOrDefault(e => e.SlotIndex == slotIndex);
        if (equipment == null)
            return ApiResponse<object>.Fail("SLOT_EMPTY");

        equipment.SlotIndex = -1;
        equipment.UpdatedAt = DateTime.UtcNow;
        await _equipmentRepo.UpdateAsync(equipment);

        var delta = new StateDeltaBuilder()
            .ChangeEquipSlot(slotType, slotIndex, null, 0)
            .AddEquipment(ToEquipmentDeltaData(equipment))
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }

    public async Task<ApiResponse<object>> SellAsync(Guid accountId, string equipmentId)
    {
        if (!Guid.TryParse(equipmentId, out var eqGuid))
            return ApiResponse<object>.Fail("INVALID_EQUIPMENT_ID");

        var equipment = await _equipmentRepo.GetByIdAsync(eqGuid);
        if (equipment == null || equipment.AccountId != accountId)
            return ApiResponse<object>.Fail("EQUIPMENT_NOT_FOUND");
        if (equipment.IsS)
            return ApiResponse<object>.Fail("CANNOT_SELL_S_GRADE");
        if (equipment.SlotIndex >= 0)
            return ApiResponse<object>.Fail("CANNOT_SELL_EQUIPPED");

        var price = SellPrices.GetValueOrDefault(equipment.Grade, 10);
        await _equipmentRepo.DeleteAsync(eqGuid);
        await _resourceService.GrantAsync(accountId, "GOLD", price, "EQUIPMENT_SELL", equipmentId);

        var goldBalance = await _resourceService.GetBalanceAsync(accountId, "GOLD");
        var delta = new StateDeltaBuilder()
            .AddResource("GOLD", (float)goldBalance)
            .RemoveEquipment(equipmentId)
            .Build();

        return ApiResponse<object>.Ok(delta: delta);
    }

    public async Task<ApiResponse<object>> ForgeAsync(Guid accountId, List<string> equipmentIds)
    {
        if (equipmentIds.Count < 3)
            return ApiResponse<object>.Fail("INSUFFICIENT_MATERIALS");

        var materialGuids = new List<Guid>();
        foreach (var id in equipmentIds)
        {
            if (!Guid.TryParse(id, out var g))
                return ApiResponse<object>.Fail("INVALID_EQUIPMENT_ID");
            materialGuids.Add(g);
        }

        var materials = new List<EquipmentEntry>();
        foreach (var id in materialGuids)
        {
            var mat = await _equipmentRepo.GetByIdAsync(id);
            if (mat == null || mat.AccountId != accountId)
                return ApiResponse<object>.Fail("MATERIAL_NOT_FOUND");
            if (mat.SlotIndex >= 0)
                return ApiResponse<object>.Fail("MATERIAL_IS_EQUIPPED");
            if (mat.IsS)
                return ApiResponse<object>.Fail("CANNOT_FORGE_S_GRADE");
            materials.Add(mat);
        }

        var baseGrade = materials[0].Grade;
        if (!materials.All(m => m.Grade == baseGrade))
            return ApiResponse<object>.Fail("GRADE_MISMATCH");

        var nextGrade = GetNextGrade(baseGrade);
        if (nextGrade == null)
            return ApiResponse<object>.Fail("MAX_GRADE");

        var mergedSubStats = MergeSubStats(materials);
        var deltaBuilder = new StateDeltaBuilder();

        var totalRefundStones = materials.Sum(m => (double)m.EnhancementLevel);
        foreach (var mat in materials)
        {
            await _equipmentRepo.DeleteAsync(mat.Id);
            deltaBuilder.RemoveEquipment(mat.Id.ToString());
        }

        if (totalRefundStones > 0)
            await _resourceService.GrantAsync(accountId, "EQUIPMENT_STONE", totalRefundStones, "FORGE_REFUND");

        var newEquipment = new EquipmentEntry
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            TemplateId = materials[0].TemplateId,
            Grade = nextGrade,
            EnhancementLevel = 0,
            Slot = materials[0].Slot,
            WeaponSubType = materials[0].WeaponSubType,
            SubStats = mergedSubStats,
            SlotIndex = -1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _equipmentRepo.CreateAsync(newEquipment);
        deltaBuilder.AddEquipment(ToEquipmentDeltaData(newEquipment));

        if (totalRefundStones > 0)
        {
            var stoneBalance = await _resourceService.GetBalanceAsync(accountId, "EQUIPMENT_STONE");
            deltaBuilder.AddResource("EQUIPMENT_STONE", (float)stoneBalance);
        }

        return ApiResponse<object>.Ok(delta: deltaBuilder.Build());
    }

    public async Task<ApiResponse<BulkForgeResponse>> BulkForgeAsync(Guid accountId)
    {
        var allEquipment = await _equipmentRepo.GetByAccountIdAsync(accountId);
        var inventory = allEquipment.Where(e => e.SlotIndex < 0 && !e.IsS).ToList();

        var deltaBuilder = new StateDeltaBuilder();
        var mergedCount = 0;

        var groups = inventory.GroupBy(e => new { e.Grade, e.TemplateId });
        foreach (var group in groups)
        {
            var items = group.ToList();
            while (items.Count >= 3)
            {
                var batch = items.Take(3).ToList();
                items = items.Skip(3).ToList();

                var nextGrade = GetNextGrade(batch[0].Grade);
                if (nextGrade == null) break;

                var mergedSubStats = MergeSubStats(batch);
                var totalRefundStones = batch.Sum(m => (double)m.EnhancementLevel);

                foreach (var mat in batch)
                {
                    await _equipmentRepo.DeleteAsync(mat.Id);
                    deltaBuilder.RemoveEquipment(mat.Id.ToString());
                }

                if (totalRefundStones > 0)
                    await _resourceService.GrantAsync(accountId, "EQUIPMENT_STONE", totalRefundStones, "FORGE_REFUND");

                var newEquipment = new EquipmentEntry
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    TemplateId = batch[0].TemplateId,
                    Grade = nextGrade,
                    EnhancementLevel = 0,
                    Slot = batch[0].Slot,
                    WeaponSubType = batch[0].WeaponSubType,
                    SubStats = mergedSubStats,
                    SlotIndex = -1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                await _equipmentRepo.CreateAsync(newEquipment);
                deltaBuilder.AddEquipment(ToEquipmentDeltaData(newEquipment));
                mergedCount++;
            }
        }

        if (mergedCount > 0)
        {
            var stoneBalance = await _resourceService.GetBalanceAsync(accountId, "EQUIPMENT_STONE");
            deltaBuilder.AddResource("EQUIPMENT_STONE", (float)stoneBalance);
        }

        return ApiResponse<BulkForgeResponse>.Ok(
            new BulkForgeResponse { MergedCount = mergedCount },
            deltaBuilder.Build());
    }

    private int FindAvailableSlot(List<EquipmentEntry> equipped)
    {
        var usedSlots = equipped.Select(e => e.SlotIndex).ToHashSet();
        for (int i = 0; i < MaxEquipSlots; i++)
        {
            if (!usedSlots.Contains(i)) return i;
        }
        return -1;
    }

    private static EquipmentDeltaData ToEquipmentDeltaData(EquipmentEntry entry)
    {
        var subStats = new List<SubStatDeltaData>();
        try
        {
            var parsed = System.Text.Json.JsonSerializer.Deserialize<List<string>>(entry.SubStats);
            if (parsed != null)
                subStats = parsed.Select(s => new SubStatDeltaData { Stat = s, Value = 0 }).ToList();
        }
        catch { }

        return new EquipmentDeltaData
        {
            Id = entry.Id.ToString(),
            Name = entry.TemplateId,
            Slot = entry.Slot,
            Grade = entry.Grade,
            IsS = entry.IsS,
            Level = entry.EnhancementLevel,
            PromoteCount = 0,
            MergeLevel = 0,
            WeaponSubType = entry.WeaponSubType,
            SubStats = subStats,
        };
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

public class BulkForgeResponse
{
    public int MergedCount { get; set; }
}
