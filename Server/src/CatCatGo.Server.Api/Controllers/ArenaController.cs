using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/arena")]
[Authorize]
public class ArenaController : ControllerBase
{
    private readonly ArenaService _arenaService;
    private readonly ResourceService _resourceService;

    public ArenaController(ArenaService arenaService, ResourceService resourceService)
    {
        _arenaService = arenaService;
        _resourceService = resourceService;
    }

    [HttpPost("match")]
    public async Task<ActionResult<ArenaMatchResponse>> Match()
    {
        var accountId = GetAccountId();
        var response = await _arenaService.MatchAsync(accountId);
        if (response == null)
            return BadRequest("Failed to find match");

        return Ok(response);
    }

    [HttpPost("result")]
    public async Task<IActionResult> SubmitResult([FromBody] ArenaResultRequest request)
    {
        var accountId = GetAccountId();
        await _arenaService.UpdateResultAsync(accountId, request.Rank);
        return Ok();
    }

    [HttpGet("ranking")]
    public async Task<ActionResult<ArenaRankingResponse>> GetRanking([FromQuery] int season = 1)
    {
        var accountId = GetAccountId();
        var response = await _arenaService.GetRankingsAsync(accountId, season);
        return Ok(response);
    }

    [HttpPost("defense")]
    public async Task<IActionResult> UpdateDefense([FromBody] ArenaDefenseRequest request)
    {
        var accountId = GetAccountId();
        await _arenaService.UpdateDefenseAsync(accountId, request.PlayerDataJson);
        return Ok();
    }

    [HttpGet("season")]
    public IActionResult GetSeason()
    {
        var info = _arenaService.GetSeasonInfo();
        return Ok(info);
    }

    [HttpPost("retry")]
    public async Task<IActionResult> Retry()
    {
        var accountId = GetAccountId();
        var result = await _arenaService.RetryAsync(accountId, _resourceService);
        return Ok(result);
    }

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}

public class ArenaDefenseRequest
{
    public required string PlayerDataJson { get; set; }
}

public class ArenaResultRequest
{
    public required string MatchId { get; set; }
    public int Rank { get; set; }
}
