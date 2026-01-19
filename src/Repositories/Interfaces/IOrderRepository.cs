using OnlineStoreSystem.EFModels;

namespace OnlineStoreSystem.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<int> CreateAsync(Order order, CancellationToken ct = default);

    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Order?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);

    Task<List<Order>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default);

    Task<List<Order>> FilterOrdersAsync(
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        decimal? minTotalAmount = null,
        CancellationToken ct = default);

    Task UpdateAsync(Order order, bool isDetached = false, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    // для форм
    Task<List<Customer>> GetCustomersAsync(CancellationToken ct = default);
    Task<List<Product>> GetAvailableProductsAsync(CancellationToken ct = default);
    Task<List<string>> GetOrderStatusesAsync(CancellationToken ct = default);
    Task<List<string>> GetDeliveryTypesAsync(CancellationToken ct = default);
    Task<List<string>> GetPaymentMethodsAsync(CancellationToken ct = default);
}
