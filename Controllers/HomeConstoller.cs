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

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get statistics
                var totalProducts = await _context.Products.CountAsync(p => !p.IsDeleted);
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

                // Get top-selling products
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

                // Last 5 orders
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

        public IActionResult About()
        {
            ViewData["Title"] = "About Us";
            ViewData["Message"] = "Online Store System - A Magical Online Shop";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Contacts";
            ViewData["Message"] = "Our contact information";
            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["Title"] = "Privacy Policy";
            return View();
        }

        // Actions for working with stored procedures
        public async Task<IActionResult> PlaceOrder()
        {
            try
            {
                ViewBag.Customers = await _context.Customers.ToListAsync();
                ViewBag.Products = await _context.Products
                    .Where(p => !p.IsDeleted && p.Stock > 0 && p.Status == ProductStatus.Active)
                    .ToListAsync();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading the order creation page");
                TempData["Error"] = "Error while loading data.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int customerId, int productId, int amount, string deliveryType)
        {
            try
            {
                if (customerId <= 0 || productId <= 0 || amount <= 0 || string.IsNullOrEmpty(deliveryType))
                {
                    TempData["Error"] = "Please fill in all fields correctly.";
                    return RedirectToAction("PlaceOrder");
                }

                var result = await _spService.PlaceOrderAsync(customerId, productId, amount, deliveryType);

                if (result.returnCode == 0)
                {
                    TempData["Success"] = $"Order #{result.orderId} has been successfully created!";
                }
                else
                {
                    TempData["Error"] = "Error creating order: insufficient stock.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating order");
                TempData["Error"] = $"Error creating order: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Statistics for the last month
                var lastMonth = DateTime.Now.AddMonths(-1);

                var monthlyOrders = await _context.Orders
                    .Where(o => o.OrderDate >= lastMonth)
                    .CountAsync();

                var monthlyRevenue = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Where(oi => oi.Order.OrderDate >= lastMonth && oi.Order.Status == "Paid")
                    .SumAsync(oi => oi.Price * oi.Amount);

                // Most popular categories
                var popularCategories = await _context.Categories
                    .Include(c => c.Products)
                    .ThenInclude(p => p.OrderItems)
                    .Select(c => new
                    {
                        Category = c.Name,
                        SalesCount = c.Products.Sum(p => p.OrderItems.Sum(oi => oi.Amount))
                    })
                    .OrderByDescending(x => x.SalesCount)
                    .Take(5)
                    .ToListAsync();

                ViewBag.MonthlyOrders = monthlyOrders;
                ViewBag.MonthlyRevenue = monthlyRevenue;
                ViewBag.PopularCategories = popularCategories;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading dashboard");
                TempData["Error"] = "Error while loading statistics.";
                return RedirectToAction("Index");
            }
        }

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

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}