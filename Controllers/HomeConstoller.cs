using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Services;
using System.Data;

namespace OnlineStoreSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly OnlineStoreDbContext _context;
        private readonly StoredProcedureService _spService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            OnlineStoreDbContext context,
            StoredProcedureService spService,
            ILogger<HomeController> logger)
        {
            _context = context;
            _spService = spService;
            _logger = logger;
        }

        // Головна сторінка з статистикою
        public async Task<IActionResult> Index()
        {
            try
            {
                // Загальна статистика
                var totalProducts = await _context.Products.CountAsync();
                var totalCustomers = await _context.Customers.CountAsync();
                var totalOrders = await _context.Orders.CountAsync();
                var totalRevenue = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Where(oi => oi.Order.Status == "Paid")
                    .SumAsync(oi => oi.Price * oi.Amount);

                ViewBag.TotalProducts = totalProducts;
                ViewBag.TotalCustomers = totalCustomers;
                ViewBag.TotalOrders = totalOrders;
                ViewBag.TotalRevenue = totalRevenue;

                // Топ-продукти через збережену процедуру
                DataTable bestSellers = new DataTable();
                try
                {
                    bestSellers = await _spService.GetBestSellersAsync(5);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while retrieving best sellers");
                }
                ViewBag.BestSellers = bestSellers;

                // Останні 5 замовлень
                var recentOrders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();

                return View(recentOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading the home page");
                TempData["Error"] = "An error occurred while loading data.";
                return View(new List<Order>());
            }
        }

        // Сторінка помилки
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            return View(errorViewModel);
        }
    }

    // Модель для сторінки помилки
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
