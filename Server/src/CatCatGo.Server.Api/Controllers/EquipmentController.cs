using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/equipment")]
[Authorize]
public class EquipmentController : ControllerBase
{
    private readonly EquipmentService _equipmentService;

    public EquipmentController(EquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    /// POST /api/equipment/upgrade (was /enhance)
    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] EquipmentIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.UpgradeAsync(accountId, request.EquipmentId);
        return Ok(result);
    }

    /// POST /api/equipment/equip
    [HttpPost("equip")]
    public async Task<IActionResult> Equip([FromBody] EquipmentIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.EquipAsync(accountId, request.EquipmentId);
        return Ok(result);
    }

    /// POST /api/equipment/unequip
    [HttpPost("unequip")]
    public async Task<IActionResult> Unequip([FromBody] UnequipRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.UnequipAsync(accountId, request.SlotType, request.SlotIndex);
        return Ok(result);
    }

    /// POST /api/equipment/sell
    [HttpPost("sell")]
    public async Task<IActionResult> Sell([FromBody] EquipmentIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.SellAsync(accountId, request.EquipmentId);
        return Ok(result);
    }

    /// POST /api/equipment/forge
    [HttpPost("forge")]
    public async Task<IActionResult> Forge([FromBody] ForgeRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.ForgeAsync(accountId, request.EquipmentIds);
        return Ok(result);
    }

    /// POST /api/equipment/bulk-forge
    [HttpPost("bulk-forge")]
    public async Task<IActionResult> BulkForge()
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.BulkForgeAsync(accountId);
        return Ok(result);
    }

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}

public class EquipmentIdRequest
{
    public required string EquipmentId { get; set; }
}

public class UnequipRequest
{
    public required string SlotType { get; set; }
    public int SlotIndex { get; set; }
}

public class ForgeRequest
{
    public required List<string> EquipmentIds { get; set; }
}
