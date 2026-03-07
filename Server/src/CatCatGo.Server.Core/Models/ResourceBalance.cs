namespace CatCatGo.Server.Core.Models;

public class ResourceBalance
{
    public Guid AccountId { get; set; }
    public string Type { get; set; } = string.Empty;
    public double Amount { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Account? Account { get; set; }
}
