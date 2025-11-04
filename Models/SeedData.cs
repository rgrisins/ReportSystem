using Microsoft.EntityFrameworkCore;
using ReportSystem.Data;
using ReportSystem.Enums;

namespace ReportSystem.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ReportSystemContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<ReportSystemContext>>()))
            {
                // Look for any report.
                if (context.Report.Any())
                {
                    return;   // DB has been seeded
                }
                context.Report.AddRange(
                    new Report
                    {
                        Title = "Web Project - Planning",
                        ReportDate = DateTime.Parse("2025-01-15"),
                        Description = "Project goals and timeline.",
                        Status = ReportStatus.Not_Reviewed,
                        ImportanceRating = ImportanceRating.High
                    },
                    new Report
                    {
                        Title = "Web Project - Design",
                        ReportDate = DateTime.Parse("2025-03-01"),
                        Description = "UI/UX design progress.",
                        Status = ReportStatus.Under_Review,
                        ImportanceRating = ImportanceRating.Medium
                    },
                    new Report
                    {
                        Title = "Web Project - Development",
                        ReportDate = DateTime.Parse("2025-05-20"),
                        Description = "Coding and testing updates.",
                        Status = ReportStatus.Under_Review,
                        ImportanceRating = ImportanceRating.High
                    },
                    new Report
                    {
                        Title = "Web Project - Launch",
                        ReportDate = DateTime.Parse("2025-07-10"),
                        Description = "Final delivery summary.",
                        Status = ReportStatus.Reviewed,
                        ImportanceRating = ImportanceRating.Low
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
