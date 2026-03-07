using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/talent")]
[Authorize]
public class TalentController : ControllerBase
{
    private readonly TalentService _talentService;

    public TalentController(TalentService talentService)
    {
        _talentService = talentService;
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] TalentUpgradeRequest request)
    {
        var accountId = GetAccountId();
        var result = await _talentService.UpgradeAsync(accountId, request.StatType);
        return ToActionResult(result);
    }

    [HttpPost("claim-milestone")]
    public async Task<IActionResult> ClaimMilestone([FromBody] TalentMilestoneRequest request)
    {
        var accountId = GetAccountId();
        var result = await _talentService.ClaimMilestoneAsync(accountId, request.MilestoneLevel);
        return ToActionResult(result);
    }

    [HttpPost("claim-all-milestones")]
    public async Task<IActionResult> ClaimAllMilestones()
    {
        var accountId = GetAccountId();
        var result = await _talentService.ClaimAllMilestonesAsync(accountId);
        return ToActionResult(result);
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var accountId = GetAccountId();
        var state = await _talentService.GetStatusAsync(accountId);
        return Ok(state);
    }

    private IActionResult ToActionResult<T>(ApiResponse<T> result) =>
        result.Success ? Ok(result) : BadRequest(result);

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}

public class TalentUpgradeRequest
{
    public required string StatType { get; set; }
}

public class TalentMilestoneRequest
{
    public int MilestoneLevel { get; set; }
}
