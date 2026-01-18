using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnlineStoreSystem.EFModels;

// ===== ENUM ДЛЯ СТАТУСУ =====
public enum ProductStatus
{
    Active,
    Available,
    OutOfStock,
    Discontinued
}

[Table("Product")]
[Index("CategoryId", Name = "IX_Product_CategoryID")]
[Index("Name", Name = "IX_Product_Name")]
public partial class Product
{
    [Key]
    [Column("ProductID")]
    public int ProductId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    public int Stock { get; set; }

    // ❗ FK МОЖЕ БУТИ NULL
    [Column("CategoryID")]
    public int? CategoryId { get; set; }

    // ===== COMPUTED COLUMN =====
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column(TypeName = "numeric(12, 3)")]
    public decimal? DiscountedPrice { get; private set; }

    // ===== SOFT DELETE =====
    public bool IsDeleted { get; set; } = false;

    // ===== ENUM → STRING =====
    public ProductStatus Status { get; set; } = ProductStatus.Active;

    // ===== JSON METADATA =====
    public Dictionary<string, string>? Metadata { get; set; }

    // ===== NAVIGATION PROPERTIES =====
    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; }   // ✅ nullable

    [InverseProperty("Product")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
