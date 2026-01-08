using System.ComponentModel.DataAnnotations;

namespace ReportSystem.Models
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(30, MinimumLength = 5)]
        public string? Username { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$", ErrorMessage = "Password must have at least 1 uppercase, 1 lowercase, and 1 number")]
        public string? Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
    }
}