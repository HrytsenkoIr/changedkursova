using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories;
using OnlineStoreSystem.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineStoreSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly OnlineStoreDbContext _context;

        public CustomerController(ICustomerRepository customerRepository, OnlineStoreDbContext context)
        {
            _customerRepository = customerRepository;
            _context = context;
        }

        // GET: Customer
       public async Task<IActionResult> Index(
    string? name,
    string? email,
    string? phone,
    string? city,
    string? street,
    string? zip)
{
    var query = _context.Customers
        .Include(c => c.Address)
        .AsNoTracking()
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(name))
        query = query.Where(c => c.Name.Contains(name));

    if (!string.IsNullOrWhiteSpace(email))
        query = query.Where(c => c.Email.Contains(email));

    if (!string.IsNullOrWhiteSpace(phone))
        query = query.Where(c => c.Phone.Contains(phone));

    if (!string.IsNullOrWhiteSpace(city))
        query = query.Where(c => c.Address.City!.Contains(city));

    if (!string.IsNullOrWhiteSpace(street))
        query = query.Where(c => c.Address.Street!.Contains(street));

    if (!string.IsNullOrWhiteSpace(zip))
        query = query.Where(c => c.Address.ZipCode!.Contains(zip));

    var customers = await query.ToListAsync();
    return View(customers);
}


        // GET: Customer/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderItems)
                .Include(c => c.Address)
                .FirstOrDefaultAsync(c => c.CustomerId == id.Value);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,Name,Email,Phone,Address")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _customerRepository.CreateAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customer/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _customerRepository.GetByIdAsync(id.Value);
            if (customer == null) return NotFound();

            var model = new CustomerEditViewModel
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = new AddressViewModel
                {
                    Street = customer.Address?.Street ?? "",
                    City = customer.Address?.City ?? "",
                    ZipCode = customer.Address?.ZipCode ?? ""
                }
            };

            return View(model);
        }

        // POST: Customer/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerEditViewModel model)
        {
            if (id != model.CustomerId) return NotFound();

            if (ModelState.IsValid)
            {
                var customer = await _customerRepository.GetByIdAsync(id);
                if (customer == null) return NotFound();

                customer.Name = model.Name;
                customer.Email = model.Email;
                customer.Phone = model.Phone;

                if (customer.Address == null)
                    customer.Address = new Address();

                customer.Address.Street = model.Address.Street;
                customer.Address.City = model.Address.City;
                customer.Address.ZipCode = model.Address.ZipCode;

                await _customerRepository.UpdateAsync(customer);
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Customer/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _customerRepository.GetByIdAsync(id.Value);
            if (customer == null) return NotFound();

            ViewBag.HasOrders = await _context.Orders.AnyAsync(o => o.CustomerId == id.Value);

            return View(customer);
        }

        // POST: Customer/DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return NotFound();

            await _customerRepository.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CustomerExists(int id)
        {
            return await _customerRepository.GetByIdAsync(id) != null;
        }
    }
}
