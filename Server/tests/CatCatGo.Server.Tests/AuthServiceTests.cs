using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Requests;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class AuthServiceTests
{
    private readonly IAccountRepository _accountRepo;
    private readonly AuthService _sut;
    private const string JwtSecret = "test-secret-key-that-is-at-least-32-characters-long";

    public AuthServiceTests()
    {
        _accountRepo = Substitute.For<IAccountRepository>();
        _sut = new AuthService(_accountRepo, JwtSecret, accessTokenMinutes: 60, refreshTokenDays: 30);
    }

    [Fact]
    public async Task RegisterAsync_NewDevice_CreatesAccountAndReturnsTokens()
    {
        _accountRepo.GetByDeviceIdAsync("device-001").Returns((Account?)null);
        _accountRepo.CreateAsync(Arg.Any<Account>()).Returns(ci => ci.Arg<Account>());

        var result = await _sut.RegisterAsync(new RegisterRequest
        {
            DeviceId = "device-001",
            DisplayName = "TestPlayer"
        });

        Assert.True(result.IsNewAccount);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.NotEmpty(result.AccountId);
        await _accountRepo.Received(1).CreateAsync(Arg.Is<Account>(a => a.DeviceId == "device-001" && a.DisplayName == "TestPlayer"));
    }

    [Fact]
    public async Task RegisterAsync_ExistingDevice_ReturnsExistingAccountWithoutCreating()
    {
        var existing = new Account
        {
            Id = Guid.NewGuid(),
            DeviceId = "device-001",
            DisplayName = "ExistingPlayer",
            RefreshToken = "old-token",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(30),
        };
        _accountRepo.GetByDeviceIdAsync("device-001").Returns(existing);

        var result = await _sut.RegisterAsync(new RegisterRequest { DeviceId = "device-001" });

        Assert.False(result.IsNewAccount);
        Assert.Equal(existing.Id.ToString(), result.AccountId);
        await _accountRepo.DidNotReceive().CreateAsync(Arg.Any<Account>());
    }

    [Fact]
    public async Task LoginByDeviceAsync_ExistingAccount_ReturnsTokensAndUpdatesLastLogin()
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            DeviceId = "device-001",
            DisplayName = "TestPlayer",
            LastLoginAt = DateTime.UtcNow.AddDays(-1),
        };
        _accountRepo.GetByDeviceIdAsync("device-001").Returns(account);

        var result = await _sut.LoginByDeviceAsync("device-001");

        Assert.NotNull(result);
        Assert.NotEmpty(result!.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        await _accountRepo.Received(1).UpdateAsync(Arg.Is<Account>(a => a.RefreshToken != null));
    }

    [Fact]
    public async Task LoginByDeviceAsync_UnregisteredDevice_ReturnsNull()
    {
        _accountRepo.GetByDeviceIdAsync("unknown-device").Returns((Account?)null);

        var result = await _sut.LoginByDeviceAsync("unknown-device");

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginByDeviceAsync_BannedAccount_ReturnsNull()
    {
        var banned = new Account
        {
            Id = Guid.NewGuid(),
            DeviceId = "device-banned",
            IsBanned = true,
            BanReason = "Cheating",
        };
        _accountRepo.GetByDeviceIdAsync("device-banned").Returns(banned);

        var result = await _sut.LoginByDeviceAsync("device-banned");

        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_ValidJwtStructure_ContainsCorrectClaims()
    {
        _accountRepo.GetByDeviceIdAsync("device-jwt").Returns((Account?)null);
        _accountRepo.CreateAsync(Arg.Any<Account>()).Returns(ci => ci.Arg<Account>());

        var result = await _sut.RegisterAsync(new RegisterRequest
        {
            DeviceId = "device-jwt",
            DisplayName = "JwtTestPlayer"
        });

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.AccessToken);

        Assert.Equal("CatCatGo.Server", jwt.Issuer);
        Assert.Contains("CatCatGo.Client", jwt.Audiences);
        var nameIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        Assert.NotNull(nameIdClaim);
        Assert.Equal(result.AccountId, nameIdClaim!.Value);
    }

    [Fact]
    public async Task RegisterAsync_NoDisplayName_GeneratesPlayerPrefix()
    {
        _accountRepo.GetByDeviceIdAsync("device-noname").Returns((Account?)null);
        _accountRepo.CreateAsync(Arg.Any<Account>()).Returns(ci => ci.Arg<Account>());

        await _sut.RegisterAsync(new RegisterRequest { DeviceId = "device-noname" });

        await _accountRepo.Received(1).CreateAsync(
            Arg.Is<Account>(a => a.DisplayName != null && a.DisplayName.StartsWith("Player_")));
    }

    [Fact]
    public async Task RefreshAsync_NotImplemented_ReturnsNull()
    {
        var result = await _sut.RefreshAsync("some-refresh-token");

        Assert.Null(result);
    }
}
