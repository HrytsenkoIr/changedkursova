using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories.Interfaces;

namespace OnlineStoreSystem.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly OnlineStoreDbContext _context;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(OnlineStoreDbContext context, ILogger<CategoryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> CreateAsync(Category category, CancellationToken ct = default)
    {
        await _context.Categories.AddAsync(category, ct);
        await _context.SaveChangesAsync(ct);
        return category.CategoryId;
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == id, ct);
    }

    public async Task<Category?> GetByIdWithProductsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == id, ct);
    }

    public async Task<List<Category>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default)
    {
        var query = _context.Categories.AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();

        return await query
            .Include(c => c.Products)
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(Category category, bool isDetached = false, CancellationToken ct = default)
    {
        try 
        {
            if (isDetached)
            {
                _context.Attach(category);
                _context.Entry(category).State = EntityState.Modified;
            }
            else
            {
                var existing = await _context.Categories.FindAsync(new object[] { category.CategoryId }, ct);
                if (existing == null) 
                {
                    _logger.LogWarning("Категорію з ID {Id} не знайдено для оновлення", category.CategoryId);
                    throw new InvalidOperationException("Category not found");
                }
                existing.Name = category.Name;
            }
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при спробі оновити категорію {Id}", category.CategoryId);
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await _context.Categories.FindAsync(new object[] { id }, ct);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> HasProductsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Products.AnyAsync(p => p.CategoryId == id, ct);
    }
}