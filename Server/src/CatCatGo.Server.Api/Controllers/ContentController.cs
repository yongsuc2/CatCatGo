using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/content")]
[Authorize]
public class ContentController : ControllerBase
{
    private readonly ContentService _contentService;

    public ContentController(ContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpPost("tower/challenge")]
    public async Task<IActionResult> TowerChallenge()
    {
        var accountId = GetAccountId();
        var result = await _contentService.TowerChallengeAsync(accountId);
        return ToActionResult(result);
    }

    [HttpPost("dungeon/challenge")]
    public async Task<IActionResult> DungeonChallenge([FromBody] DungeonRequest request)
    {
        var accountId = GetAccountId();
        var result = await _contentService.DungeonChallengeAsync(accountId, request.DungeonType);
        return ToActionResult(result);
    }

    [HttpPost("dungeon/sweep")]
    public async Task<IActionResult> DungeonSweep([FromBody] DungeonRequest request)
    {
        var accountId = GetAccountId();
        var result = await _contentService.DungeonSweepAsync(accountId, request.DungeonType);
        return ToActionResult(result);
    }

    [HttpPost("goblin/mine")]
    public async Task<IActionResult> GoblinMine()
    {
        var accountId = GetAccountId();
        var result = await _contentService.GoblinMineAsync(accountId);
        return ToActionResult(result);
    }

    [HttpPost("goblin/cart")]
    public async Task<IActionResult> GoblinCart()
    {
        var accountId = GetAccountId();
        var result = await _contentService.GoblinCartAsync(accountId);
        return ToActionResult(result);
    }

    [HttpPost("catacomb/start")]
    public async Task<IActionResult> CatacombStart()
    {
        var accountId = GetAccountId();
        var result = await _contentService.CatacombStartAsync(accountId);
        return ToActionResult(result);
    }

    [HttpPost("catacomb/battle")]
    public async Task<IActionResult> CatacombBattle()
    {
        var accountId = GetAccountId();
        var result = await _contentService.CatacombBattleAsync(accountId);
        return ToActionResult(result);
    }

    [HttpPost("catacomb/end")]
    public async Task<IActionResult> CatacombEnd()
    {
        var accountId = GetAccountId();
        var result = await _contentService.CatacombEndAsync(accountId);
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

public class DungeonRequest
{
    public required string DungeonType { get; set; }
}
