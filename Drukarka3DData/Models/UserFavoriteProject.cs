using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Drukarka3DData.Models
{
    public class UserFavoriteProject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserFavoriteProjectId { get; set; }

        public bool IsRated { get; set; }
        public bool IsFavourite { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }
        public string UserId { get; set;}

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

    }
}
