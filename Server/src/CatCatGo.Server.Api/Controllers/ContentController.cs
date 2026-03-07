using System.Security.Claims;
using CatCatGo.Server.Core.Services;
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

    /// POST /api/content/tower/challenge
    [HttpPost("tower/challenge")]
    public async Task<IActionResult> TowerChallenge()
    {
        var accountId = GetAccountId();
        var result = await _contentService.TowerChallengeAsync(accountId);
        return Ok(result);
    }

    /// POST /api/content/dungeon/challenge (was dungeon/enter + dungeon/result)
    [HttpPost("dungeon/challenge")]
    public async Task<IActionResult> DungeonChallenge([FromBody] DungeonRequest request)
    {
        var accountId = GetAccountId();
        var result = await _contentService.DungeonChallengeAsync(accountId, request.DungeonType);
        return Ok(result);
    }

    /// POST /api/content/dungeon/sweep (new)
    [HttpPost("dungeon/sweep")]
    public async Task<IActionResult> DungeonSweep([FromBody] DungeonRequest request)
    {
        var accountId = GetAccountId();
        var result = await _contentService.DungeonSweepAsync(accountId, request.DungeonType);
        return Ok(result);
    }

    /// POST /api/content/goblin/mine
    [HttpPost("goblin/mine")]
    public async Task<IActionResult> GoblinMine()
    {
        var accountId = GetAccountId();
        var result = await _contentService.GoblinMineAsync(accountId);
        return Ok(result);
    }

    /// POST /api/content/goblin/cart (new)
    [HttpPost("goblin/cart")]
    public async Task<IActionResult> GoblinCart()
    {
        var accountId = GetAccountId();
        var result = await _contentService.GoblinCartAsync(accountId);
        return Ok(result);
    }

    /// POST /api/content/catacomb/start (was catacomb/run)
    [HttpPost("catacomb/start")]
    public async Task<IActionResult> CatacombStart()
    {
        var accountId = GetAccountId();
        var result = await _contentService.CatacombStartAsync(accountId);
        return Ok(result);
    }

    /// POST /api/content/catacomb/battle (new)
    [HttpPost("catacomb/battle")]
    public async Task<IActionResult> CatacombBattle()
    {
        var accountId = GetAccountId();
        var result = await _contentService.CatacombBattleAsync(accountId);
        return Ok(result);
    }

    /// POST /api/content/catacomb/end (new)
    [HttpPost("catacomb/end")]
    public async Task<IActionResult> CatacombEnd()
    {
        var accountId = GetAccountId();
        var result = await _contentService.CatacombEndAsync(accountId);
        return Ok(result);
    }

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
