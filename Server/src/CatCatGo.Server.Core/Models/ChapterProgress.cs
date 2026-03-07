namespace CatCatGo.Server.Core.Models;

public class ChapterProgress
{
    public Guid AccountId { get; set; }
    public int ClearedChapterMax { get; set; }
    public string BestSurvivalDays { get; set; } = "{}";
    public string ClaimedTreasures { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; }

    public Account? Account { get; set; }
}
