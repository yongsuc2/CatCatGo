using System.Security.Claims;
using CatCatGo.Server.Core.Services;
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
    public async Task<IActionResult> Pull()
    {
        var accountId = GetAccountId();
        var result = await _gachaService.PullAsync(accountId);
        return Ok(result);
    }

    [HttpPost("pull10")]
    public async Task<IActionResult> Pull10()
    {
        var accountId = GetAccountId();
        var result = await _gachaService.Pull10Async(accountId);
        return Ok(result);
    }

    [HttpGet("pity")]
    public async Task<IActionResult> GetPity()
    {
        var accountId = GetAccountId();
        var result = await _gachaService.GetPityAsync(accountId);
        return Ok(result);
    }

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}
