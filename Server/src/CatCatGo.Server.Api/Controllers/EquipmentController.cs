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

    [HttpPost("enhance")]
    public async Task<IActionResult> Enhance([FromBody] EquipmentIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.EnhanceAsync(accountId, request.EquipmentId);
        return Ok(result);
    }

    [HttpPost("forge")]
    public async Task<IActionResult> Forge([FromBody] ForgeRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.ForgeAsync(accountId, request.MaterialIds);
        return Ok(result);
    }

    [HttpPost("equip")]
    public async Task<IActionResult> Equip([FromBody] EquipSlotRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.EquipAsync(accountId, request.EquipmentId, request.SlotIndex);
        return Ok(result);
    }

    [HttpPost("unequip")]
    public async Task<IActionResult> Unequip([FromBody] EquipmentIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.UnequipAsync(accountId, request.EquipmentId);
        return Ok(result);
    }

    [HttpPost("sell")]
    public async Task<IActionResult> Sell([FromBody] EquipmentIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _equipmentService.SellAsync(accountId, request.EquipmentId);
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
    public Guid EquipmentId { get; set; }
}

public class ForgeRequest
{
    public required List<Guid> MaterialIds { get; set; }
}

public class EquipSlotRequest
{
    public Guid EquipmentId { get; set; }
    public int SlotIndex { get; set; }
}
