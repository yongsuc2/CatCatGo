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

    public ArenaController(ArenaService arenaService)
    {
        _arenaService = arenaService;
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

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}

public class ArenaResultRequest
{
    public required string MatchId { get; set; }
    public int Rank { get; set; }
}
