using CatCatGo.Server.Core.Models;

namespace CatCatGo.Server.Core.Interfaces;

public interface IProductRepository
{
    Task<List<Product>> GetActiveProductsAsync();
    Task<Product?> GetByIdAsync(string id);
    Task<Product> CreateAsync(Product product);
    Task UpdateAsync(Product product);
}
