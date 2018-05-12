using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drukarka3DData.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string Street { get; set; }
        public string ApartmentNumber { get; set; }
        public virtual IEnumerable<Order> Orders { get; set; }
        public virtual IEnumerable<UserFavoriteProject> FavouriteProjects { get; set; }
    }
}
