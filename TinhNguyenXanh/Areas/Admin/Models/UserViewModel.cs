namespace TinhNguyenXanh.Areas.Admin.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public bool IsLocked { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
