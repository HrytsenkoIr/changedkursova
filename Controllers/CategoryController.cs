using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories.Interfaces;
using OnlineStoreSystem.Constants; // Додано

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

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryRepository.GetByIdWithProductsAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [Authorize(Roles = UserRoles.Admin)] // Виправлено
        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)] // Виправлено
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid) return View(model);
            await _categoryRepository.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = UserRoles.Admin)] // Виправлено
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)] // Виправлено
        public async Task<IActionResult> Edit(int id, Category model)
        {
            if (id != model.CategoryId) return NotFound();
            if (!ModelState.IsValid) return View(model);
            await _categoryRepository.UpdateAsync(model);
            return RedirectToAction(nameof(Details), new { id = model.CategoryId });
        }

        [Authorize(Roles = UserRoles.Admin)] // Виправлено
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdWithProductsAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)] // Виправлено
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