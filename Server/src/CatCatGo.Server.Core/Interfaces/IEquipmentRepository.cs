using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface IEquipmentRepository
{
    Task<EquipmentEntry?> GetByIdAsync(Guid id);
    Task<List<EquipmentEntry>> GetByAccountIdAsync(Guid accountId);
    Task<List<EquipmentEntry>> GetEquippedAsync(Guid accountId);
    Task CreateAsync(EquipmentEntry entry);
    Task UpdateAsync(EquipmentEntry entry);
    Task DeleteAsync(Guid id);
}
