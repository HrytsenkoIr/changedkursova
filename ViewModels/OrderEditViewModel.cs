using System.ComponentModel.DataAnnotations;

namespace OnlineStoreSystem.ViewModels
{
    public class OrderEditViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Оберіть клієнта")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Вкажіть дату")]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Оберіть статус")]
        public string Status { get; set; } = string.Empty;

        public string DeliveryType { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public List<OrderItemViewModel> ExistingOrderItems { get; set; } = new();

        public List<OrderItemViewModel> NewOrderItems { get; set; } = new();
    }
}
