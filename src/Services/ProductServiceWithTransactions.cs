using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OnlineStoreSystem.EFModels;

namespace OnlineStoreSystem.Services
{
    public class ProductServiceWithTransactions
    {
        private readonly OnlineStoreDbContext _context;
        private readonly string _connectionString;

        public ProductServiceWithTransactions(OnlineStoreDbContext context, string connectionString)
        {
            _context = context;
            _connectionString = connectionString;
        }

        // Неявна транзакція (Implicit)
        public async Task ImplicitTransactionAsync(CancellationToken ct = default)
        {
            try
            {
                var category = new Category { Name = "Нові товари" };
                _context.Categories.Add(category);

                var product = new Product
                {
                    Name = "Продукт A",
                    Price = 100,
                    Stock = 50,
                    Category = category
                };
                _context.Products.Add(product);

                await _context.SaveChangesAsync(ct);
                Console.WriteLine("Implicit transaction applied successfully.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"DbUpdateConcurrencyException: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DbUpdateException: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Other exception: {ex.Message}");
            }
        }

        //  Явна транзакція (Explicit)
        public async Task ExplicitTransactionAsync(CancellationToken ct = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var category = new Category { Name = "Категорія X" };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync(ct);

                var product = new Product
                {
                    Name = "Продукт X",
                    Price = 250,
                    Stock = 30,
                    CategoryId = category.CategoryId
                };
                _context.Products.Add(product);
                await _context.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);
                Console.WriteLine("Explicit transaction committed successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                Console.WriteLine($"Explicit transaction rolled back. Error: {ex.Message}");
            }
        }

        //  Інтеграція з ADO.NET 
        public async Task AdoNetTransactionAsync(CancellationToken ct = default)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var sqlTransaction = connection.BeginTransaction();
            try
            {
                await using (var cmd = new SqlCommand(
                           "UPDATE Product SET Price = Price * 1.1 WHERE Stock > @stock", connection, sqlTransaction))
                {
                    cmd.Parameters.AddWithValue("@stock", 10);
                    await cmd.ExecuteNonQueryAsync(ct);
                }

                var options = new DbContextOptionsBuilder<OnlineStoreDbContext>()
                    .UseSqlServer(connection)
                    .Options;

                await using var efContext = new OnlineStoreDbContext(options);

                await efContext.Database.UseTransactionAsync(sqlTransaction);

                var category = await efContext.Categories.FirstOrDefaultAsync(ct);
                if (category != null)
                {
                    category.Name += " (оновлено через EF)";
                    await efContext.SaveChangesAsync(ct);
                }

                await sqlTransaction.CommitAsync(ct);
                Console.WriteLine("ADO.NET + EF Core transaction committed successfully.");
            }
            catch (Exception ex)
            {
                await sqlTransaction.RollbackAsync(ct);
                Console.WriteLine($"ADO.NET + EF Core transaction rolled back. Error: {ex.Message}");
            }
        }
    }
}
