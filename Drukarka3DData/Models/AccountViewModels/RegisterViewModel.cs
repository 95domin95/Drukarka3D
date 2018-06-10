using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Drukarka3DData.Models
{
    public class RegisterViewModel
    {
        [StringLength(50, ErrorMessage = "Zbyt długi login")]
        [Display(Name = "Login:")]
        [Required(ErrorMessage = "Musisz wprowadzić login"), MinLength(6), MaxLength(50)]
        public string Login { get; set; }

        [EmailAddress(ErrorMessage = "Niepoprawny adres e-mail")]
        [Required(ErrorMessage = "Musisz wprowadzić adres e-mail")]
        [Display(Name = "Adres e-mail:"), DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [StringLength(50, ErrorMessage = "Zbyt długie hasło")]
        [Required(ErrorMessage = "Musisz wprowadzić hasło"), DataType(DataType.Password)]
        [Display(Name = "Hasło:"), MinLength(6), MaxLength(50)]
        public string Password { get; set; }

        [DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Hasła muszą się zgadzać")]
        [Display(Name = "Potwierdź hasło:")]
        public string ConfirmPassword { get; set; }

    }
}
