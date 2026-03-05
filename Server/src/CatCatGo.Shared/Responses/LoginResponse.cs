namespace CatCatGo.Shared.Responses;

public class LoginResponse
{
    public required string AccountId { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public long ExpiresAt { get; set; }
    public bool IsNewAccount { get; set; }
}
