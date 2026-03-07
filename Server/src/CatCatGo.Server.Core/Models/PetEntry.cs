namespace CatCatGo.Server.Core.Models;

public class PetEntry
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string PetId { get; set; } = string.Empty;
    public string Grade { get; set; } = "COMMON";
    public int Level { get; set; } = 1;
    public int Experience { get; set; }
    public bool IsEquipped { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Account? Account { get; set; }
}
