namespace CatCatGo.Server.Core.Models;

public class HeritageState
{
    public Guid AccountId { get; set; }
    public int SkullLevel { get; set; }
    public int KnightLevel { get; set; }
    public int RangerLevel { get; set; }
    public int GhostLevel { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Account? Account { get; set; }
}
