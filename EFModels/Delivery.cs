using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnlineStoreSystem.EFModels;

[Table("Delivery")]
[Index("OrderId", Name = "IX_Delivery_OrderID")]
public partial class Delivery
{
    [Key]
    [Column("DeliveryID")]
    public int DeliveryId { get; set; }

    [Column("OrderID")]
    public int OrderId { get; set; }

    [StringLength(50)]
    public string Type { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Cost { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("Deliveries")]
    public virtual Order Order { get; set; } = null!;
}
