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

    /// POST /api/pet/hatch (was /upgrade for pet gacha)
    [HttpPost("hatch")]
    public async Task<IActionResult> Hatch()
    {
        var accountId = GetAccountId();
        var result = await _petService.HatchAsync(accountId);
        return Ok(result);
    }

    /// POST /api/pet/feed
    [HttpPost("feed")]
    public async Task<IActionResult> Feed([FromBody] PetFeedRequest request)
    {
        var accountId = GetAccountId();
        var result = await _petService.FeedAsync(accountId, request.PetId, request.Amount);
        return Ok(result);
    }

    /// POST /api/pet/deploy (was /equip)
    [HttpPost("deploy")]
    public async Task<IActionResult> Deploy([FromBody] PetIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _petService.DeployAsync(accountId, request.PetId);
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
    public required string PetId { get; set; }
    public int Amount { get; set; }
}

public class PetIdRequest
{
    public required string PetId { get; set; }
}
