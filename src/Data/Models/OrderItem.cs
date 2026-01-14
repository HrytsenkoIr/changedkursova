namespace OnlineStore.Models;

public class OrderItem
{
    public int OrderItemID { get; set; }
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public string? ProductName { get; set; }
}