namespace CatCatGo.Shared.Responses;

public class ResourceBalanceResponse
{
    public required Dictionary<string, double> Balances { get; set; }
}

public class ResourceSpendResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public double RemainingBalance { get; set; }
}
