namespace CatCatGo.Server.Core.Models;

public class DailyAttendance
{
    public Guid AccountId { get; set; }
    public int CurrentDay { get; set; } = 1;
    public DateTime LastClaimDate { get; set; }
    public DateTime CycleStartDate { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Account? Account { get; set; }
}
