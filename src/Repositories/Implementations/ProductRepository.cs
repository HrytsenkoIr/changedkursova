using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories.Interfaces;

namespace OnlineStoreSystem.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly OnlineStoreDbContext _context;

    public ProductRepository(OnlineStoreDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateAsync(Product product, CancellationToken ct = default)
    {
        await _context.Products.AddAsync(product, ct);
        await _context.SaveChangesAsync(ct);
        return product.ProductId;
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Products.FindAsync(new object[] { id }, ct);
    }

    public async Task<Product?> GetByIdWithOrdersAsync(int id, CancellationToken ct = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.OrderItems)
                .ThenInclude(oi => oi.Order)
                .ThenInclude(o => o.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == id, ct);
    }

    public async Task<List<Product>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default)
    {
        var query = _context.Products.AsQueryable();

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query.ToListAsync(ct);
    }

    public async Task<List<Product>> GetWithCategoriesAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<List<Product>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        var existing = await _context.Products.FindAsync(new object[] { product.ProductId }, ct);
        if (existing == null)
            throw new KeyNotFoundException("Product not found");

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.CategoryId = product.CategoryId;
        existing.Status = product.Status;
        existing.Metadata = product.Metadata;

        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateDetachedAsync(Product product, CancellationToken ct = default)
    {
        _context.Attach(product);
        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM Product WHERE ProductID = {0}",
            new object[] { id },
            ct);
    }

    public async Task<List<Product>> GetFilteredAsync(
        decimal? minPrice,
        decimal? maxPrice,
        int? categoryId,
        CancellationToken ct = default)
    {
        var query = _context.Products.AsQueryable();

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        return await query.AsNoTracking().ToListAsync(ct);
    }

    public async Task<Dictionary<int, decimal>> GetSalesStatsAsync(CancellationToken ct = default)
    {
        return await _context.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalRevenue = g.Sum(x => x.Price * x.Amount)
            })
            .ToDictionaryAsync(x => x.ProductId, x => x.TotalRevenue, ct);
    }
}
