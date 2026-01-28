using System.Collections.Generic;

namespace TinhNguyenXanh.Models.ViewModel
{
    public class SearchResultsViewModel
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public string? Location { get; set; }

        public List<Event> Events { get; set; } = new();
        public List<Organization> Organizations { get; set; } = new();
    }
}
