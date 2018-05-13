using System;
using System.Collections.Generic;
using System.Text;

namespace Drukarka3DData.Models
{
    public class FavouriteProjectsViewModel
    {
        public int NumberOfPages { get; set; }
        public int Page { get; set; }
        public int ElementsOnPage { get; set; }
        public string SortExpression { get; set; }
        public string Filter { get; set; }
        public string SortingOrder { get; set; }
        public IEnumerable<string> SortingOrderValues { get; set; }
        public IEnumerable<string> SortExpressionValues { get; set; }
        public IEnumerable<int> ElementsOnPageValues { get; set; }
        public IEnumerable<Order> Orders { get; set; }
    }
}
