using System.ComponentModel.DataAnnotations;

namespace ReportSystem.Enums
{
    public enum ReportStatus
    {
        [Display(Name = "Not Reviewed")]
        Not_Reviewed,

        [Display(Name = "Under Review")]
        Under_Review,

        Reviewed
    }
}