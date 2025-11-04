using ReportSystem.Enums;
using System.ComponentModel.DataAnnotations;

namespace ReportSystem.Models
{
    public class Report
    {
        public int Id { get; set; }

        [StringLength(60, MinimumLength = 3)]
        [Required]
        public string? Title { get; set; }

        [Display(Name = "Report Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ReportDate { get; set; }

        [Required]
        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        public ReportStatus Status { get; set; }

        [Display(Name = "Importance Rating")]
        [Required]
        public ImportanceRating ImportanceRating { get; set; }
    }
}
