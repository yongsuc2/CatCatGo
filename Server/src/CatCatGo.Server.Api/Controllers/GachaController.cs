using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/gacha")]
[Authorize]
public class GachaController : ControllerBase
{
    private readonly GachaService _gachaService;

    public GachaController(GachaService gachaService)
    {
        _gachaService = gachaService;
    }

    [HttpPost("pull")]
    public async Task<IActionResult> Pull([FromBody] GachaPullRequest request)
    {
        var accountId = GetAccountId();
        var result = await _gachaService.PullAsync(accountId, request.Count);
        return ToActionResult(result);
    }

    [HttpPost("pull10")]
    public async Task<IActionResult> Pull10()
    {
        var accountId = GetAccountId();
        var result = await _gachaService.Pull10Async(accountId);
        return ToActionResult(result);
    }

    [HttpGet("pity")]
    public async Task<IActionResult> GetPity()
    {
        var accountId = GetAccountId();
        var result = await _gachaService.GetPityAsync(accountId);
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

public class GachaPullRequest
{
    public string ChestType { get; set; } = "EQUIPMENT";
    public int Count { get; set; } = 1;
}
