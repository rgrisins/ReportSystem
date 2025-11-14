using Microsoft.AspNetCore.Mvc.Rendering;
using ReportSystem.Enums;
using System.ComponentModel.DataAnnotations;

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

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }
    }
}
