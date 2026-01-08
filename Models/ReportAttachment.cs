using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportSystem.Models
{
    public class ReportAttachment
    {
        public int Id { get; set; }

        [Required]
        public int ReportId { get; set; }

        [ForeignKey("ReportId")]
        public Report? Report { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "File Name")]
        public string? FileName { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "File Path")]
        public string? FilePath { get; set; }

        [Display(Name = "File Size (bytes)")]
        public long FileSize { get; set; }

        [StringLength(100)]
        [Display(Name = "Content Type")]
        public string? ContentType { get; set; }

        [Display(Name = "Uploaded At")]
        [DataType(DataType.Date)]
        public DateTime UploadedAt { get; set; }

        [Display(Name = "Uploaded By")]
        public string? UploadedBy { get; set; }

        [ForeignKey("UploadedBy")]
        public User? UploadedByUser { get; set; }
    }
}