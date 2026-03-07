namespace CatCatGo.Server.Core.Models;

public class ResourceLedger
{
    public long Id { get; set; }
    public Guid AccountId { get; set; }
    public string Type { get; set; } = string.Empty;
    public double Delta { get; set; }
    public double Balance { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? RefId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Account? Account { get; set; }
}
