using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface IContentRepository
{
    Task<ContentProgress?> GetProgressAsync(Guid accountId, string contentType);
    Task UpsertProgressAsync(ContentProgress progress);
    Task<List<ContentProgress>> GetAllProgressAsync(Guid accountId);
}
