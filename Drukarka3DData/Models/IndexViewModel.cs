using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Drukarka3DData.Models
{
    public class IndexViewModel
    {
        [Display(Name = "Adress e-mail")]
        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [Required(ErrorMessage = "Pole adress e-mail jest wymagane")]
        [EmailAddress]
        [Display(Name = "Adress e-mail")]
        public string Email { get; set; }

        [StringLength(9, ErrorMessage = "Niepoprawny numer telefonu")]
        [Phone(ErrorMessage = "Niepoprawny numer telefonu")]
        [Required(ErrorMessage = "Pole numer telefonu jest wymagane")]
        [Display(Name = "Numer telefonu")]
        public string PhoneNumber { get; set; }

        [RegularExpression("([a-zA-Z]+)", ErrorMessage = "Niepoprawne imię")]
        [Required(ErrorMessage = "Pole imię jest wymagane")]
        [Display(Name = "Imię")]
        public string Name { get; set; }

        [RegularExpression("([a-zA-Z]+)", ErrorMessage = "Niepoprawne nazwisko")]
        [Required(ErrorMessage = "Pole nazwisko jest wymagane")]
        [Display(Name = "Nazwisko")]
        public string Surname { get; set; }

        [RegularExpression("([a-zA-Z]+)", ErrorMessage = "Niepoprawna nazwa miejscowości")]
        [Required(ErrorMessage = "Pole miejscowość jest wymagane")]
        [Display(Name = "Miejscowość")]
        public string City { get; set; }

        [Required(ErrorMessage = "Pole kod pocztowy jest wymagane")]
        [Display(Name = "Kod pocztowy")]
        public string PostCode { get; set; }

        [Required(ErrorMessage = "Pole ulica jest wymagane")]
        [Display(Name = "Ulica")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Pole numer domu jest wymagane")]
        [Display(Name = "Numer domu")]
        public string ApartmentNumber { get; set; }

        public string StatusMessage { get; set; }
    }
}
