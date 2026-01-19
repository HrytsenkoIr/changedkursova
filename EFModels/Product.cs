using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnlineStoreSystem.EFModels;

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

    [Column("CategoryID")]
    public int? CategoryId { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column(TypeName = "numeric(12, 3)")]
    public decimal? DiscountedPrice { get; private set; }

    public bool IsDeleted { get; set; } = false;

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    public Dictionary<string, string>? Metadata { get; set; }


    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; } 

    [InverseProperty("Product")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
