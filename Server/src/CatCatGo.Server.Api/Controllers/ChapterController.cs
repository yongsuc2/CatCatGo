using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/chapter")]
[Authorize]
public class ChapterController : ControllerBase
{
    private readonly ChapterService _chapterService;

    public ChapterController(ChapterService chapterService)
    {
        _chapterService = chapterService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start()
    {
        var accountId = GetAccountId();
        var result = await _chapterService.StartAsync(accountId);
        return Ok(result);
    }

    [HttpPost("encounter")]
    public async Task<IActionResult> Encounter()
    {
        var accountId = GetAccountId();
        var result = await _chapterService.GenerateEncounterAsync(accountId);
        return Ok(result);
    }

    [HttpPost("encounter/resolve")]
    public async Task<IActionResult> ResolveEncounter([FromBody] EncounterResolveRequest request)
    {
        var accountId = GetAccountId();
        var result = await _chapterService.ResolveEncounterAsync(accountId, request.Choice);
        return Ok(result);
    }

    [HttpPost("skill/select")]
    public async Task<IActionResult> SelectSkill([FromBody] SkillSelectRequest request)
    {
        var accountId = GetAccountId();
        var result = await _chapterService.SelectSkillAsync(accountId, request.SkillIndex);
        return Ok(result);
    }

    [HttpPost("skill/reroll")]
    public async Task<IActionResult> RerollSkills()
    {
        var accountId = GetAccountId();
        var result = await _chapterService.RerollSkillsAsync(accountId);
        return Ok(result);
    }

    [HttpPost("abandon")]
    public async Task<IActionResult> Abandon()
    {
        var accountId = GetAccountId();
        var result = await _chapterService.AbandonAsync(accountId);
        return Ok(result);
    }

    [HttpGet("state")]
    public async Task<IActionResult> GetState()
    {
        var accountId = GetAccountId();
        var result = await _chapterService.GetStateAsync(accountId);
        return Ok(result);
    }

    [HttpPost("treasure/claim")]
    public async Task<IActionResult> ClaimTreasure([FromBody] TreasureClaimRequest request)
    {
        var accountId = GetAccountId();
        var result = await _chapterService.ClaimTreasureAsync(accountId, request.ChapterId, request.MilestoneKey);
        return Ok(result);
    }

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}

public class EncounterResolveRequest
{
    public required string Choice { get; set; }
}

public class SkillSelectRequest
{
    public int SkillIndex { get; set; }
}

public class TreasureClaimRequest
{
    public int ChapterId { get; set; }
    public required string MilestoneKey { get; set; }
}
