using Microsoft.Data.SqlClient;
using OnlineStore.Models;
using OnlineStore.Services;

namespace OnlineStore.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly DatabaseConnection _db;

    public CustomerRepository(DatabaseConnection db) => _db = db;

    public async Task<int> CreateAsync(Customer c, CancellationToken ct = default)
    {
        const string sql = "INSERT INTO Customer (Name, Email, Phone, Address) VALUES (@Name, @Email, @Phone, @Address); SELECT CAST(SCOPE_IDENTITY() AS INT);";
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", c.Name);
        cmd.Parameters.AddWithValue("@Email", c.Email);
        cmd.Parameters.AddWithValue("@Phone", c.Phone);
        cmd.Parameters.AddWithValue("@Address", c.Address);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = "SELECT CustomerID, Name, Email, Phone, Address FROM Customer WHERE CustomerID = @id";
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (await r.ReadAsync(ct))
            return new Customer
            {
                CustomerID = r.GetInt32(0),
                Name = r.GetString(1),
                Email = r.GetString(2),
                Phone = r.GetString(3),
                Address = r.GetString(4)
            };
        return null;
    }

    public async Task<List<Customer>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT CustomerID, Name, Email, Phone, Address FROM Customer";
        var list = new List<Customer>();
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add(new Customer
            {
                CustomerID = r.GetInt32(0),
                Name = r.GetString(1),
                Email = r.GetString(2),
                Phone = r.GetString(3),
                Address = r.GetString(4)
            });
        return list;
    }

    public async Task<bool> UpdateAsync(Customer c, CancellationToken ct = default)
    {
        const string sql = "UPDATE Customer SET Name = @Name, Email = @Email, Phone = @Phone, Address = @Address WHERE CustomerID = @id";
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", c.CustomerID);
        cmd.Parameters.AddWithValue("@Name", c.Name);
        cmd.Parameters.AddWithValue("@Email", c.Email);
        cmd.Parameters.AddWithValue("@Phone", c.Phone);
        cmd.Parameters.AddWithValue("@Address", c.Address);
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM Customer WHERE CustomerID = @id";
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }
}

