using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface IDailyRepository
{
    Task<DailyAttendance?> GetAttendanceAsync(Guid accountId);
    Task UpsertAttendanceAsync(DailyAttendance attendance);
    Task<List<QuestProgress>> GetQuestsAsync(Guid accountId, string questType, DateTime resetDate);
    Task UpsertQuestAsync(QuestProgress quest);
}
