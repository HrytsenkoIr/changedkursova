using OnlineStoreSystem.EFModels;

namespace OnlineStoreSystem.Repositories;

public interface ICustomerRepository
{
    Task<Customer> CreateAsync(Customer customer, CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Customer?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);

    Task<List<Customer>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default);

    Task<List<Customer>> GetFilteredAsync(
        string? name,
        string? email,
        string? phone,
        string? city,
        string? street,
        string? zip,
        CancellationToken ct = default);

    Task<bool> HasOrdersAsync(int customerId, CancellationToken ct = default);

    Task UpdateAsync(Customer customer, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
