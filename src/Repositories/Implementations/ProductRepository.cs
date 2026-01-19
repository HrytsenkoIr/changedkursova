using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories.Interfaces;

namespace OnlineStoreSystem.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly OnlineStoreDbContext _context;

    public ProductRepository(OnlineStoreDbContext context) //доступ до дб
    {
        _context = context;
    }

    //Додати новий товар в базу
    public async Task<int> CreateAsync(Product product, CancellationToken ct = default)
    {
        await _context.Products.AddAsync(product, ct);//помічає в дб щоб потім в базу додати
        await _context.SaveChangesAsync(ct);//додати в базу
        return product.ProductId;//повертає айдішник нового продукту
    }

    //Отримати за ід
    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Products.FindAsync(new object[] { id }, ct);//шукає по первинному ключу
    }

    //Отримати за ід разом з категорією і замовленнями
    public async Task<Product?> GetByIdWithOrdersAsync(int id, CancellationToken ct = default)
    {
        return await _context.Products //EF колекція продуктів
            .Include(p => p.Category)//підвантажує категорію
            .Include(p => p.OrderItems)//підвантажує товари замовлень
                .ThenInclude(oi => oi.Order) //підвантажує саме замовлення
                .ThenInclude(o => o.Payments)// підвантажує платежі
            .AsNoTracking()//тільки читання, щоб не відстеж стан
            .FirstOrDefaultAsync(p => p.ProductId == id, ct);//асинх запит на перший продукт з ід
    }

    public async Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.OrderItems)
                .ThenInclude(oi => oi.Order)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == id, ct);
    }

    public async Task<Product?> GetByIdForDeleteAsync(int id, CancellationToken ct = default)
    {
        return await _context.Products
            .Include(p => p.OrderItems)
            .FirstOrDefaultAsync(p => p.ProductId == id, ct);
    }

    public async Task<List<Category>> GetAllCategoriesAsync(CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .ToListAsync(ct);
    }

    //отримати всі продукти
    public async Task<List<Product>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default)
    {
        var query = _context.Products.AsQueryable();//запит щоб ланцюжити

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query.ToListAsync(ct); //Селект і повертає список продуктів
    }

    //отримати всі продукти з категоріями
    public async Task<List<Product>> GetWithCategoriesAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .Include(p => p.Category) //джоін з категоріями
            .AsNoTracking()
            .ToListAsync(ct);
    }

    //отримати сторінку продуктів для пагінації
    public async Task<List<Product>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)//пропуск попередніх
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        var existing = await _context.Products.FindAsync(new object[] { product.ProductId }, ct);//завантаж
        if (existing == null)
            throw new KeyNotFoundException("Product not found");

        //оновлює поля
        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        existing.CategoryId = product.CategoryId;
        existing.Status = product.Status;
        existing.Metadata = product.Metadata;

        await _context.SaveChangesAsync(ct); //зберіг зміни
    }

    //детатчед, бо дані ззовні
    public async Task UpdateDetachedAsync(Product product, CancellationToken ct = default)
    {
        _context.Attach(product);//підк до еф
        _context.Entry(product).State = EntityState.Modified;//помічає як змінений
        await _context.SaveChangesAsync(ct);//зберігає зміни
    }

    //видалення через скл за ід
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM Product WHERE ProductID = {0}",//прямий делете без завантаження
            new object[] { id },
            ct);
    }

    //продукти за фільтрами
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

    //статистика продажів
    public async Task<Dictionary<int, decimal>> GetSalesStatsAsync(CancellationToken ct = default)
    {
        return await _context.OrderItems
            .GroupBy(oi => oi.ProductId)//група по продуктід
            .Select(g => new
            {
                ProductId = g.Key,
                TotalRevenue = g.Sum(x => x.Price * x.Amount)
            })
            .ToDictionaryAsync(x => x.ProductId, x => x.TotalRevenue, ct);
    }
}
