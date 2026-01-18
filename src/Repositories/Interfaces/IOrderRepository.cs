using OnlineStoreSystem.EFModels;

namespace OnlineStoreSystem.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<int> CreateAsync(Order order, CancellationToken ct = default);

    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);

// IOrderRepository.cs
Task<Order?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);

    Task<List<Order>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default);

    Task UpdateAsync(Order order, bool isDetached = false, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    // FILTER
    Task<List<Order>> FilterOrdersAsync(
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        decimal? minTotalAmount = null,
        CancellationToken ct = default);

    // DTO 
    Task<List<OrderRepository.OrderDto>> GetOrdersDtoAsync(CancellationToken ct = default);

    //  STATS
    Task<List<OrderRepository.OrdersStatDto>> GetOrdersStatsPerCustomerAsync(CancellationToken ct = default);
}