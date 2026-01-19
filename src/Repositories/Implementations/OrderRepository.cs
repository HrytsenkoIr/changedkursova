using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories.Interfaces;

namespace OnlineStoreSystem.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OnlineStoreDbContext _context;

    public OrderRepository(OnlineStoreDbContext context)
    {
        _context = context;
    }
//Отримати замовлення по ід  усіма пов'язаними даними
    public async Task<Order?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.Customer).ThenInclude(c => c.Address)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .Include(o => o.Payments)
            .Include(o => o.Deliveries)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(o => o.OrderId == id, ct);
    }
//Отримати замовлення по ід
    public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Orders.FindAsync(new object[] { id }, ct);
    }

    public async Task<int> CreateAsync(Order order, CancellationToken ct = default)
    {
        //Якщо дата не вказана, встановити поточну
        if (order.OrderDate == default)
            order.OrderDate = DateTime.Now;

        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
        return order.OrderId;
    }

//Отримати всі замовлення з підвантаженням пов'язаних даних
    public async Task<List<Order>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default)
    {
        var query = _context.Orders.AsQueryable();
        if (!trackChanges)
            query = query.AsNoTracking();

        return await query
            .Include(o => o.Customer)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .Include(o => o.Deliveries)
            .Include(o => o.Payments)
            .ToListAsync(ct);
    }

//фільтрація замовлнь за різними критеріями
    public async Task<List<Order>> FilterOrdersAsync(
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        decimal? minTotalAmount = null,
        CancellationToken ct = default)
    {
        var query = _context.Orders.AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        if (minTotalAmount.HasValue)
            query = query.Where(o => o.OrderItems.Sum(oi => oi.Price * oi.Amount) >= minTotalAmount.Value);

        return await query
            .Include(o => o.Customer)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
            .Include(o => o.Payments)
            .Include(o => o.Deliveries)
            .AsNoTracking()
            .ToListAsync(ct);
    }
//Оновлення замовлення
    public async Task UpdateAsync(Order order, bool isDetached = false, CancellationToken ct = default)
    {
        if (order.OrderDate == default)
            order.OrderDate = DateTime.Now;

        if (isDetached)
        {
            _context.Attach(order);
            _context.Entry(order).State = EntityState.Modified;
        }
        else
        {//Знайти існуюче замовлення
            var existing = await _context.Orders.FindAsync(new object[] { order.OrderId }, ct);
            if (existing == null)
                throw new InvalidOperationException("Order not found");

            existing.Status = order.Status;
            existing.CustomerId = order.CustomerId;
            existing.OrderDate = order.OrderDate;
        }

        await _context.SaveChangesAsync(ct);
    }
//Видалення замовлення по ід
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM Orders WHERE OrderID = {0}",
            new object[] { id },
            ct);
    }

//Список клієнтів
    public async Task<List<Customer>> GetCustomersAsync(CancellationToken ct = default)
        => await _context.Customers.AsNoTracking().ToListAsync(ct);

//Список товарів доступних
    public async Task<List<Product>> GetAvailableProductsAsync(CancellationToken ct = default)
        => await _context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Stock > 0)
            .ToListAsync(ct);
//Всі типи доставки
    public async Task<List<string>> GetDeliveryTypesAsync(CancellationToken ct = default)
        => await _context.Deliveries.AsNoTracking()
            .Where(d => d.Type != null)
            .Select(d => d.Type!)
            .Distinct()
            .ToListAsync(ct);
//Всі способи оплати
    public async Task<List<string>> GetPaymentMethodsAsync(CancellationToken ct = default)
        => await _context.Payments.AsNoTracking()
            .Where(p => p.Method != null)
            .Select(p => p.Method!)
            .Distinct()
            .ToListAsync(ct);
//Всі можливі статуси замовлень
    public async Task<List<string>> GetOrderStatusesAsync(CancellationToken ct = default)
        => await _context.Orders.AsNoTracking()
            .Where(o => o.Status != null)
            .Select(o => o.Status!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync(ct);
}
