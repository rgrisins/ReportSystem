using Microsoft.AspNetCore.Identity;
using ReportSystem.Enums;
using ReportSystem.Validation;
using System.ComponentModel.DataAnnotations;

namespace ReportSystem.Models
{
    public class User : IdentityUser
    {
        [Required]
        [Display(Name = "Username")]
        [StringLength(30, MinimumLength = 5)]
        public override string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Creation Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [MaxDateToday]
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public UserRole Role { get; set; } = UserRole.User;
    }
}
