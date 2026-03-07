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

    [HttpPost("consume")]
    public async Task<IActionResult> Consume([FromBody] ConsumeRequest request)
    {
        var accountId = GetAccountId();
        var response = await _shopService.ConsumeAsync(accountId, request.PurchaseId);
        return Ok(response);
    }

    [HttpGet("subscription")]
    public IActionResult GetSubscription()
    {
        var accountId = GetAccountId();
        var response = _shopService.GetSubscriptionStatus(accountId);
        return Ok(response);
    }

    [HttpPost("rtdn")]
    [AllowAnonymous]
    public async Task<IActionResult> Rtdn([FromBody] WebhookRequest request)
    {
        await _shopService.HandleRtdnAsync(request.Data);
        return Ok();
    }

    [HttpPost("s2s-notification")]
    [AllowAnonymous]
    public async Task<IActionResult> S2SNotification([FromBody] WebhookRequest request)
    {
        await _shopService.HandleS2SNotificationAsync(request.Data);
        return Ok();
    }

    private Guid GetAccountId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(claim!);
    }
}

public class ConsumeRequest
{
    public required string PurchaseId { get; set; }
}

public class WebhookRequest
{
    public required string Data { get; set; }
}
