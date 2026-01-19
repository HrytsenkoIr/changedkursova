using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories.Interfaces;

namespace OnlineStoreSystem.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly OnlineStoreDbContext _context;

    public CategoryRepository(OnlineStoreDbContext context)
    {
        _context = context;
    }

    // Створення категорії
    public async Task<int> CreateAsync(Category category, CancellationToken ct = default)
    {
        await _context.Categories.AddAsync(category, ct);
        await _context.SaveChangesAsync(ct);
        return category.CategoryId;
    }

    // Отримати категорію за id
    public async Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == id, ct);
    }

    // Отримати категорію з продуктами
    public async Task<Category?> GetByIdWithProductsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == id, ct);
    }

    // Отримати всі категорії
    public async Task<List<Category>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default)
    {
        var query = _context.Categories.AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();

        return await query
            .Include(c => c.Products)
            .ToListAsync(ct);
    }

    // Оновлення категорії
    public async Task UpdateAsync(Category category, bool isDetached = false, CancellationToken ct = default)
    {
        if (isDetached)
        {
            _context.Attach(category);
            _context.Entry(category).State = EntityState.Modified;
        }
        else
        {
            var existing = await _context.Categories.FindAsync(new object[] { category.CategoryId }, ct);
            if (existing == null) throw new InvalidOperationException("Category not found");

            existing.Name = category.Name;
            // Description видалено, бо його немає у моделі
        }

        await _context.SaveChangesAsync(ct);
    }

    // Видалення категорії
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await _context.Categories.FindAsync(new object[] { id }, ct);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(ct);
        }
    }

    // Перевірка чи є продукти у категорії
    public async Task<bool> HasProductsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Products.AnyAsync(p => p.CategoryId == id, ct);
    }
}
