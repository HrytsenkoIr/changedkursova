using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;
using System.Threading.Tasks;

namespace OnlineStoreSystem.Controllers
{
    public class CategoryController : Controller
    {
        private readonly OnlineStoreDbContext _context;

        public CategoryController(OnlineStoreDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index(string? search, string? sortBy, string? sortOrder)
        {
            var query = _context.Categories
                .Include(c => c.Products)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search));
                ViewBag.Search = search;
            }

            // Сортування
            sortBy ??= "id";
            sortOrder ??= "asc";

            query = (sortBy.ToLower(), sortOrder.ToLower()) switch
            {
                ("name", "asc") => query.OrderBy(c => c.Name),
                ("name", "desc") => query.OrderByDescending(c => c.Name),
                ("products", "asc") => query.OrderBy(c => c.Products.Count),
                ("products", "desc") => query.OrderByDescending(c => c.Products.Count),
                ("id", "desc") => query.OrderByDescending(c => c.CategoryId),
                _ => query.OrderBy(c => c.CategoryId),
            };

            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            var categories = await query.ToListAsync();
            return View(categories);
        }

        // GET: Category/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.CategoryId == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View(new Category());
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Category/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name")] Category category)
        {
            if (id != category.CategoryId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Category/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
