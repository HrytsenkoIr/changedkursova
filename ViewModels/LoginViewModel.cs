using System.ComponentModel.DataAnnotations;

namespace OnlineStoreSystem.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Оберіть тип користувача")]
        [Display(Name = "Тип користувача")]
        public string UserType { get; set; } = "AdminUser";

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = "";

        [Display(Name = "Запам'ятати мене")]
        public bool RememberMe { get; set; }
    }
}