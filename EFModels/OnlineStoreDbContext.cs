using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace OnlineStoreSystem.EFModels;

public partial class OnlineStoreDbContext : DbContext
{
    public OnlineStoreDbContext() { }

    public OnlineStoreDbContext(DbContextOptions<OnlineStoreDbContext> options)
        : base(options) { }

    public virtual DbSet<Category> Categories { get; set; } = null!;
    public virtual DbSet<Customer> Customers { get; set; } = null!;
    public virtual DbSet<Delivery> Deliveries { get; set; } = null!;
    public virtual DbSet<MigrationHistory> MigrationHistories { get; set; } = null!;
    public virtual DbSet<Order> Orders { get; set; } = null!;
    public virtual DbSet<OrderItem> OrderItems { get; set; } = null!;
    public virtual DbSet<Payment> Payments { get; set; } = null!;
    public virtual DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // =========================================================
        // JSON OPTIONS (shared)
        // =========================================================
        var jsonOptions = new JsonSerializerOptions();

        // =========================================================
        // PRODUCT CONFIG
        // =========================================================
        modelBuilder.Entity<Product>(entity =>
        {
            // ENUM -> STRING
            entity.Property(p => p.Status)
                  .HasConversion<string>();

            // JSON Metadata
            entity.Property(p => p.Metadata)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, jsonOptions),
                      v => JsonSerializer.Deserialize<Dictionary<string, string>>(v!, jsonOptions)!
                  );

            entity.Property(p => p.Metadata)
                  .Metadata.SetValueComparer(
                      new ValueComparer<Dictionary<string, string>>(
                          (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                          c => c == null
                              ? 0
                              : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                          c => c == null
                              ? new Dictionary<string, string>()
                              : c.ToDictionary(e => e.Key, e => e.Value)
                      )
                  );

            // Computed column
            entity.Property(p => p.DiscountedPrice)
                  .HasComputedColumnSql("([Price] * (0.9))", stored: true);

            // ===== CATEGORY RELATION =====
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            // ===== ORDER ITEMS =====
            entity.HasMany(p => p.OrderItems)
                  .WithOne(oi => oi.Product)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================================
        // CUSTOMER CONFIG
        // =========================================================
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.OwnsOne(c => c.Address);

            entity.ToTable("Customer", tb =>
                tb.HasTrigger("trg_PreventDeleteCustomerWithOrders"));
        });

        // =========================================================
        // ORDER CONFIG
        // =========================================================
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_Order_Base"));

            entity.Property(o => o.Status)
                  .HasDefaultValue("Pending");
        });

        // =========================================================
        // ORDER ITEM CONFIG
        // =========================================================
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_OrderItem_Base"));

            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================================================
        // PAYMENT CONFIG
        // =========================================================
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_Payment_Base"));

            entity.Property(p => p.PaymentDate)
                  .HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // =========================================================
        // DELIVERY CONFIG
        // =========================================================
        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.ToTable(tb => tb.HasTrigger("trg_Delivery_Base"));

            entity.Property(d => d.Status)
                  .HasDefaultValue("Processing");
        });

        // =========================================================
        // SPLIT QUERY / NAVIGATION OPTIMIZATION
        // =========================================================
        modelBuilder.Entity<Order>()
            .Navigation(o => o.OrderItems)
            .UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);

        modelBuilder.Entity<Order>()
            .Navigation(o => o.Payments)
            .UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);

        modelBuilder.Entity<Order>()
            .Navigation(o => o.Deliveries)
            .UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
