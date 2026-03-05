namespace CatCatGo.Server.Core.Models;

public class Account
{
    public Guid Id { get; set; }
    public string? DeviceId { get; set; }
    public string? SocialType { get; set; }
    public string? SocialId { get; set; }
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}
