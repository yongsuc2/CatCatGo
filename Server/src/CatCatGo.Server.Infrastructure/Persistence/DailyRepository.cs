using CatCatGo.Server.Core.Interfaces;
using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class DailyRepository : IDailyRepository
{
    private readonly AppDbContext _db;

    public DailyRepository(AppDbContext db) { _db = db; }

    public async Task<DailyAttendance?> GetAttendanceAsync(Guid accountId)
    {
        return await _db.DailyAttendances.FindAsync(accountId);
    }

    public async Task UpsertAttendanceAsync(DailyAttendance attendance)
    {
        var existing = await _db.DailyAttendances.FindAsync(attendance.AccountId);
        if (existing == null)
            _db.DailyAttendances.Add(attendance);
        else
        {
            existing.CurrentDay = attendance.CurrentDay;
            existing.LastClaimDate = attendance.LastClaimDate;
            existing.CycleStartDate = attendance.CycleStartDate;
            existing.UpdatedAt = attendance.UpdatedAt;
        }
        await _db.SaveChangesAsync();
    }

    public async Task<List<QuestProgress>> GetQuestsAsync(Guid accountId, string questType, DateTime resetDate)
    {
        return await _db.QuestProgresses
            .Where(q => q.AccountId == accountId && q.QuestType == questType && q.ResetDate >= resetDate)
            .ToListAsync();
    }

    public async Task UpsertQuestAsync(QuestProgress quest)
    {
        if (quest.Id == 0)
            _db.QuestProgresses.Add(quest);
        else
            _db.QuestProgresses.Update(quest);
        await _db.SaveChangesAsync();
    }
}
