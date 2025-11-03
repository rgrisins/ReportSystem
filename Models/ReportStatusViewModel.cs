using Microsoft.AspNetCore.Mvc.Rendering;

namespace ReportSystem.Models
{
    public class ReportStatusViewModel
    {
        public List<Report>? Reports { get; set; }
        public SelectList? Statuses { get; set; }
        public string? ReportStatus { get; set; }
        public string? SearchString { get; set; }
    }
}
