namespace CatCatGo.Server.Core.Models;

public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? PriceTier { get; set; }
    public int GemsAmount { get; set; }
    public int BonusGems { get; set; }
    public string? Rewards { get; set; }
    public bool IsActive { get; set; } = true;
    public string? EventId { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
}
