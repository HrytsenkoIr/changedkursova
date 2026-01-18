using Microsoft.EntityFrameworkCore;

namespace OnlineStoreSystem.EFModels;

[Owned]
public class Address
{
    public string? Street { get; set; }
public string? City { get; set; }
public string? ZipCode { get; set; }

}
