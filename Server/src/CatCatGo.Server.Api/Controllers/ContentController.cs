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

    [HttpPost("tower/challenge")]
    public async Task<IActionResult> TowerChallenge()
    {
        var accountId = GetAccountId();
        var result = await _contentService.TowerChallengeAsync(accountId);
        return Ok(result);
    }

    [HttpPost("dungeon/enter")]
    public async Task<IActionResult> DungeonEnter([FromBody] DungeonRequest request)
    {
        var accountId = GetAccountId();
        var result = await _contentService.DungeonEnterAsync(accountId, request.DungeonType);
        return Ok(result);
    }

    [HttpPost("dungeon/result")]
    public async Task<IActionResult> DungeonResult([FromBody] DungeonResultRequest request)
    {
        var accountId = GetAccountId();
        var result = await _contentService.DungeonResultAsync(accountId, request.DungeonType, request.Victory);
        return Ok(result);
    }

    [HttpPost("travel/start")]
    public async Task<IActionResult> TravelStart([FromBody] TravelStartRequest request)
    {
        var accountId = GetAccountId();
        var result = await _contentService.TravelStartAsync(accountId, request.StaminaCost);
        return Ok(result);
    }

    [HttpPost("travel/complete")]
    public async Task<IActionResult> TravelComplete([FromBody] TravelCompleteRequest request)
    {
        var accountId = GetAccountId();
        var result = await _contentService.TravelCompleteAsync(accountId, request.ClearedChapterMax, request.SpeedMultiplier);
        return Ok(result);
    }

    [HttpPost("goblin/mine")]
    public async Task<IActionResult> GoblinMine()
    {
        var accountId = GetAccountId();
        var result = await _contentService.GoblinMineAsync(accountId);
        return Ok(result);
    }

    [HttpPost("catacomb/run")]
    public async Task<IActionResult> CatacombRun()
    {
        var accountId = GetAccountId();
        var result = await _contentService.CatacombRunAsync(accountId);
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

public class DungeonResultRequest
{
    public required string DungeonType { get; set; }
    public bool Victory { get; set; }
}

public class TravelStartRequest
{
    public double StaminaCost { get; set; }
}

public class TravelCompleteRequest
{
    public int ClearedChapterMax { get; set; }
    public double SpeedMultiplier { get; set; } = 1;
}
