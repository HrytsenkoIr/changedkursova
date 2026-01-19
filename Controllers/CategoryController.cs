using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories.Interfaces;

namespace OnlineStoreSystem.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // INDEX: список категорій
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        // DETAILS: перегляд категорії
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryRepository.GetByIdWithProductsAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // CREATE: форма створення
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new Category());
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid) return View(model);

            await _categoryRepository.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // EDIT: форма редагування
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Category model)
        {
            if (id != model.CategoryId) return NotFound();
            if (!ModelState.IsValid) return View(model);

            await _categoryRepository.UpdateAsync(model);
            return RedirectToAction(nameof(Details), new { id = model.CategoryId });
        }

        // DELETE: форма підтвердження видалення
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdWithProductsAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasProducts = await _categoryRepository.HasProductsAsync(id);
            if (hasProducts)
            {
                TempData["Error"] = "Неможливо видалити категорію: спочатку видаліть або перенесіть усі продукти.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            await _categoryRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
