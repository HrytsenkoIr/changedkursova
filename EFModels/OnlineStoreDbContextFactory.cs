using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace OnlineStoreSystem.EFModels
{
    public class OnlineStoreDbContextFactory : IDesignTimeDbContextFactory<OnlineStoreDbContext>
    {
        public OnlineStoreDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<Program>() // якщо у тебе є UserSecrets
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? "Server=(local);Database=OnlineStore;User Id=AdminUser;Password=AdminPass123;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<OnlineStoreDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new OnlineStoreDbContext(optionsBuilder.Options);
        }
    }
}
