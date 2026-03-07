using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account?> GetByDeviceIdAsync(string deviceId);
    Task<Account?> GetBySocialIdAsync(string socialType, string socialId);
    Task<Account?> GetByRefreshTokenAsync(string refreshToken);
    Task<Account> CreateAsync(Account account);
    Task UpdateAsync(Account account);
}
