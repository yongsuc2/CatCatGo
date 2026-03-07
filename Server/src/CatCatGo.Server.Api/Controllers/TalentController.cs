using System.Security.Claims;
using CatCatGo.Server.Core.Services;
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

    /// POST /api/talent/upgrade
    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] TalentUpgradeRequest request)
    {
        var accountId = GetAccountId();
        var result = await _talentService.UpgradeAsync(accountId, request.StatType);
        return Ok(result);
    }

    /// POST /api/talent/claim-milestone
    [HttpPost("claim-milestone")]
    public async Task<IActionResult> ClaimMilestone([FromBody] TalentMilestoneRequest request)
    {
        var accountId = GetAccountId();
        var result = await _talentService.ClaimMilestoneAsync(accountId, request.MilestoneLevel);
        return Ok(result);
    }

    /// POST /api/talent/claim-all-milestones
    [HttpPost("claim-all-milestones")]
    public async Task<IActionResult> ClaimAllMilestones()
    {
        var accountId = GetAccountId();
        var result = await _talentService.ClaimAllMilestonesAsync(accountId);
        return Ok(result);
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var accountId = GetAccountId();
        var state = await _talentService.GetStatusAsync(accountId);
        return Ok(state);
    }

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
