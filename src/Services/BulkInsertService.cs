using Microsoft.Data.SqlClient;
using OnlineStore.Models;
using System.Diagnostics;
using System.Text;

namespace OnlineStore.Services;

public class BulkInsertService
{
    private readonly DatabaseConnection _db;

    public BulkInsertService(DatabaseConnection db)
    {
        _db = db;
    }

    public List<Customer> GenerateTestCustomers(int count = 1000)
    {
        var list = new List<Customer>();
        for (int i = 0; i < count; i++)
        {
            list.Add(new Customer
            {
                Name = $"Customer {i+1}",
                Email = $"customer{i+1}@example.com",
                Phone = $"+38050{i%1000:0000000}",
                Address = $"Kyiv, Street {i+1}"
            });
        }
        return list;
    }

    public async Task InsertLoopAsync(List<Customer> customers)
    {
        var sw = Stopwatch.StartNew();
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        foreach (var c in customers)
        {
            var cmd = new SqlCommand(
                "INSERT INTO Customer (Name, Email, Phone, Address) VALUES (@Name, @Email, @Phone, @Address)",
                conn);
            cmd.Parameters.AddWithValue("@Name", c.Name);
            cmd.Parameters.AddWithValue("@Email", c.Email);
            cmd.Parameters.AddWithValue("@Phone", c.Phone);
            cmd.Parameters.AddWithValue("@Address", c.Address);
            await cmd.ExecuteNonQueryAsync();
        }

        sw.Stop();
        Console.WriteLine($"INSERT in loop: {sw.ElapsedMilliseconds} ms");
    }

    public async Task InsertBatchedAsync(List<Customer> customers, int batchSize = 50)
    {
        var sw = Stopwatch.StartNew();
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        for (int i = 0; i < customers.Count; i += batchSize)
        {
            var batch = customers.Skip(i).Take(batchSize).ToList();
            var sb = new StringBuilder("INSERT INTO Customer (Name, Email, Phone, Address) VALUES ");
            var parameters = new List<SqlParameter>();

            for (int j = 0; j < batch.Count; j++)
            {
                sb.Append($"(@Name{j}, @Email{j}, @Phone{j}, @Address{j}),");
                parameters.Add(new SqlParameter($"@Name{j}", batch[j].Name));
                parameters.Add(new SqlParameter($"@Email{j}", batch[j].Email));
                parameters.Add(new SqlParameter($"@Phone{j}", batch[j].Phone));
                parameters.Add(new SqlParameter($"@Address{j}", batch[j].Address));
            }

            sb.Length--; // Remove last comma
            var cmd = new SqlCommand(sb.ToString(), conn);
            cmd.Parameters.AddRange(parameters.ToArray());
            await cmd.ExecuteNonQueryAsync();
        }

        sw.Stop();
        Console.WriteLine($"Batched INSERT ({batchSize}): {sw.ElapsedMilliseconds} ms");
    }

    public async Task InsertBulkAsync(List<Customer> customers, int batchSize = 100)
    {
        var sw = Stopwatch.StartNew();
        await using var conn = _db.CreateConnection();
        await conn.OpenAsync();

        var table = new System.Data.DataTable();
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("Phone", typeof(string));
        table.Columns.Add("Address", typeof(string));

        foreach (var c in customers)
        {
            table.Rows.Add(c.Name, c.Email, c.Phone, c.Address);
        }

        using var bulk = new SqlBulkCopy(conn)
        {
            DestinationTableName = "Customer",
            BatchSize = batchSize
        };
        bulk.ColumnMappings.Add("Name", "Name");
        bulk.ColumnMappings.Add("Email", "Email");
        bulk.ColumnMappings.Add("Phone", "Phone");
        bulk.ColumnMappings.Add("Address", "Address");

        await bulk.WriteToServerAsync(table);

        sw.Stop();
        Console.WriteLine($"SqlBulkCopy (BatchSize={batchSize}): {sw.ElapsedMilliseconds} ms");
    }
}
