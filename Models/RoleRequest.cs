using ReportSystem.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportSystem.Models
{
    public class RoleRequest
    {
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [StringLength(500)]
        public string? Reason { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewedBy { get; set; }

        [ForeignKey("ReviewedBy")]
        public User? ReviewedByUser { get; set; }

        [StringLength(500)]
        public string? ReviewNote { get; set; }
    }
}