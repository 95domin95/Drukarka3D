using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Drukarka3DData.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }
        public int ViewsCount { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Status { get; set; }
        public string UserScreenPath { get; set; }
        public double Rate { get; set; }
        public bool Private { get; set; }
        public int RatingsCount { get; set; }
        public double RatingsSum { get; set; }
        //[Required]
        //public double AverageUsersRate { get; set; }
        public DateTime UploadDate { get; set; }
        public ApplicationUser User { get; set; }
    }
}
