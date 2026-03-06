using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/shop")]
[Authorize]
public class ShopController : ControllerBase
{
    private readonly ShopService _shopService;

    public ShopController(ShopService shopService)
    {
        _shopService = shopService;
    }

    [HttpGet("catalog")]
    public async Task<ActionResult<ShopCatalogResponse>> GetCatalog()
    {
        var response = await _shopService.GetCatalogAsync();
        return Ok(response);
    }

    [HttpPost("purchase")]
    public async Task<ActionResult<PurchaseResponse>> Purchase([FromBody] PurchaseRequest request)
    {
        var accountId = GetAccountId();
        var response = await _shopService.ProcessPurchaseAsync(accountId, request);
        return Ok(response);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var accountId = GetAccountId();
        var purchases = await _shopService.GetPurchaseHistoryAsync(accountId);
        return Ok(purchases);
    }

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}
