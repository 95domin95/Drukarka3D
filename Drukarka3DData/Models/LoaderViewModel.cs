using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Drukarka3DData.Models
{
    public class LoaderViewModel
    {

        [Display(Name = "Projekt publiczny:")]
        public string IsPrivate { get; set; }

        [Required(ErrorMessage = "Nazwa projektu jest wymagana")]
        [RegularExpression("([a-zA-Z0-9ąćęłńóśźżĄĘŁŃÓŚŹŻ_. ]+)", ErrorMessage = "Nie poprawna nazwa projektu")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Zbyt krótka/długa nazwa projektu")]
        [Display(Name = "Nazwa projektu:")]
        public string ProjectName { get; set; }
        public IFormFile File { get; set; }
    }
}
