using Microsoft.EntityFrameworkCore;
using ReportSystem.Data;

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
                        Status = "Not Reviewed",
                        ImportanceRating = "High"
                    },
                    new Report
                    {
                        Title = "Web Project - Design",
                        ReportDate = DateTime.Parse("2025-03-01"),
                        Description = "UI/UX design progress.",
                        Status = "Under Review",
                        ImportanceRating = "Medium"
                    },
                    new Report
                    {
                        Title = "Web Project - Development",
                        ReportDate = DateTime.Parse("2025-05-20"),
                        Description = "Coding and testing updates.",
                        Status = "Under Review",
                        ImportanceRating = "High"
                    },
                    new Report
                    {
                        Title = "Web Project - Launch",
                        ReportDate = DateTime.Parse("2025-07-10"),
                        Description = "Final delivery summary.",
                        Status = "Reviewed",
                        ImportanceRating = "Low"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
