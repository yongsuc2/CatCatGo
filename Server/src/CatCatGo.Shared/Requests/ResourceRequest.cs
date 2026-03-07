namespace CatCatGo.Shared.Requests;

public class ResourceSpendRequest
{
    public required string Type { get; set; }
    public double Amount { get; set; }
    public required string Source { get; set; }
    public string? RefId { get; set; }
}
