using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/battle")]
[Authorize]
public class BattleController : ControllerBase
{
    private readonly BattleVerifier _battleVerifier;

    public BattleController(BattleVerifier battleVerifier)
    {
        _battleVerifier = battleVerifier;
    }

    [HttpPost("start")]
    public ActionResult<BattleStartResponse> Start([FromBody] BattleStartRequest request)
    {
        var accountId = GetAccountId();
        var response = _battleVerifier.StartBattle(accountId, request);
        return Ok(response);
    }

    [HttpPost("report")]
    public async Task<ActionResult<BattleReportResponse>> Report([FromBody] BattleReportRequest request)
    {
        var accountId = GetAccountId();
        var response = await _battleVerifier.VerifyReportAsync(accountId, request);
        return Ok(response);
    }

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}
