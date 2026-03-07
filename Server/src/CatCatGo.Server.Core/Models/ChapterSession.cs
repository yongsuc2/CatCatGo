namespace CatCatGo.Server.Core.Models;

public class ChapterSession
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public int ChapterId { get; set; }
    public int CurrentDay { get; set; }
    public int Seed { get; set; }
    public string SessionSkills { get; set; } = "[]";
    public int RerollsUsed { get; set; }
    public int JungbakCounter { get; set; }
    public int DaebakCounter { get; set; }
    public string PendingEncounter { get; set; } = "{}";
    public string PendingSkillChoices { get; set; } = "[]";
    public bool IsActive { get; set; } = true;
    public int BestSurvivalDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Account? Account { get; set; }
}
