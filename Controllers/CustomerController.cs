using Microsoft.AspNetCore.Mvc;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories;
using OnlineStoreSystem.ViewModels;

namespace OnlineStoreSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
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
            var customers = await _customerRepository.GetFilteredAsync(
                name, email, phone, city, street, zip);

            return View(customers);
        }

        // GET: Customer/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _customerRepository.GetByIdWithDetailsAsync(id.Value);
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
        public async Task<IActionResult> Create(Customer customer)
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
                customer.Address = new Address
                {
                    Street = model.Address.Street,
                    City = model.Address.City,
                    ZipCode = model.Address.ZipCode
                };

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

            ViewBag.HasOrders = await _customerRepository.HasOrdersAsync(id.Value);

            return View(customer);
        }

        // POST: Customer/DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _customerRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
