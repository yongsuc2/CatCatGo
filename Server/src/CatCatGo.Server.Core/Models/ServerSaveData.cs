namespace CatCatGo.Server.Core.Models;

public class ServerSaveData
{
    public Guid AccountId { get; set; }
    public string Data { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Checksum { get; set; } = string.Empty;

    public Account? Account { get; set; }
}
