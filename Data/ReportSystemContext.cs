using Microsoft.EntityFrameworkCore;
using ReportSystem.Models;

namespace ReportSystem.Data
{
    public class ReportSystemContext : DbContext
    {
        public ReportSystemContext(DbContextOptions<ReportSystemContext> options)
            : base(options)
        {
        }

        public DbSet<ReportSystem.Models.Report> Report { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>()
                .Property(r => r.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Report>()
                .Property(r => r.ImportanceRating)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
