using StackExchange.Redis;

namespace CatCatGo.Server.Infrastructure.Cache;

public class RedisSessionStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisSessionStore(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task SetSessionAsync(string accountId, string data, TimeSpan expiry)
    {
        await _db.StringSetAsync($"session:{accountId}", data, expiry);
    }

    public async Task<string?> GetSessionAsync(string accountId)
    {
        var value = await _db.StringGetAsync($"session:{accountId}");
        return value.HasValue ? value.ToString() : null;
    }

    public async Task RemoveSessionAsync(string accountId)
    {
        await _db.KeyDeleteAsync($"session:{accountId}");
    }

    public async Task SetBattleSessionAsync(string battleId, string data, TimeSpan expiry)
    {
        await _db.StringSetAsync($"battle:{battleId}", data, expiry);
    }

    public async Task<string?> GetBattleSessionAsync(string battleId)
    {
        var value = await _db.StringGetAsync($"battle:{battleId}");
        return value.HasValue ? value.ToString() : null;
    }

    public async Task IncrementRateLimitAsync(string key, TimeSpan window)
    {
        var fullKey = $"ratelimit:{key}";
        await _db.StringIncrementAsync(fullKey);
        await _db.KeyExpireAsync(fullKey, window, ExpireWhen.HasNoExpiry);
    }

    public async Task<long> GetRateLimitCountAsync(string key)
    {
        var value = await _db.StringGetAsync($"ratelimit:{key}");
        return value.HasValue ? (long)value : 0;
    }
}
