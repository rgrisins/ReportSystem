using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Models;

namespace ReportSystem.Data
{
    public class ReportSystemContext : DbContext
    {
        public ReportSystemContext (DbContextOptions<ReportSystemContext> options)
            : base(options)
        {
        }

        public DbSet<ReportSystem.Models.Report> Report { get; set; } = default!;
    }
}
