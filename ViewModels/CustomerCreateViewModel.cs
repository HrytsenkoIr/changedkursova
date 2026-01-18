using System.ComponentModel.DataAnnotations;

namespace OnlineStoreSystem.ViewModels
{
    public class CustomerCreateViewModel
    {
        [Required(ErrorMessage = "Ім'я обов'язкове")]
        [StringLength(100, ErrorMessage = "Ім'я не може перевищувати 100 символів")]
        [Display(Name = "ПІБ")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обов'язковий")]
        [EmailAddress(ErrorMessage = "Невірний формат Email")]
        [StringLength(100, ErrorMessage = "Email не може перевищувати 100 символів")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефон обов'язковий")]
        [Phone(ErrorMessage = "Невірний формат телефону")]
        [StringLength(20, ErrorMessage = "Телефон не може перевищувати 20 символів")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Адреса")]
        public AddressViewModel Address { get; set; } = new AddressViewModel();
    }

    public class CustomerEditViewModel : CustomerCreateViewModel
    {
        public int CustomerId { get; set; }
    }

    public class AddressViewModel
    {
        [StringLength(200, ErrorMessage = "Вулиця не може перевищувати 200 символів")]
        [Display(Name = "Вулиця")]
        public string Street { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Місто не може перевищувати 100 символів")]
        [Display(Name = "Місто")]
        public string City { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Поштовий код не може перевищувати 20 символів")]
        [Display(Name = "Поштовий код")]
        public string ZipCode { get; set; } = string.Empty;
    }
}