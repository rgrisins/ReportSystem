using ReportSystem.Enums;
using System.ComponentModel.DataAnnotations;

namespace ReportSystem.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 5)]
        public string? Username { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [Display(Name = "Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$", ErrorMessage = "Password must have at least 1 uppercase, 1 lowercase, and 1 number")]
        public string? PasswordHash { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

    }
}
