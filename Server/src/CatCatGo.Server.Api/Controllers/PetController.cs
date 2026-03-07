using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/pet")]
[Authorize]
public class PetController : ControllerBase
{
    private readonly PetService _petService;

    public PetController(PetService petService)
    {
        _petService = petService;
    }

    [HttpPost("feed")]
    public async Task<IActionResult> Feed([FromBody] PetFeedRequest request)
    {
        var accountId = GetAccountId();
        var result = await _petService.FeedAsync(accountId, request.PetId, request.Amount);
        return Ok(result);
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] PetIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _petService.UpgradeAsync(accountId, request.PetId);
        return Ok(result);
    }

    [HttpPost("equip")]
    public async Task<IActionResult> Equip([FromBody] PetIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _petService.EquipAsync(accountId, request.PetId);
        return Ok(result);
    }

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}

public class PetFeedRequest
{
    public Guid PetId { get; set; }
    public int Amount { get; set; }
}

public class PetIdRequest
{
    public Guid PetId { get; set; }
}
