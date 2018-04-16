using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Drukarka3DData.Models
{
    public class LoginViewModel
    {
        [StringLength(50)]
        [Display(Name = "Email:")]
        [Required(ErrorMessage = "Musisz wprowadzić login"), MinLength(6), MaxLength(50)]
        public string Login { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Musisz wprowadzić hasło"), DataType(DataType.Password)]
        [Display(Name = "Hasło:"), MinLength(6), MaxLength(50)]
        public string Password { get; set; }

        [Display(Name = "Zamamiętaj mnie")]
        public bool RememberMe { get; set; }

    }
}
