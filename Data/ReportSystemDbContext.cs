using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Models;

namespace ReportSystem.Data
{
    public class ReportSystemDbContext : IdentityDbContext<User>
    {
        public ReportSystemDbContext(DbContextOptions<ReportSystemDbContext> options)
            : base(options)
        {
        }

        public DbSet<Report> Report { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure enum properties to be stored as strings
            modelBuilder.Entity<Report>()
                .Property(r => r.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Report>()
                .Property(r => r.ImportanceRating)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
