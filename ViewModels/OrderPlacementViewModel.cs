using System.ComponentModel.DataAnnotations;

namespace OnlineStoreSystem.ViewModels
{
    public class OrderPlacementViewModel
    {
        [Required(ErrorMessage = "Оберіть клієнта")]
        [Display(Name = "Клієнт")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Оберіть товар")]
        [Display(Name = "Товар")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Вкажіть кількість")]
        [Range(1, 100, ErrorMessage = "Кількість має бути від 1 до 100")]
        [Display(Name = "Кількість")]
        public int Amount { get; set; }

        [Required(ErrorMessage = "Оберіть тип доставки")]
        [Display(Name = "Тип доставки")]
        public string DeliveryType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Оберіть тип оплати")]
        [Display(Name = "Тип оплати")]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
