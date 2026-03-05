namespace CatCatGo.Server.Core.Models;

public class ArenaRanking
{
    public Guid AccountId { get; set; }
    public string Tier { get; set; } = "BRONZE";
    public int Points { get; set; }
    public int Season { get; set; } = 1;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public string PlayerData { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; }

    public Account? Account { get; set; }
}
