using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/pet")]
[Authorize]
public class PetController : ControllerBase
{
    private readonly PetService _petService;

    public PetController(PetService petService)
    {
        _petService = petService;
    }

    [HttpPost("hatch")]
    public async Task<IActionResult> Hatch()
    {
        var accountId = GetAccountId();
        var result = await _petService.HatchAsync(accountId);
        return ToActionResult(result);
    }

    [HttpPost("feed")]
    public async Task<IActionResult> Feed([FromBody] PetFeedRequest request)
    {
        var accountId = GetAccountId();
        var result = await _petService.FeedAsync(accountId, request.PetId, request.Amount);
        return ToActionResult(result);
    }

    [HttpPost("deploy")]
    public async Task<IActionResult> Deploy([FromBody] PetIdRequest request)
    {
        var accountId = GetAccountId();
        var result = await _petService.DeployAsync(accountId, request.PetId);
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

public class PetFeedRequest
{
    public required string PetId { get; set; }
    public int Amount { get; set; }
}

public class PetIdRequest
{
    public required string PetId { get; set; }
}
