using Microsoft.Data.SqlClient;
using OnlineStore.Models;
using OnlineStore.Services;

namespace OnlineStore.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DatabaseConnection _db;

    public ProductRepository(DatabaseConnection db) => _db = db;

    public async Task<int> CreateAsync(Product p, CancellationToken ct = default)
    {
        const string sql = "INSERT INTO Product (Name, Description, Price, Stock, CategoryID) VALUES (@Name, @Desc, @Price, @Stock, @Cat); SELECT CAST(SCOPE_IDENTITY() AS INT);";
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", p.Name);
        cmd.Parameters.AddWithValue("@Desc", (object?)p.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Price", p.Price);
        cmd.Parameters.AddWithValue("@Stock", p.Stock);
        cmd.Parameters.AddWithValue("@Cat", p.CategoryID);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = "SELECT ProductID, Name, Description, Price, Stock, CategoryID FROM Product WHERE ProductID = @id";
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (await r.ReadAsync(ct))
            return new Product
            {
                ProductID = r.GetInt32(0),
                Name = r.GetString(1),
                Description = r.IsDBNull(2) ? null : r.GetString(2),
                Price = r.GetDecimal(3),
                Stock = r.GetInt32(4),
                CategoryID = r.GetInt32(5)
            };
        return null;
    }

    public async Task<List<Product>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT ProductID, Name, Description, Price, Stock, CategoryID FROM Product";
        var list = new List<Product>();
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add(new Product
            {
                ProductID = r.GetInt32(0),
                Name = r.GetString(1),
                Description = r.IsDBNull(2) ? null : r.GetString(2),
                Price = r.GetDecimal(3),
                Stock = r.GetInt32(4),
                CategoryID = r.GetInt32(5)
            });
        return list;
    }

    public async Task<List<Product>> GetWithCategoriesAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT p.ProductID, p.Name, p.Description, p.Price, p.Stock, p.CategoryID, c.Name FROM Product p INNER JOIN Category c ON p.CategoryID = c.CategoryID";
        var list = new List<Product>();
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            list.Add(new Product
            {
                ProductID = r.GetInt32(0),
                Name = r.GetString(1),
                Description = r.IsDBNull(2) ? null : r.GetString(2),
                Price = r.GetDecimal(3),
                Stock = r.GetInt32(4),
                CategoryID = r.GetInt32(5),
                CategoryName = r.GetString(6)
            });
        return list;
    }
}