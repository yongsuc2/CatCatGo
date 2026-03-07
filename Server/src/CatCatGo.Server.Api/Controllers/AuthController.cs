using System.Security.Claims;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatCatGo.Server.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("[Auth] Register DeviceId={DeviceId}, DisplayName={DisplayName}", request.DeviceId, request.DisplayName);
        var response = await _authService.RegisterAsync(request);
        _logger.LogInformation("[Auth] Register OK AccountId={AccountId}, IsNew={IsNew}", response.AccountId, response.IsNewAccount);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("[Auth] Login DeviceId={DeviceId}", request.DeviceId);
        if (string.IsNullOrEmpty(request.DeviceId))
            return BadRequest("DeviceId is required");

        var response = await _authService.LoginByDeviceAsync(request.DeviceId);
        if (response == null)
        {
            _logger.LogWarning("[Auth] Login FAILED DeviceId={DeviceId} (not found or banned)", request.DeviceId);
            return Unauthorized();
        }

        _logger.LogInformation("[Auth] Login OK AccountId={AccountId}", response.AccountId);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshAsync(request.RefreshToken);
        if (response == null)
        {
            _logger.LogWarning("[Auth] Refresh FAILED (invalid token)");
            return Unauthorized();
        }

        _logger.LogInformation("[Auth] Refresh OK AccountId={AccountId}", response.AccountId);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("reset-data")]
    public async Task<IActionResult> ResetData()
    {
        var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        _logger.LogInformation("[Auth] ResetData AccountId={AccountId}", accountId);

        var result = await _authService.ResetDataAsync(accountId);
        if (!result) return NotFound();

        _logger.LogInformation("[Auth] ResetData OK AccountId={AccountId}", accountId);
        return Ok(new { Message = "All game data has been reset." });
    }

    [Authorize]
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        _logger.LogInformation("[Auth] DeleteAccount AccountId={AccountId}", accountId);

        var result = await _authService.DeleteAccountAsync(accountId);
        if (!result) return NotFound();

        _logger.LogInformation("[Auth] DeleteAccount OK AccountId={AccountId}", accountId);
        return Ok(new { Message = "Account has been deleted." });
    }
}
