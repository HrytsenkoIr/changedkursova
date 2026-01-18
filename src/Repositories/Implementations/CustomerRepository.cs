using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;
using OnlineStoreSystem.Repositories.Interfaces;

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

    public async Task<List<Customer>> GetAllAsync(bool trackChanges = false, CancellationToken ct = default)
    {
        var query = _context.Customers.AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();
        return await query.ToListAsync(ct);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        var existing = await _context.Customers.FindAsync(new object[] { customer.CustomerId }, ct);
        if (existing == null)
            throw new InvalidOperationException("Customer not found");

        existing.Name = customer.Name;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        existing.Address = customer.Address;

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM Customer WHERE CustomerID = {0}", new object[] { id }, ct);
    }
}
