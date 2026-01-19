using System.ComponentModel.DataAnnotations;

namespace OnlineStoreSystem.ViewModels
{
    public class ProductCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введіть назву товару")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Вкажіть ціну")]
        [Range(0.01, 1000000, ErrorMessage = "Ціна повинна бути більше 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Кількість не може бути відʼємною")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Оберіть категорію")]
        public int? CategoryId { get; set; }

    }
}
