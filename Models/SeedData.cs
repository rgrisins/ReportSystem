using Microsoft.AspNetCore.Identity;
using ReportSystem.Data;
using ReportSystem.Enums;

namespace ReportSystem.Models
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var context = services.GetRequiredService<ReportSystemDbContext>();

            foreach (var roleName in new[] { "Admin", "User" })
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));

            var adminEmail = "admin@system.com";
            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new User
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Role = UserRole.Admin
                };
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            if (context.Report.Any())
                return;

            context.Report.AddRange(
                new Report { Title = "Web Project - Planning", ReportDate = DateTime.SpecifyKind(DateTime.Parse("2025-01-15"), DateTimeKind.Utc), Description = "Project goals and timeline.", Status = ReportStatus.Not_Reviewed, ImportanceRating = ImportanceRating.High },
                new Report { Title = "Web Project - Design", ReportDate = DateTime.SpecifyKind(DateTime.Parse("2025-01-15"), DateTimeKind.Utc), Description = "UI/UX design progress.", Status = ReportStatus.Under_Review, ImportanceRating = ImportanceRating.Medium },
                new Report { Title = "Web Project - Development", ReportDate = DateTime.SpecifyKind(DateTime.Parse("2025-05-20"), DateTimeKind.Utc), Description = "Coding and testing updates.", Status = ReportStatus.Under_Review, ImportanceRating = ImportanceRating.High },
                new Report { Title = "Web Project - Launch", ReportDate = DateTime.SpecifyKind(DateTime.Parse("2025-07-10"), DateTimeKind.Utc), Description = "Final delivery summary.", Status = ReportStatus.Reviewed, ImportanceRating = ImportanceRating.Low }
            );
            await context.SaveChangesAsync();
        }
    }
}
