using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.ViewModels;

namespace OnlineStoreSystem.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly OnlineStoreDbContext _context;

        public OrderController(OnlineStoreDbContext context)
        {
            _context = context;
        }

        //  INDEX 
        public async Task<IActionResult> Index(string? status)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Deliveries)
                .Include(o => o.Payments)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(status))
                ordersQuery = ordersQuery.Where(o => o.Status == status);

            var orders = await ordersQuery.ToListAsync();
            await LoadSelectLists(status);

            return View(orders);
        }

        //  DETAILS 
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Deliveries)
                .Include(o => o.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();
            return View(order);
        }

        //  CREATE
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            await LoadSelectLists();
            return View(new OrderPlacementViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(OrderPlacementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectLists();
                return View(model);
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null || product.Stock < model.Amount)
            {
                ModelState.AddModelError("", "Недостатньо товару на складі");
                await LoadSelectLists();
                return View(model);
            }

            var order = new Order
            {
                CustomerId = model.CustomerId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = model.ProductId,
                        Amount = model.Amount,
                        Price = product.Price
                    }
                },
                Deliveries = new List<Delivery>
                {
                    new Delivery
                    {
                        Type = model.DeliveryType,
                        Cost = 0,
                        Status = "Pending Shipment"
                    }
                },
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Method = model.PaymentMethod,
                        Amount = product.Price * model.Amount,
                        PaymentDate = DateTime.Now
                    }
                }
            };

            product.Stock -= model.Amount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = order.OrderId });
        }

        // EDIT 
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Deliveries)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            var model = new OrderEditViewModel
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                DeliveryType = order.Deliveries.FirstOrDefault()?.Type ?? "",
                PaymentMethod = order.Payments.FirstOrDefault()?.Method ?? "",
                ExistingOrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    Amount = oi.Amount,
                    ProductName = oi.Product?.Name ?? "",
                    Price = oi.Price
                }).ToList()
            };

            await LoadSelectLists(order.Status);
            return View(model);
        }

[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin,Manager")]
public async Task<IActionResult> Edit(OrderEditViewModel model)
{
    if (!ModelState.IsValid)
    {
        await LoadSelectLists(model.Status);
        return View(model);
    }

    var order = await _context.Orders
        .Include(o => o.OrderItems)
        .Include(o => o.Deliveries)
        .Include(o => o.Payments)
        .FirstOrDefaultAsync(o => o.OrderId == model.OrderId);

    if (order == null)
        return NotFound();

    order.CustomerId = model.CustomerId;
    order.OrderDate = model.OrderDate;
    order.Status = model.Status;

    var delivery = order.Deliveries.FirstOrDefault();
    if (delivery != null && !string.IsNullOrWhiteSpace(model.DeliveryType))
        delivery.Type = model.DeliveryType;


    var payment = order.Payments.FirstOrDefault();
    if (payment != null && !string.IsNullOrWhiteSpace(model.PaymentMethod))
        payment.Method = model.PaymentMethod;

    // Existing
    foreach (var vm in model.ExistingOrderItems)
    {
        var item = order.OrderItems.FirstOrDefault(i => i.ProductId == vm.ProductId);
        if (item == null) continue;

        var product = await _context.Products.FindAsync(item.ProductId);
        if (product == null) continue;

        int diff = vm.Amount - item.Amount;

        if (diff > 0 && product.Stock < diff)
        {
            ModelState.AddModelError("", $"Недостатньо товару {product.Name}");
            await LoadSelectLists(model.Status);
            return View(model);
        }

        product.Stock -= diff;
        item.Amount = vm.Amount;
        item.Price = product.Price;
    }

    // New
    foreach (var vm in model.NewOrderItems)
    {
        if (vm.ProductId <= 0 || vm.Amount <= 0)
            continue;

        var product = await _context.Products.FindAsync(vm.ProductId);
        if (product == null || product.Stock < vm.Amount)
        {
            ModelState.AddModelError("", $"Недостатньо товару {product?.Name}");
            await LoadSelectLists(model.Status);
            return View(model);
        }

        order.OrderItems.Add(new OrderItem
        {
            ProductId = vm.ProductId,
            Amount = vm.Amount,
            Price = product.Price
        });

        product.Stock -= vm.Amount;
    }

    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Details), new { id = order.OrderId });
}


        //  DELETE 
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Include(o => o.Deliveries)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id, bool returnToStock = true)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .Include(o => o.Deliveries)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            if (returnToStock)
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null) product.Stock += item.Amount;
                }
            }

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Payments.RemoveRange(order.Payments);
            _context.Deliveries.RemoveRange(order.Deliveries);
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // CHANGE STATUS
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // HELPERS
        private async Task LoadSelectLists(string? currentStatus = null)
        {
            ViewBag.Customers = new SelectList(await _context.Customers.AsNoTracking().ToListAsync(), "CustomerId", "Name");

            ViewBag.Products = new SelectList(
                await _context.Products.AsNoTracking().Where(p => !p.IsDeleted && p.Stock > 0).ToListAsync(),
                "ProductId", "Name"
            );

            ViewBag.DeliveryTypes = new SelectList(
                await _context.Deliveries.AsNoTracking()
                    .Where(d => d.Type != null)
                    .Select(d => d.Type!).Distinct().ToListAsync()
            );

            ViewBag.PaymentMethods = new SelectList(
                await _context.Payments.AsNoTracking()
                    .Where(p => p.Method != null)
                    .Select(p => p.Method!).Distinct().ToListAsync()
            );

            var statuses = await _context.Orders.AsNoTracking()
                .Where(o => !string.IsNullOrEmpty(o.Status))
                .Select(o => o.Status)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            ViewBag.OrderStatuses = new SelectList(statuses, currentStatus);
        }
    }
}
