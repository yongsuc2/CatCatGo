using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface IChapterRepository
{
    Task<ChapterSession?> GetActiveSessionAsync(Guid accountId);
    Task<ChapterSession?> GetByIdAsync(Guid id);
    Task CreateSessionAsync(ChapterSession session);
    Task UpdateSessionAsync(ChapterSession session);
    Task<ChapterProgress?> GetProgressAsync(Guid accountId);
    Task UpsertProgressAsync(ChapterProgress progress);
}
