using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Models;
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

    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] EquipmentIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.UpgradeAsync(accountId, request.EquipmentId);
        return ToActionResult(result);
    }

    [HttpPost("equip")]
    public async Task<IActionResult> Equip([FromBody] EquipmentIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.EquipAsync(accountId, request.EquipmentId);
        return ToActionResult(result);
    }

    [HttpPost("unequip")]
    public async Task<IActionResult> Unequip([FromBody] UnequipRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.UnequipAsync(accountId, request.SlotType, request.SlotIndex);
        return ToActionResult(result);
    }

    [HttpPost("sell")]
    public async Task<IActionResult> Sell([FromBody] EquipmentIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.SellAsync(accountId, request.EquipmentId);
        return ToActionResult(result);
    }

    [HttpPost("forge")]
    public async Task<IActionResult> Forge([FromBody] ForgeRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.ForgeAsync(accountId, request.EquipmentIds);
        return ToActionResult(result);
    }

    [HttpPost("bulk-forge")]
    public async Task<IActionResult> BulkForge()
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.BulkForgeAsync(accountId);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(ApiResponse<T> result) =>
        result.Success ? Ok(result) : BadRequest(result);

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
