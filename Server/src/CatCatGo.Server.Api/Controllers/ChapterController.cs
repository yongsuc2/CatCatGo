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

    /// POST /api/chapter/start
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] ChapterStartRequest request)
    {
        var accountId = GetAccountId();
        var result = await _chapterService.StartAsync(accountId, request.ChapterId, request.ChapterType);
        return Ok(result);
    }

    /// POST /api/chapter/advance-day (was /encounter)
    [HttpPost("advance-day")]
    public async Task<IActionResult> AdvanceDay([FromBody] SessionRequest request)
    {
        var accountId = GetAccountId();
        var result = await _chapterService.AdvanceDayAsync(accountId, request.SessionId);
        return Ok(result);
    }

    /// POST /api/chapter/resolve-encounter (was /encounter/resolve)
    [HttpPost("resolve-encounter")]
    public async Task<IActionResult> ResolveEncounter([FromBody] ResolveEncounterRequest request)
    {
        var accountId = GetAccountId();
        var result = await _chapterService.ResolveEncounterAsync(accountId, request.SessionId, request.ChoiceIndex);
        return Ok(result);
    }

    /// POST /api/chapter/select-skill (was /skill/select)
    [HttpPost("select-skill")]
    public async Task<IActionResult> SelectSkill([FromBody] SelectSkillRequest request)
    {
        var accountId = GetAccountId();
        var result = await _chapterService.SelectSkillAsync(accountId, request.SessionId, request.SkillId);
        return Ok(result);
    }

    /// POST /api/chapter/reroll (was /skill/reroll)
    [HttpPost("reroll")]
    public async Task<IActionResult> Reroll([FromBody] SessionRequest request)
    {
        var accountId = GetAccountId();
        var result = await _chapterService.RerollAsync(accountId, request.SessionId);
        return Ok(result);
    }

    /// POST /api/chapter/battle-result
    [HttpPost("battle-result")]
    public async Task<IActionResult> BattleResult([FromBody] ChapterBattleResultRequest request)
    {
        var accountId = GetAccountId();
        var result = await _chapterService.BattleResultAsync(
            accountId, request.SessionId, request.BattleSeed,
            request.Result, request.TurnCount, request.PlayerRemainingHp);
        return Ok(result);
    }

    /// POST /api/chapter/abandon
    [HttpPost("abandon")]
    public async Task<IActionResult> Abandon([FromBody] SessionRequest request)
    {
        var accountId = GetAccountId();
        var result = await _chapterService.AbandonAsync(accountId, request.SessionId);
        return Ok(result);
    }

    [HttpGet("state")]
    public async Task<IActionResult> GetState()
    {
        var accountId = GetAccountId();
        var result = await _chapterService.GetStateAsync(accountId);
        return Ok(result);
    }

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}

public class ChapterStartRequest
{
    public int ChapterId { get; set; }
    public string ChapterType { get; set; } = "SIXTY_DAY";
}

public class SessionRequest
{
    public required string SessionId { get; set; }
}

public class ResolveEncounterRequest
{
    public required string SessionId { get; set; }
    public int ChoiceIndex { get; set; }
}

public class SelectSkillRequest
{
    public required string SessionId { get; set; }
    public required string SkillId { get; set; }
}

public class ChapterBattleResultRequest
{
    public required string SessionId { get; set; }
    public int BattleSeed { get; set; }
    public required string Result { get; set; }
    public int TurnCount { get; set; }
    public int PlayerRemainingHp { get; set; }
}
