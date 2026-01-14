using OnlineStore.Models;

namespace OnlineStore.Repositories;

public interface IProductRepository
{
    Task<int> CreateAsync(Product product, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Product>> GetAllAsync(CancellationToken ct = default);
    Task<List<Product>> GetWithCategoriesAsync(CancellationToken ct = default);
}