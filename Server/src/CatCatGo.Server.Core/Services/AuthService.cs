using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;
using Microsoft.IdentityModel.Tokens;

namespace CatCatGo.Server.Core.Services;

public class AuthService
{
    private readonly IAccountRepository _accountRepo;
    private readonly string _jwtSecret;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenDays;

    public AuthService(IAccountRepository accountRepo, string jwtSecret,
        int accessTokenMinutes = 60, int refreshTokenDays = 30)
    {
        _accountRepo = accountRepo;
        _jwtSecret = jwtSecret;
        _accessTokenMinutes = accessTokenMinutes;
        _refreshTokenDays = refreshTokenDays;
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _accountRepo.GetByDeviceIdAsync(request.DeviceId);
        if (existing != null)
            return await GenerateTokenResponse(existing, false);

        var account = new Account
        {
            Id = Guid.NewGuid(),
            DeviceId = request.DeviceId,
            DisplayName = request.DisplayName ?? $"Player_{Guid.NewGuid().ToString()[..6]}",
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
        };

        var refreshToken = GenerateRefreshToken();
        account.RefreshToken = refreshToken;
        account.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenDays);

        await _accountRepo.CreateAsync(account);
        return await GenerateTokenResponse(account, true);
    }

    public async Task<LoginResponse?> LoginByDeviceAsync(string deviceId)
    {
        var account = await _accountRepo.GetByDeviceIdAsync(deviceId);
        if (account == null || account.IsBanned) return null;

        account.LastLoginAt = DateTime.UtcNow;
        account.RefreshToken = GenerateRefreshToken();
        account.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenDays);
        await _accountRepo.UpdateAsync(account);

        return await GenerateTokenResponse(account, false);
    }

    public async Task<LoginResponse?> RefreshAsync(string refreshToken)
    {
        var account = await _accountRepo.GetByRefreshTokenAsync(refreshToken);
        if (account == null || account.IsBanned)
            return null;

        if (account.RefreshTokenExpiry == null || account.RefreshTokenExpiry < DateTime.UtcNow)
            return null;

        account.LastLoginAt = DateTime.UtcNow;
        account.RefreshToken = GenerateRefreshToken();
        account.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenDays);
        await _accountRepo.UpdateAsync(account);

        return await GenerateTokenResponse(account, false);
    }

    private Task<LoginResponse> GenerateTokenResponse(Account account, bool isNew)
    {
        var accessToken = GenerateAccessToken(account);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_accessTokenMinutes).ToUnixTimeSeconds();

        return Task.FromResult(new LoginResponse
        {
            AccountId = account.Id.ToString(),
            AccessToken = accessToken,
            RefreshToken = account.RefreshToken!,
            ExpiresAt = expiresAt,
            IsNewAccount = isNew,
        });
    }

    private string GenerateAccessToken(Account account)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Name, account.DisplayName ?? ""),
        };

        var token = new JwtSecurityToken(
            issuer: "CatCatGo.Server",
            audience: "CatCatGo.Client",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> ResetDataAsync(Guid accountId)
    {
        var account = await _accountRepo.GetByIdAsync(accountId);
        if (account == null) return false;

        await _accountRepo.DeleteAllDataAsync(accountId);
        return true;
    }

    public async Task<bool> DeleteAccountAsync(Guid accountId)
    {
        var account = await _accountRepo.GetByIdAsync(accountId);
        if (account == null) return false;

        await _accountRepo.DeleteAsync(accountId);
        return true;
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
