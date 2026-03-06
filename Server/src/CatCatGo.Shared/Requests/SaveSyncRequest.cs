namespace CatCatGo.Shared.Requests;

public class SaveSyncRequest
{
    public required string Data { get; set; }
    public long ClientTimestamp { get; set; }
    public int Version { get; set; }
    public required string Checksum { get; set; }
}
