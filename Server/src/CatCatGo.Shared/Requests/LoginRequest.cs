namespace CatCatGo.Shared.Requests;

public class RegisterRequest
{
    public required string DeviceId { get; set; }
    public string? DisplayName { get; set; }
}

public class LoginRequest
{
    public string? DeviceId { get; set; }
    public string? SocialToken { get; set; }
    public string? SocialType { get; set; }
}

public class RefreshTokenRequest
{
    public required string RefreshToken { get; set; }
}

public class LinkSocialRequest
{
    public required string SocialType { get; set; }
    public required string SocialToken { get; set; }
}
