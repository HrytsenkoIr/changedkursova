using System.ComponentModel.DataAnnotations;

namespace OnlineStoreSystem.ViewModels
{
    public class CategoryCreateViewModel
    {
        [Required(ErrorMessage = "Назва категорії обов'язкова")]
        [StringLength(50, ErrorMessage = "Назва не може перевищувати 50 символів")]
        [Display(Name = "Назва категорії")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
        [Display(Name = "Опис (опціонально)")]
        public string? Description { get; set; }
    }

    public class CategoryEditViewModel : CategoryCreateViewModel
    {
        public int CategoryId { get; set; }

        [Display(Name = "Видалено")]
        public bool IsDeleted { get; set; }
    }
}