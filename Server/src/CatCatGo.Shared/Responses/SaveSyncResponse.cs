namespace CatCatGo.Shared.Responses;

public class SaveSyncResponse
{
    public required string Action { get; set; }
    public string? Data { get; set; }
    public long ServerTimestamp { get; set; }
    public int Version { get; set; }
}
