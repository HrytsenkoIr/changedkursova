using OnlineStore.Models;

namespace OnlineStore.Repositories;

public interface IOrderRepository
{
    Task<int> CreateOrderWithTransactionAsync(Order order, List<OrderItem> items, CancellationToken ct = default);
}