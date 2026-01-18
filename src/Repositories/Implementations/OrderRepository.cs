
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

public async Task<Order?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
{
    return await _context.Orders
        .Include(o => o.Customer)
            .ThenInclude(c => c.Address)
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
        .Include(o => o.Payments)
        .Include(o => o.Deliveries)
        .AsNoTracking()
        .AsSplitQuery() 
        .FirstOrDefaultAsync(o => o.OrderId == id, ct);
}

    //  CREATE
    public async Task<int> CreateAsync(Order order, CancellationToken ct = default)
    {
        if (order.OrderDate == default)
            order.OrderDate = DateTime.Now;

        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
        return order.OrderId;
    }

    // GET BY ID 
    public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Orders.FindAsync(new object[] { id }, ct);
    }

    // GET ALL 
    public async Task<List<Order>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default)
    {
        var query = _context.Orders.AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();
        return await query.ToListAsync(ct);
    }

    // UPDATE
    public async Task UpdateAsync(Order order, bool isDetached = false, CancellationToken ct = default)
    {
        if (order.OrderDate == default)
            order.OrderDate = DateTime.Now;

        if (isDetached)
        {
            var local = _context.Orders.Local.FirstOrDefault(o => o.OrderId == order.OrderId);
            if (local != null)
            {
                local.Status = order.Status;
                local.CustomerId = order.CustomerId;
                local.OrderDate = order.OrderDate;
            }
            else
            {
                _context.Orders.Update(order);
            }
        }
        else
        {
            var existing = await _context.Orders.FindAsync(new object[] { order.OrderId }, ct);
            if (existing == null) throw new InvalidOperationException("Order not found");

            existing.Status = order.Status;
            existing.CustomerId = order.CustomerId;
            existing.OrderDate = order.OrderDate;
        }

        await _context.SaveChangesAsync(ct);
    }

    // DELETE
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM Orders WHERE OrderID = {0}", new object[] { id }, ct);
    }

    //  GET ALL WITH DETAILS 
    public async Task<List<Order>> GetAllWithDetailsAsync(CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Payments)
            .Include(o => o.Deliveries)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    //  DTO 
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = null!;
        public int ItemsCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
    }

    public async Task<List<OrderDto>> GetOrdersDtoAsync(CancellationToken ct = default)
    {
        return await _context.Orders
            .Select(o => new OrderDto
            {
                OrderId = o.OrderId,
                CustomerName = o.Customer.Name,
                ItemsCount = o.OrderItems.Count,
                TotalAmount = o.OrderItems.Sum(oi => oi.Price * oi.Amount),
                Status = o.Status
            })
            .AsNoTracking()
            .ToListAsync(ct);
    }

    //  PAGINATION
    public async Task<(List<Order> Data, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string sortBy = "OrderDate", string sortDirection = "desc",
        CancellationToken ct = default)
    {
        var query = _context.Orders.AsQueryable();
        var totalCount = await query.CountAsync(ct);

        query = sortBy.ToLower() switch
        {
            "status" => sortDirection == "asc" ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status),
            "customer" => sortDirection == "asc" ? query.OrderBy(o => o.Customer.Name) : query.OrderByDescending(o => o.Customer.Name),
            _ => sortDirection == "asc" ? query.OrderBy(o => o.OrderDate) : query.OrderByDescending(o => o.OrderDate)
        };

        var data = await query
            .Include(o => o.Customer)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return (data, totalCount);
    }

    // ORDERS STATISTICS PER CUSTOMER
    public class OrdersStatDto
    {
        public string CustomerName { get; set; } = null!;
        public int OrdersCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public async Task<List<OrdersStatDto>> GetOrdersStatsPerCustomerAsync(CancellationToken ct = default)
    {
        return await _context.Orders
            .GroupBy(o => o.Customer.Name)
            .Select(g => new OrdersStatDto
            {
                CustomerName = g.Key,
                OrdersCount = g.Count(),
                TotalAmount = g.Sum(o => o.OrderItems.Sum(oi => oi.Price * oi.Amount))
            })
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // FILTER ORDERS
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
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Payments)
            .Include(o => o.Deliveries)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}