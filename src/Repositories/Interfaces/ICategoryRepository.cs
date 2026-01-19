using OnlineStoreSystem.EFModels;

namespace OnlineStoreSystem.Repositories.Interfaces;

public interface ICategoryRepository
{
    // CRUD
    Task<int> CreateAsync(Category category, CancellationToken ct = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Category?> GetByIdWithProductsAsync(int id, CancellationToken ct = default);
    Task<List<Category>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default);
    Task UpdateAsync(Category category, bool isDetached = false, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    // Для форм/селектів
    Task<bool> HasProductsAsync(int id, CancellationToken ct = default);
}
