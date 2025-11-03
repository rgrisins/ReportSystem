using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReportSystem.Enums;

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

        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$")]
        [StringLength(12)]
        [Required]
        public string? Status { get; set; }

        [Display(Name = "Importance Rating")]
        [RegularExpression(@"^[A-Z]+[a-zA-Z0-9""'\s-]*$")]
        [StringLength(6)]
        [Required]
        public string? ImportanceRating { get; set; }
    }
}
