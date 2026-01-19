using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;

namespace OnlineStoreSystem.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly OnlineStoreDbContext _context;

    public CustomerRepository(OnlineStoreDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> CreateAsync(Customer customer, CancellationToken ct = default)
    {
        await _context.Customers.AddAsync(customer, ct);
        await _context.SaveChangesAsync(ct);
        return customer;
    }

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Customers.FindAsync(new object[] { id }, ct);
    }

    public async Task<Customer?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Customers
            .Include(c => c.Address)
            .Include(c => c.Orders)
                .ThenInclude(o => o.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == id, ct);
    }

    public async Task<List<Customer>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default)
    {
        var query = _context.Customers.AsQueryable();
        if (!trackChanges)
            query = query.AsNoTracking();

        return await query.ToListAsync(ct);
    }

    // фільтрація з Index
    public async Task<List<Customer>> GetFilteredAsync(
        string? name,
        string? email,
        string? phone,
        string? city,
        string? street,
        string? zip,
        CancellationToken ct = default)
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

        return await query.ToListAsync(ct);
    }

    public async Task<bool> HasOrdersAsync(int customerId, CancellationToken ct = default)
    {
        return await _context.Orders.AnyAsync(o => o.CustomerId == customerId, ct);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        var existing = await _context.Customers
            .Include(c => c.Address)
            .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId, ct);

        if (existing == null)
            throw new InvalidOperationException("Customer not found");

        existing.Name = customer.Name;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;

        if (existing.Address == null)
            existing.Address = new Address();

        existing.Address.Street = customer.Address?.Street;
        existing.Address.City = customer.Address?.City;
        existing.Address.ZipCode = customer.Address?.ZipCode;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM Customer WHERE CustomerID = {0}",
            new object[] { id },
            ct);
    }
}
