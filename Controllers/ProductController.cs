using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories.Interfaces;
using OnlineStoreSystem.ViewModels;

namespace OnlineStoreSystem.Controllers
{
    [Authorize(Policy = "WorkerOrAbove")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly OnlineStoreDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductRepository productRepository,
            OnlineStoreDbContext context,
            ILogger<ProductController> logger)
        {
            _productRepository = productRepository;
            _context = context;
            _logger = logger;
        }

        // INDEX
        [AllowAnonymous]
        public async Task<IActionResult> Index(decimal? minPrice, decimal? maxPrice, int? categoryId)
        {
            var products = await _productRepository.GetFilteredAsync(minPrice, maxPrice, categoryId);

            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "CategoryId",
                "Name");

            return View(products);
        }


        // DETAILS
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Index));

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.OrderItems)
                    .ThenInclude(oi => oi.Order)
                .FirstOrDefaultAsync(p => p.ProductId == id.Value);

            if (product == null)
                return RedirectToAction(nameof(Index));

            return View(product);
        }

        // CREATE (GET)
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "CategoryId",
                "Name");

            return View(new ProductCreateViewModel());
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Create(ProductCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    await _context.Categories.ToListAsync(),
                    "CategoryId",
                    "Name",
                    vm.CategoryId);
                return View(vm);
            }

            var product = new Product
            {
                Name = vm.Name,
                Description = vm.Description,
                Price = vm.Price,
                Stock = vm.Stock,
                CategoryId = vm.CategoryId,
                Status = vm.Stock > 0 ? ProductStatus.Active : ProductStatus.OutOfStock
            };

            var id = await _productRepository.CreateAsync(product);
            return RedirectToAction(nameof(Details), new { id });
        }

        // EDIT (GET)
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Index));

            var product = await _productRepository.GetByIdAsync(id.Value);
            if (product == null)
                return RedirectToAction(nameof(Index));

            var vm = new ProductCreateViewModel
            {
                Id = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId ?? 0
            };

            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "CategoryId",
                "Name",
                vm.CategoryId);

            return View(vm);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Edit(int id, ProductCreateViewModel vm)
        {
            if (id != vm.Id)
                return RedirectToAction(nameof(Index));

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    await _context.Categories.ToListAsync(),
                    "CategoryId",
                    "Name",
                    vm.CategoryId);
                return View(vm);
            }

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return RedirectToAction(nameof(Index));

            product.Name = vm.Name;
            product.Description = vm.Description;
            product.Price = vm.Price;
            product.Stock = vm.Stock;
            product.CategoryId = vm.CategoryId;
            product.Status = product.Stock > 0
                ? ProductStatus.Active
                : ProductStatus.OutOfStock;

            await _productRepository.UpdateAsync(product);
            return RedirectToAction(nameof(Details), new { id });
        }

        // DELETE (GET)
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(Index));

            var product = await _context.Products
                .Include(p => p.OrderItems)
                .FirstOrDefaultAsync(p => p.ProductId == id.Value);

            if (product == null)
                return RedirectToAction(nameof(Index));

            ViewBag.HasOrders = product.OrderItems != null && product.OrderItems.Any();
            ViewBag.OrderCount = product.OrderItems?.Count ?? 0;

            return View(product);
        }

        // DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.OrderItems)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return RedirectToAction(nameof(Index));

            if (product.OrderItems != null && product.OrderItems.Any())
            {
                TempData["Error"] = "Неможливо видалити товар, який використовується у замовленнях.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            await _productRepository.DeleteAsync(id);
            TempData["Success"] = "Товар успішно видалено.";

            return RedirectToAction(nameof(Index));
        }

        // STATISTICS
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Statistics()
        {
            var stats = await _productRepository.GetSalesStatsAsync();
            var products = await _productRepository.GetAllAsync(false);

            ViewBag.SalesStats = stats;
            return View(products);
        }
    }
}
