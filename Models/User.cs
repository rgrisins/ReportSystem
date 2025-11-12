using Microsoft.AspNetCore.Identity;
using ReportSystem.Enums;

namespace ReportSystem.Models
{
    public class User : IdentityUser
    {
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public UserRole Role { get; set; } = UserRole.User;
    }
}
