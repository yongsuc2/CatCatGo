namespace CatCatGo.Shared.Requests;

public class BattleStartRequest
{
    public int ChapterId { get; set; }
    public int Day { get; set; }
    public required string EncounterType { get; set; }
}

public class BattleReportRequest
{
    public required string BattleId { get; set; }
    public int Seed { get; set; }
    public required string Result { get; set; }
    public int TurnCount { get; set; }
    public required List<string> PlayerSkillIds { get; set; }
    public required string EnemyTemplateId { get; set; }
    public int GoldReward { get; set; }
}
