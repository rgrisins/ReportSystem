using Microsoft.AspNetCore.Mvc.Rendering;
using ReportSystem.Enums;
using System.ComponentModel.DataAnnotations;


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

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }
    }
}
