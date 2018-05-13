using System;
using System.Collections.Generic;
using System.Text;

namespace Drukarka3DData.Models
{
    public class OrderViewModel
    {
        public int LikesCount { get; set; }
        public string SearchString { get; set; }
        public string SortingOrder { get; set; }
        public string SortingType { get; set; }
        public int PageNumber { get; set; }
        public bool IsRated { get; set; }
        public bool IsLiked { get; set; }
        public bool IsSignedIn { get; set; }
        public bool IsProjectOwner { get; set; }
        public int NumberOfResolutsInPage { get; set; }
        public virtual IEnumerable<Order> Order { get; set; }
    }
}
