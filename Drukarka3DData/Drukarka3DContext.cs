using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Drukarka3DData.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Drukarka3DData
{
    public class Drukarka3DContext : IdentityDbContext<ApplicationUser>
    {
        public Drukarka3DContext()
        {

        }

        public Drukarka3DContext(DbContextOptions options) : base(options) { }

        //public DbSet<User> User { get; set; }
        //public DbSet<File> File { get; set; }
        public DbSet<Order> Order { get; set; }
    }
}
