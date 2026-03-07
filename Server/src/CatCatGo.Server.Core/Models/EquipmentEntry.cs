namespace CatCatGo.Server.Core.Models;

public class EquipmentEntry
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string TemplateId { get; set; } = string.Empty;
    public string Grade { get; set; } = "COMMON";
    public int EnhancementLevel { get; set; }
    public bool IsS { get; set; }
    public string Slot { get; set; } = string.Empty;
    public string? WeaponSubType { get; set; }
    public string SubStats { get; set; } = "[]";
    public int SlotIndex { get; set; } = -1;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Account? Account { get; set; }
}
