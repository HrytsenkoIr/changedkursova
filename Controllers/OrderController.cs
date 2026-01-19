using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories.Interfaces;
using OnlineStoreSystem.ViewModels;

namespace OnlineStoreSystem.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // INDEX
        public async Task<IActionResult> Index(string? status)
        {
            var orders = await _orderRepository.FilterOrdersAsync(status);
            await LoadSelectLists(status);
            return View(orders);
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // CREATE
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

            var order = new Order
            {
                CustomerId = model.CustomerId,
                OrderDate = DateTime.Now,
                Status = "Pending"
            };

            order.OrderItems.Add(new OrderItem
            {
                ProductId = model.ProductId,
                Amount = model.Amount,
                Price = (await _orderRepository.GetAvailableProductsAsync())
                        .FirstOrDefault(p => p.ProductId == model.ProductId)?.Price ?? 0
            });

            await _orderRepository.CreateAsync(order);

            return RedirectToAction(nameof(Index));
        }

        // EDIT
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(id);
            if (order == null) return NotFound();

            var model = new OrderEditViewModel
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                Status = order.Status ?? string.Empty,
                DeliveryType = order.Deliveries.FirstOrDefault()?.Type ?? string.Empty,
                PaymentMethod = order.Payments.FirstOrDefault()?.Method ?? string.Empty,
                ExistingOrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "",
                    Amount = oi.Amount,
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

            var order = await _orderRepository.GetByIdWithDetailsAsync(model.OrderId);
            if (order == null) return NotFound();

            order.CustomerId = model.CustomerId;
            order.OrderDate = model.OrderDate;
            order.Status = model.Status;


            foreach (var existingItem in model.ExistingOrderItems)
            {
                var oi = order.OrderItems.FirstOrDefault(x => x.ProductId == existingItem.ProductId);
                if (oi != null)
                {
                    oi.Amount = existingItem.Amount;
                    oi.Price = existingItem.Price;
                }
            }

            // Нові товари
            foreach (var newItem in model.NewOrderItems)
            {
                if (newItem.ProductId > 0 && newItem.Amount > 0)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = newItem.ProductId,
                        Amount = newItem.Amount,
                        Price = newItem.Price
                    });
                }
            }

            await _orderRepository.UpdateAsync(order, isDetached: false);

            return RedirectToAction(nameof(Details), new { id = model.OrderId });
        }

        // DELETE
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(id);
            if (order == null) return NotFound();

            await _orderRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // HELPERS
        private async Task LoadSelectLists(string? currentStatus = null)
        {
            ViewBag.Customers = new SelectList(
                await _orderRepository.GetCustomersAsync(),
                "CustomerId", "Name");

            ViewBag.Products = new SelectList(
                await _orderRepository.GetAvailableProductsAsync(),
                "ProductId", "Name");

            ViewBag.DeliveryTypes = new SelectList(
                await _orderRepository.GetDeliveryTypesAsync());

            ViewBag.PaymentMethods = new SelectList(
                await _orderRepository.GetPaymentMethodsAsync());

            ViewBag.OrderStatuses = new SelectList(
                await _orderRepository.GetOrderStatusesAsync(),
                currentStatus);
        }
    }
}
