namespace CatCatGo.Server.Core.Models;

public class GachaPity
{
    public Guid AccountId { get; set; }
    public string BoxType { get; set; } = "EQUIPMENT";
    public int PityCount { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Account? Account { get; set; }
}
