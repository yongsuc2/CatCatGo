namespace CatCatGo.Server.Core.Models;

public class BattleSession
{
    public string BattleId { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public int Seed { get; set; }
    public int ChapterId { get; set; }
    public int Day { get; set; }
    public string EncounterType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsCompleted { get; set; }
}
