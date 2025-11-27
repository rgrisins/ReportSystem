using ReportSystem.Enums;
using ReportSystem.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [MaxDateToday]
        public DateTime ReportDate { get; set; }

        [Required]
        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        public ReportStatus Status { get; set; }

        [Display(Name = "Importance Rating")]
        [Required]
        public ImportanceRating ImportanceRating { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Last Modified By")]
        public string? LastModifiedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public User? CreatedByUser { get; set; }

        [ForeignKey("LastModifiedBy")]
        public User? LastModifiedByUser { get; set; }

        [Display(Name = "Last Modified At")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? LastModifiedAt { get; set; }

        public ICollection<ReportAttachment> Attachments { get; set; } = new List<ReportAttachment>();
    }
}
