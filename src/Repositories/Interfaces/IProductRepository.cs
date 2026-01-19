using OnlineStoreSystem.EFModels;

namespace OnlineStoreSystem.Repositories.Interfaces;

public interface IProductRepository
{
    Task<int> CreateAsync(Product product, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Product?> GetByIdWithOrdersAsync(int id, CancellationToken ct = default);

    Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);

    Task<Product?> GetByIdForDeleteAsync(int id, CancellationToken ct = default);

    Task<List<Category>> GetAllCategoriesAsync(CancellationToken ct = default);

    Task<List<Product>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task UpdateDetachedAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<List<Product>> GetWithCategoriesAsync(CancellationToken ct = default);
    Task<List<Product>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<List<Product>> GetFilteredAsync(decimal? minPrice, decimal? maxPrice, int? categoryId, CancellationToken ct = default);
    Task<Dictionary<int, decimal>> GetSalesStatsAsync(CancellationToken ct = default);
}
