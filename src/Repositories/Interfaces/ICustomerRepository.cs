using OnlineStore.Models;

namespace OnlineStore.Repositories;

public interface ICustomerRepository
{
    Task<int> CreateAsync(Customer customer, CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Customer>> GetAllAsync(CancellationToken ct = default);
    Task<bool> UpdateAsync(Customer customer, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}