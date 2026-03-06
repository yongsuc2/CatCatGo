using System.Security.Cryptography;
using System.Text;
using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Server.Core.Services;
using CatCatGo.Shared.Requests;
using NSubstitute;
using Xunit;

namespace CatCatGo.Server.Tests;

public class SaveServiceTests
{
    private readonly ISaveRepository _saveRepo;
    private readonly SaveService _sut;

    public SaveServiceTests()
    {
        _saveRepo = Substitute.For<ISaveRepository>();
        _sut = new SaveService(_saveRepo);
    }

    [Fact]
    public async Task SyncAsync_NoServerSave_CreatesNewAndReturnsAccepted()
    {
        var accountId = Guid.NewGuid();
        _saveRepo.GetByAccountIdAsync(accountId).Returns((ServerSaveData?)null);

        var result = await _sut.SyncAsync(accountId, new SaveSyncRequest
        {
            Data = "{\"gold\":100}",
            ClientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Version = 1,
            Checksum = "abc"
        });

        Assert.Equal("ACCEPTED", result.Action);
        Assert.Equal(1, result.Version);
        await _saveRepo.Received(1).UpsertAsync(Arg.Is<ServerSaveData>(s =>
            s.AccountId == accountId && s.Data == "{\"gold\":100}"));
    }

    [Fact]
    public async Task SyncAsync_ClientNewer_UpdatesServerAndReturnsAccepted()
    {
        var accountId = Guid.NewGuid();
        var serverSave = new ServerSaveData
        {
            AccountId = accountId,
            Data = "{\"gold\":50}",
            Version = 1,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-10),
        };
        _saveRepo.GetByAccountIdAsync(accountId).Returns(serverSave);

        var result = await _sut.SyncAsync(accountId, new SaveSyncRequest
        {
            Data = "{\"gold\":200}",
            ClientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Version = 2,
            Checksum = "def"
        });

        Assert.Equal("ACCEPTED", result.Action);
        Assert.Equal(2, result.Version);
        await _saveRepo.Received(1).UpsertAsync(Arg.Is<ServerSaveData>(s => s.Data == "{\"gold\":200}"));
    }

    [Fact]
    public async Task SyncAsync_ServerNewer_ReturnsServerData()
    {
        var accountId = Guid.NewGuid();
        var serverSave = new ServerSaveData
        {
            AccountId = accountId,
            Data = "{\"gold\":300}",
            Version = 5,
            UpdatedAt = DateTime.UtcNow,
        };
        _saveRepo.GetByAccountIdAsync(accountId).Returns(serverSave);

        var result = await _sut.SyncAsync(accountId, new SaveSyncRequest
        {
            Data = "{\"gold\":100}",
            ClientTimestamp = 0,
            Version = 3,
            Checksum = "old"
        });

        Assert.Equal("SERVER_NEWER", result.Action);
        Assert.Equal("{\"gold\":300}", result.Data);
        Assert.Equal(5, result.Version);
    }

    [Fact]
    public async Task LoadAsync_SaveExists_ReturnsLoadedData()
    {
        var accountId = Guid.NewGuid();
        var serverSave = new ServerSaveData
        {
            AccountId = accountId,
            Data = "{\"player\":\"data\"}",
            Version = 3,
            UpdatedAt = DateTime.UtcNow,
        };
        _saveRepo.GetByAccountIdAsync(accountId).Returns(serverSave);

        var result = await _sut.LoadAsync(accountId);

        Assert.NotNull(result);
        Assert.Equal("LOADED", result!.Action);
        Assert.Equal("{\"player\":\"data\"}", result.Data);
        Assert.Equal(3, result.Version);
    }

    [Fact]
    public async Task LoadAsync_NoSave_ReturnsNull()
    {
        var accountId = Guid.NewGuid();
        _saveRepo.GetByAccountIdAsync(accountId).Returns((ServerSaveData?)null);

        var result = await _sut.LoadAsync(accountId);

        Assert.Null(result);
    }

    [Fact]
    public async Task SyncAsync_ChecksumIsValidSha256()
    {
        var accountId = Guid.NewGuid();
        _saveRepo.GetByAccountIdAsync(accountId).Returns((ServerSaveData?)null);

        var data = "{\"test\":\"checksum\"}";

        await _sut.SyncAsync(accountId, new SaveSyncRequest
        {
            Data = data,
            ClientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Version = 1,
            Checksum = "x"
        });

        var expectedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
        await _saveRepo.Received(1).UpsertAsync(Arg.Is<ServerSaveData>(s => s.Checksum == expectedHash));
    }
}
