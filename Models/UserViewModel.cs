using Microsoft.AspNetCore.Mvc.Rendering;
using ReportSystem.Enums;

namespace ReportSystem.Models
{
    public class UserViewModel
    {
        public List<User>? Users { get; set; }
        public SelectList? Roles { get; set; }
        public Dictionary<UserRole, int> RoleCounts { get; set; } = new();
        public int TotalCount { get; set; }
        public string? UserRole { get; set; }
        public string? SearchString { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
