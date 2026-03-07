namespace CatCatGo.Server.Core.Models;

public class QuestProgress
{
    public long Id { get; set; }
    public Guid AccountId { get; set; }
    public string QuestId { get; set; } = string.Empty;
    public string QuestType { get; set; } = "DAILY";
    public int Progress { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsRewarded { get; set; }
    public DateTime ResetDate { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Account? Account { get; set; }
}
