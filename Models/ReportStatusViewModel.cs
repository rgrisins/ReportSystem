using Microsoft.AspNetCore.Mvc.Rendering;
using ReportSystem.Enums;


namespace ReportSystem.Models
{
    public class ReportStatusViewModel
    {
        public List<Report>? Reports { get; set; }
        public SelectList? Statuses { get; set; }
        public SelectList? Ratings { get; set; }
        public string? ReportStatus { get; set; }
        public string? ImportanceRating { get; set; }
        public string? SearchString { get; set; }
        public int TotalCount { get; set; }
        public Dictionary<ReportStatus, int> StatusCounts { get; set; } = new();
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
