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
        public string Name { get; set; }
        public string Status { get; set; }
        public string UploadDate { get; set; }
        public string Path { get; set; }
        public ApplicationUser User { get; set; }
        public File File { get; set; }
    }
}
