using System.ComponentModel.DataAnnotations;

namespace OnlineStoreSystem.ViewModels
{
    public class CategoryCreateViewModel
    {
        [Required(ErrorMessage = "Назва категорії обов'язкова")]
        [StringLength(50, ErrorMessage = "Назва не може перевищувати 50 символів")]
        [Display(Name = "Назва категорії")]
        public string Name { get; set; } = string.Empty;

    }

    public class CategoryEditViewModel : CategoryCreateViewModel
    {
        public int CategoryId { get; set; }

    }
}