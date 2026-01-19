using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnlineStoreSystem.EFModels;

[Table("Customer")]
[Index("Email", Name = "IX_Customer_Email")]
[Index("Email", Name = "UQ_Customer_Email", IsUnique = true)]
public partial class Customer
{
    [Key]
    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(20)]
    public string Phone { get; set; } = null!;

    public Address Address { get; set; } = new Address();

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [InverseProperty("Customer")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
