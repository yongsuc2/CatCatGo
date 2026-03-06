namespace CatCatGo.Server.Core.Models;

public class CheatFlag
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Severity { get; set; } = 1;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }

    public Account? Account { get; set; }
}
