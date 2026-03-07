using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/treasure")]
[Authorize]
public class TreasureController : ControllerBase
{
    private readonly TreasureService _treasureService;

    public TreasureController(TreasureService treasureService)
    {
        _treasureService = treasureService;
    }

    /// POST /api/treasure/claim
    [HttpPost("claim")]
    public async Task<IActionResult> Claim([FromBody] TreasureClaimRequest request)
    {
        var accountId = GetAccountId();
        var result = await _treasureService.ClaimAsync(accountId, request.MilestoneId);
        return Ok(result);
    }

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}

public class TreasureClaimRequest
{
    public required string MilestoneId { get; set; }
}
