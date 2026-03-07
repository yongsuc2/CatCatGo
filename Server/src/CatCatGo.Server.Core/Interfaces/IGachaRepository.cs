using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface IGachaRepository
{
    Task<GachaPity?> GetPityAsync(Guid accountId, string boxType);
    Task UpsertPityAsync(GachaPity pity);
}
