using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OnlineStoreSystem.EFModels;

[Table("__MigrationHistory")]
[Index("MigrationName", "MigrationHash", Name = "UQ_Migration", IsUnique = true)]
public partial class MigrationHistory
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string MigrationName { get; set; } = null!;

    [StringLength(64)]
    public string MigrationHash { get; set; } = null!;

    public DateTime AppliedAt { get; set; }
}
