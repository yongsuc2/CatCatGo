namespace CatCatGo.Shared.Responses;

public class ArenaMatchResponse
{
    public required string MatchId { get; set; }
    public required List<ArenaOpponentDto> Opponents { get; set; }
}

public class ArenaOpponentDto
{
    public required string AccountId { get; set; }
    public required string DisplayName { get; set; }
    public required string Tier { get; set; }
    public int Points { get; set; }
    public required string PlayerDataJson { get; set; }
}

public class ArenaRankingResponse
{
    public required List<ArenaRankEntry> Rankings { get; set; }
    public int MyRank { get; set; }
    public required string MyTier { get; set; }
    public int MyPoints { get; set; }
}

public class ArenaRankEntry
{
    public int Rank { get; set; }
    public required string DisplayName { get; set; }
    public required string Tier { get; set; }
    public int Points { get; set; }
}

public class BattleStartResponse
{
    public required string BattleId { get; set; }
    public int Seed { get; set; }
}

public class BattleReportResponse
{
    public bool Verified { get; set; }
    public string? RewardsJson { get; set; }
    public string? Error { get; set; }
}
