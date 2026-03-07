namespace CatCatGo.Server.Core.Models;

public class TalentState
{
    public Guid AccountId { get; set; }
    public int AtkLevel { get; set; }
    public int HpLevel { get; set; }
    public int DefLevel { get; set; }
    public int TotalLevel { get; set; }
    public string Grade { get; set; } = "TRAINEE";
    public int SubGrade { get; set; } = 1;
    public string ClaimedMilestones { get; set; } = "[]";
    public DateTime UpdatedAt { get; set; }

    public Account? Account { get; set; }
}
