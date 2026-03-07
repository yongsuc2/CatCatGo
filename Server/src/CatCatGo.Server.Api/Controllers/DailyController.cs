using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/daily")]
[Authorize]
public class DailyController : ControllerBase
{
    private readonly DailyService _dailyService;

    public DailyController(DailyService dailyService)
    {
        _dailyService = dailyService;
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendance()
    {
        var accountId = GetAccountId();
        var result = await _dailyService.GetAttendanceAsync(accountId);
        return Ok(result);
    }

    [HttpPost("attendance/claim")]
    public async Task<IActionResult> ClaimAttendance()
    {
        var accountId = GetAccountId();
        var result = await _dailyService.ClaimAttendanceAsync(accountId);
        return ToActionResult(result);
    }

    [HttpGet("quest")]
    public async Task<IActionResult> GetQuests()
    {
        var accountId = GetAccountId();
        var result = await _dailyService.GetQuestsAsync(accountId);
        return Ok(result);
    }

    [HttpPost("quest/claim")]
    public async Task<IActionResult> ClaimQuest([FromBody] QuestClaimRequest request)
    {
        var accountId = GetAccountId();
        var result = await _dailyService.ClaimQuestAsync(accountId, request.EventId, request.MissionId);
        return ToActionResult(result);
    }

    [HttpPost("quest/claim-all")]
    public async Task<IActionResult> ClaimAllQuests([FromBody] QuestClaimAllRequest request)
    {
        var accountId = GetAccountId();
        var result = await _dailyService.ClaimAllQuestsAsync(accountId, request.EventId);
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

public class QuestClaimRequest
{
    public required string EventId { get; set; }
    public required string MissionId { get; set; }
}

public class QuestClaimAllRequest
{
    public required string EventId { get; set; }
}
