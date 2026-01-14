using Microsoft.Data.SqlClient;
using OnlineStore.Models;
using OnlineStore.Services;

namespace OnlineStore.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly DatabaseConnection _db;

    public OrderRepository(DatabaseConnection db) => _db = db;

    public async Task<int> CreateOrderWithTransactionAsync(Order o, List<OrderItem> items, CancellationToken ct = default)
    {
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);
        try
        {

            const string orderSql = "INSERT INTO Orders (CustomerID, OrderDate, Status) VALUES (@cid, GETDATE(), @status); SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int orderId;
            await using (var cmd = new SqlCommand(orderSql, conn, (SqlTransaction)tx))
            {
                cmd.Parameters.AddWithValue("@cid", o.CustomerID);
                cmd.Parameters.AddWithValue("@status", o.Status);
                orderId = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            }

            const string itemSql = "INSERT INTO OrderItem (OrderID, ProductID, Amount, Price) VALUES (@oid, @pid, @amt, @price)";
            foreach (var item in items)
            {
                await using var cmd = new SqlCommand(itemSql, conn, (SqlTransaction)tx);
                cmd.Parameters.AddWithValue("@oid", orderId);
                cmd.Parameters.AddWithValue("@pid", item.ProductID);
                cmd.Parameters.AddWithValue("@amt", item.Amount);
                cmd.Parameters.AddWithValue("@price", item.Price);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            const string stockSql = "UPDATE Product SET Stock = Stock - 1 WHERE ProductID = @pid AND Stock >= 1";
            foreach (var item in items)
            {
                await using var cmd = new SqlCommand(stockSql, conn, (SqlTransaction)tx);
                cmd.Parameters.AddWithValue("@pid", item.ProductID);
                if (await cmd.ExecuteNonQueryAsync(ct) == 0)
                    throw new InvalidOperationException($"Insufficient stock for product {item.ProductID}");
            }

            await tx.CommitAsync(ct);
            return orderId;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
