namespace CatCatGo.Server.Core.Models;

public class ContentProgress
{
    public Guid AccountId { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int HighestStage { get; set; }
    public int DailyRunsUsed { get; set; }
    public DateTime LastResetDate { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Account? Account { get; set; }
}
