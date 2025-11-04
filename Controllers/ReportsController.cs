using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Data;
using ReportSystem.Enums;
using ReportSystem.Models;


namespace ReportSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ReportSystemContext _context;

        public ReportsController(ReportSystemContext context)
        {
            _context = context;
        }

        // GET: Reports
        public async Task<IActionResult> Index(string reportStatus, string searchString)
        {
            if (_context.Report == null)
            {
                return Problem("Entity set 'ReportSystemContext.Report'  is null.");
            }

            var statusQuery = GetStatusQuery();

            var reports = FilterReportsByStatus(SearchReports(searchString), reportStatus);

            var reportStatusVM = new ReportStatusViewModel
            {
                Statuses = new SelectList(await statusQuery.Distinct().ToListAsync()),
                Reports = await reports.ToListAsync()
            };

            return View(reportStatusVM);
        }

        // GET: Reports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            return await GetReportViewById(id);
        }

        // GET: Reports/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Reports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReportDate,Description,Status,ImportanceRating")] Report report)
        {
            if (ModelState.IsValid)
            {
                _context.Add(report);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(report);
        }

        // GET: Reports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            return await GetReportViewById(id);
        }

        // POST: Reports/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReportDate,Description,Status,ImportanceRating")] Report report)
        {
            if (id != report.Id) { return NotFound(); }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(report);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (ReportExists(report.Id)) { return NotFound(); }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(report);
        }

        // GET: Reports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            return await GetReportViewById(id);
        }

        // POST: Reports/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, bool notUsed)
        {
            var report = await _context.Report.FindAsync(id);

            RemoveReport(report);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReportExists(int id)
        {
            return _context.Report.Any(e => e.Id == id);
        }

        private IQueryable<ReportStatus> GetStatusQuery()
        {
            // Use LINQ to get list of statuses.
            IQueryable<ReportStatus> statusQuery = from m in _context.Report
                                                   orderby m.Status
                                                   select m.Status;
            return statusQuery;
        }

        private IQueryable<Report> GetReports()
        {
            // Use LINQ to get list of Reports.
            return from m in _context.Report
                   select m;
        }

        // Method to search reports by search query
        private IQueryable<Report> SearchReports(string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                return GetReports().Where(s => s.Title!.ToUpper().Contains(searchString.ToUpper()));
            }
            return GetReports();
        }

        // Method to filter reports by status
        private IQueryable<Report> FilterReportsByStatus(IQueryable<Report> reports, string reportStatus)
        {
            if (!string.IsNullOrEmpty(reportStatus))
            {
                return reports.Where(x => x.Status.ToString() == reportStatus);
            }
            return reports;
        }

        private async Task<IActionResult?> GetReportViewById(int? id)
        {
            if (id == null) { return NotFound(); }

            var report = await _context.Report.FirstOrDefaultAsync(m => m.Id == id);

            if (report == null) { return NotFound(); }

            return View(report);
        }

        private void RemoveReport(Report report)
        {
            if (report != null)
            {
                _context.Report.Remove(report);
            }
        }

    }
}
