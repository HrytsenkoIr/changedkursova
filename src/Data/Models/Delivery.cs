namespace OnlineStore.Models;

public class Delivery
{
    public int DeliveryID { get; set; }
    public int OrderID { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string? Status { get; set; }
    public DateTime? DeliveryDate { get; set; }
}