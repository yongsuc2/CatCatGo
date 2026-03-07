using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/resource")]
[Authorize]
public class ResourceController : ControllerBase
{
    private readonly ResourceService _resourceService;

    public ResourceController(ResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    [HttpGet("balance")]
    public async Task<ActionResult<ResourceBalanceResponse>> GetBalance()
    {
        var accountId = GetAccountId();
        var balances = await _resourceService.GetAllBalancesAsync(accountId);
        return Ok(new ResourceBalanceResponse { Balances = balances });
    }

    [HttpPost("spend")]
    public async Task<ActionResult<ResourceSpendResponse>> Spend([FromBody] ResourceSpendRequest request)
    {
        var accountId = GetAccountId();
        var success = await _resourceService.SpendAsync(accountId, request.Type, request.Amount, request.Source, request.RefId);

        if (!success)
        {
            var current = await _resourceService.GetBalanceAsync(accountId, request.Type);
            return Ok(new ResourceSpendResponse { Success = false, Error = "INSUFFICIENT_BALANCE", RemainingBalance = current });
        }

        var remaining = await _resourceService.GetBalanceAsync(accountId, request.Type);
        return Ok(new ResourceSpendResponse { Success = true, RemainingBalance = remaining });
    }

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}
