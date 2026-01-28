using TinhNguyenXanh.DTOs;

namespace TinhNguyenXanh.Models.ViewModel
{
    public class EventCommentsViewModel
    {
        public int EventId { get; set; }
        public List<EventComment> Comments { get; set; } = new();
        public EventCommentDTO NewComment { get; set; } = new();
    }
}
