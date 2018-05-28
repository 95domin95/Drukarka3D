using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Drukarka3DData.Models
{
    public class AdminPanelViewModel
    {
        [Display(Name = "Nazwa Pliku:")]
        public string FileName { get; set; }
        [Display(Name = "Informacja zwrotna:")]
        public string Message { get; set; }
    }
}
