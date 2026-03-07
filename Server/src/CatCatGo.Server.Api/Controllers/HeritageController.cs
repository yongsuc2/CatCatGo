using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/heritage")]
[Authorize]
public class HeritageController : ControllerBase
{
    private readonly HeritageService _heritageService;

    public HeritageController(HeritageService heritageService)
    {
        _heritageService = heritageService;
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade([FromBody] HeritageUpgradeRequest request)
    {
        var accountId = GetAccountId();
        var result = await _heritageService.UpgradeAsync(accountId, request.Route);
        return ToActionResult(result);
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var accountId = GetAccountId();
        var result = await _heritageService.GetStatusAsync(accountId);
        return Ok(result);
    }

    private IActionResult ToActionResult<T>(ApiResponse<T> result) =>
        result.Success ? Ok(result) : BadRequest(result);

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}

public class HeritageUpgradeRequest
{
    public required string Route { get; set; }
}
