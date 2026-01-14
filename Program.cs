using Microsoft.Extensions.Configuration;
using OnlineStore.Models;
using OnlineStore.Repositories;
using OnlineStore.Services;

namespace OnlineStore;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Witch Store ADO.NET Application\n");

        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();

            var db = new DatabaseConnection(config);

            Console.WriteLine("Testing database connection...");
            await using (var conn = db.CreateConnection())
            {
                await conn.OpenAsync();
                Console.WriteLine($"Connected to database: {conn.Database}\n");
            }

            Console.WriteLine("TASK 1: Running Migrations");
            var migPath = config["MigrationsPath"] ?? "migrations";
            var migRunner = new MigrationRunner(db, migPath);
            await migRunner.RunMigrationsAsync();
            Console.WriteLine();

            Console.WriteLine("TASK 2: CRUD Operations");
            await DemoCrudAsync(db);
            Console.WriteLine();

            Console.WriteLine("TASK 3: Complex Queries and Pagination");
            await DemoComplexAsync(db);
            Console.WriteLine();

            Console.WriteLine("TASK 4: Transactions");
            await DemoTransactionsAsync(db);
            Console.WriteLine();

            Console.WriteLine("TASK 5: Stored Procedures");
            await DemoStoredProceduresAsync(db);
            Console.WriteLine();

//             Console.WriteLine("TASK 6: Bulk Insert Test");
// await DemoBulkInsertAsync(db);
// Console.WriteLine();





            Console.WriteLine("\nAll tasks completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static async Task DemoCrudAsync(DatabaseConnection db)
    {
        var repo = new CustomerRepository(db);
        
        Console.WriteLine("Creating new customer...");

        var uniqueEmail = $"test{DateTime.Now.Ticks % 100000}@example.com";
        var c = new Customer
        {
            Name = "Test Test",
            Email = uniqueEmail,
            Phone = "+380501111111",
            Address = "Kyiv, Test St, 1"
        };
        
        var id = await repo.CreateAsync(c);
        Console.WriteLine($"Customer created with ID: {id}, Email: {uniqueEmail}");

        Console.WriteLine("\nReading customer by ID...");
        var customer = await repo.GetByIdAsync(id);
        Console.WriteLine($"Customer found: {customer?.Name}, Email: {customer?.Email}");

        Console.WriteLine("\nUpdating customer...");
        customer!.Phone = "+380502222222";
        await repo.UpdateAsync(customer);
        Console.WriteLine("Customer updated successfully");

        Console.WriteLine("\nGetting all customers...");
        var all = await repo.GetAllAsync();
        Console.WriteLine($"Total customers in database: {all.Count}");

        Console.WriteLine("\nDeleting test customer...");
        await repo.DeleteAsync(id);
        Console.WriteLine("Customer deleted successfully");
    }

    static async Task DemoComplexAsync(DatabaseConnection db)
    {
        var repo = new ProductRepository(db);
        Console.WriteLine("Executing JOIN query (Products with Categories)...");
        var products = await repo.GetWithCategoriesAsync();
        Console.WriteLine($"Retrieved {products.Count} products with category information");
        Console.WriteLine("\nTop 5 products:");
        foreach (var p in products.Take(5))
            Console.WriteLine($"  - {p.Name} (Category: {p.CategoryName}), Price: {p.Price} UAH, Stock: {p.Stock}");
    }

    static async Task DemoTransactionsAsync(DatabaseConnection db)
    {
        var repo = new OrderRepository(db);
        var prodRepo = new ProductRepository(db);
        Console.WriteLine("Demonstrating transaction (Creating order with stock update)...");
        var product = await prodRepo.GetByIdAsync(1);
        if (product != null)
        {
            Console.WriteLine($"\nProduct: {product.Name}");
            Console.WriteLine($"Stock before order: {product.Stock}");
            var order = new Order { CustomerID = 1, Status = "Pending", TotalAmount = product.Price * 2 };
            var items = new List<OrderItem> { new OrderItem { ProductID = 1, Amount = 2, Price = product.Price } };
            try
            {
                var orderId = await repo.CreateOrderWithTransactionAsync(order, items);
                Console.WriteLine($"Order created successfully with ID: {orderId}");
                product = await prodRepo.GetByIdAsync(1);
                Console.WriteLine($"Stock after order: {product!.Stock}");
                Console.WriteLine("Transaction completed - all changes committed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transaction failed: {ex.Message}");
                Console.WriteLine("All changes have been rolled back!");
            }
        }
    }

    static async Task DemoStoredProceduresAsync(DatabaseConnection db)
    {
        var sp = new StoredProcedureService(db);
        Console.WriteLine("Calling stored procedure: sp_PlaceOrder...");
        try
        {
            var (ret, oid) = await sp.PlaceOrderAsync(1, 2, 1, "Courier");
            if (ret == 0)
                Console.WriteLine($"Order placed successfully, Order ID: {oid}");
            else
                Console.WriteLine($"Order placement failed with return code: {ret}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling stored procedure: {ex.Message}");
        }

        Console.WriteLine("\nCalling stored procedure: sp_GetBestSellers...");
        var best = await sp.GetBestSellersAsync(5);
        Console.WriteLine($"Top {best.Rows.Count} best-selling products:");
        foreach (System.Data.DataRow row in best.Rows)
            Console.WriteLine($"  - {row["Name"]}: {row["TotalSold"]} sold, Revenue: {row["TotalRevenue"]} UAH");
    }
//     static async Task DemoBulkInsertAsync(DatabaseConnection db)
// {
//     var bulkService = new BulkInsertService(db);
//     var customers = bulkService.GenerateTestCustomers(1000);

//     Console.WriteLine("\n--- Bulk Insert Performance Test ---");

//     // INSERT в циклі
//     await bulkService.InsertLoopAsync(customers);

//     // Batched INSERT
//     await bulkService.InsertBatchedAsync(customers, 50);

//     // SqlBulkCopy
//     await bulkService.InsertBulkAsync(customers, 100);
// }


}
