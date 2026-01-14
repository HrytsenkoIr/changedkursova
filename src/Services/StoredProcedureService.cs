using System.Data;
using Microsoft.Data.SqlClient;
using OnlineStore.Services;

namespace OnlineStore.Services;

public class StoredProcedureService
{
    private readonly DatabaseConnection _dbConnection;

    public StoredProcedureService(DatabaseConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<(int returnCode, int orderId)> PlaceOrderAsync(int customerId, int productId, int amount, string deliveryType, CancellationToken ct = default)
    {
        await using var conn = _dbConnection.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("sp_PlaceOrder", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@CustomerID", customerId);
        cmd.Parameters.AddWithValue("@ProductID", productId);
        cmd.Parameters.AddWithValue("@Amount", amount);
        cmd.Parameters.AddWithValue("@DeliveryType", deliveryType);
        var orderIdParam = new SqlParameter("@NewOrderID", SqlDbType.Int) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(orderIdParam);
        var returnParam = new SqlParameter { Direction = ParameterDirection.ReturnValue };
        cmd.Parameters.Add(returnParam);
        await cmd.ExecuteNonQueryAsync(ct);
        return ((int)returnParam.Value, orderIdParam.Value != DBNull.Value ? (int)orderIdParam.Value : 0);
    }

    public async Task<DataTable> GetBestSellersAsync(int topCount = 5, CancellationToken ct = default)
    {
        await using var conn = _dbConnection.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("sp_GetBestSellers", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@TopCount", topCount);
        var dt = new DataTable();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        dt.Load(reader);
        return dt;
    }
}