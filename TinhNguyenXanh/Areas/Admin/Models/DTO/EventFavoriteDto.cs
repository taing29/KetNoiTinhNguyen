namespace TinhNguyenXanh.Areas.Admin.Models.DTO
{
    public class EventFavoriteDto
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int FavoriteCount { get; set; }
    }
}
