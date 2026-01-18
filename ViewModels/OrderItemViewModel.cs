using System.ComponentModel.DataAnnotations;

namespace OnlineStoreSystem.ViewModels
{
    public class OrderItemViewModel
    {
        public int ProductId { get; set; }

        public int Amount { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal Price { get; set; }
    }
}
