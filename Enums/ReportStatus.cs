using System.ComponentModel.DataAnnotations;

namespace ReportSystem.Enums
{
    public enum ReportStatus
    {
        [Display(Name = "Not Reviewed")]
        NotReviewed,

        [Display(Name = "Under Review")]
        UnderReview,

        Reviewed
    }
}