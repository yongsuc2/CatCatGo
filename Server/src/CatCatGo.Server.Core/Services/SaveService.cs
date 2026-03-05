using System.Security.Cryptography;
using System.Text;
using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using CatCatGo.Shared.Requests;
using CatCatGo.Shared.Responses;

namespace CatCatGo.Server.Core.Services;

public class SaveService
{
    private readonly ISaveRepository _saveRepo;

    public SaveService(ISaveRepository saveRepo)
    {
        _saveRepo = saveRepo;
    }

    public async Task<SaveSyncResponse> SyncAsync(Guid accountId, SaveSyncRequest request)
    {
        var serverSave = await _saveRepo.GetByAccountIdAsync(accountId);

        if (serverSave == null)
        {
            var newSave = new ServerSaveData
            {
                AccountId = accountId,
                Data = request.Data,
                Version = request.Version,
                UpdatedAt = DateTime.UtcNow,
                Checksum = ComputeChecksum(request.Data),
            };
            await _saveRepo.UpsertAsync(newSave);

            return new SaveSyncResponse
            {
                Action = "ACCEPTED",
                ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Version = request.Version,
            };
        }

        var serverTimestamp = new DateTimeOffset(serverSave.UpdatedAt, TimeSpan.Zero).ToUnixTimeMilliseconds();

        if (request.ClientTimestamp > serverTimestamp)
        {
            serverSave.Data = request.Data;
            serverSave.Version = request.Version;
            serverSave.UpdatedAt = DateTime.UtcNow;
            serverSave.Checksum = ComputeChecksum(request.Data);
            await _saveRepo.UpsertAsync(serverSave);

            return new SaveSyncResponse
            {
                Action = "ACCEPTED",
                ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Version = request.Version,
            };
        }

        return new SaveSyncResponse
        {
            Action = "SERVER_NEWER",
            Data = serverSave.Data,
            ServerTimestamp = serverTimestamp,
            Version = serverSave.Version,
        };
    }

    public async Task<SaveSyncResponse?> LoadAsync(Guid accountId)
    {
        var serverSave = await _saveRepo.GetByAccountIdAsync(accountId);
        if (serverSave == null) return null;

        return new SaveSyncResponse
        {
            Action = "LOADED",
            Data = serverSave.Data,
            ServerTimestamp = new DateTimeOffset(serverSave.UpdatedAt, TimeSpan.Zero).ToUnixTimeMilliseconds(),
            Version = serverSave.Version,
        };
    }

    private static string ComputeChecksum(string data)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
