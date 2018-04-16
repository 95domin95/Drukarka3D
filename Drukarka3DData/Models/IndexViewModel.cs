using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Drukarka3DData.Models
{
    public class IndexViewModel
    {
        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Adress e-mail")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Numer telefonu")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Imię")]
        public string Name { get; set; }
        [Display(Name = "Nazwisko")]
        public string Surname { get; set; }
        [Display(Name = "Miejscowość")]
        public string City { get; set; }
        [Display(Name = "Kod pocztowy")]
        public string PostCode { get; set; }
        [Display(Name = "Ulica")]
        public string Street { get; set; }
        [Display(Name = "Numer domu")]
        public string ApartmentNumber { get; set; }

        public string StatusMessage { get; set; }
    }
}
