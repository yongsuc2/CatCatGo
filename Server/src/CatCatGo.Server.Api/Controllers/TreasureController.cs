using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Models;
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

    [HttpPost("claim")]
    public async Task<IActionResult> Claim([FromBody] TreasureClaimRequest request)
    {
        var accountId = GetAccountId();
        var result = await _treasureService.ClaimAsync(accountId, request.MilestoneId);
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

public class TreasureClaimRequest
{
    public required string MilestoneId { get; set; }
}
