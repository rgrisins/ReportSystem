using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Data;
using ReportSystem.Enums;

namespace ReportSystem.Models
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ReportSystemContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<ReportSystemContext>>()))
            {
                context.Database.EnsureCreated();

                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

                string[] roleNames = { "Admin", "User" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                var adminEmail = "admin@system.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    var user = new User
                    {
                        UserName = "admin",
                        Email = adminEmail,
                        EmailConfirmed = true,
                        Role = UserRole.Admin
                    };

                    var result = await userManager.CreateAsync(user, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                }

                if (context.Report.Any())
                {
                    return;
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

                await context.SaveChangesAsync();
            }
        }
    }
}
