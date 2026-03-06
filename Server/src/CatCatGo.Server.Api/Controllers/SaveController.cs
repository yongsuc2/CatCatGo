using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/save")]
[Authorize]
public class SaveController : ControllerBase
{
    private readonly SaveService _saveService;

    public SaveController(SaveService saveService)
    {
        _saveService = saveService;
    }

    [HttpGet]
    public async Task<ActionResult<SaveSyncResponse>> Load()
    {
        var accountId = GetAccountId();
        var response = await _saveService.LoadAsync(accountId);
        if (response == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost("sync")]
    public async Task<ActionResult<SaveSyncResponse>> Sync([FromBody] SaveSyncRequest request)
    {
        var accountId = GetAccountId();
        var response = await _saveService.SyncAsync(accountId, request);
        return Ok(response);
    }

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}
