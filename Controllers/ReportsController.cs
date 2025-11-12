using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Data;
using ReportSystem.Enums;
using ReportSystem.Models;
using ReportSystem.Services;


namespace ReportSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ReportSystemContext _context;
        private readonly SessionService _sessionService;

        // Constructor that sets the ReportSystemContext and SessionService dependencies
        public ReportsController(ReportSystemContext context, SessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        [Authorize(Roles = "Admin")]
        // GET: Reports
        public async Task<IActionResult> Index(string importanceRating, string reportStatus, string searchString, DateTime? dateFrom, DateTime? dateTo)
        {
            var reports = FilterReportsByDate(FilterReportsByImportance(FilterReportsByStatus(SearchReports(searchString), reportStatus), importanceRating), dateFrom, dateTo);

            var reportStatusVM = new ReportStatusViewModel
            {
                Statuses = new SelectList(Enum.GetValues(typeof(ReportStatus))),
                Ratings = new SelectList(Enum.GetValues(typeof(ImportanceRating))),
                Reports = await reports.ToListAsync(),
                TotalCount = GetTotalReportCount(),
                StatusCounts = GetStatusCounts(),
                DateFrom = dateFrom,
                DateTo = dateTo
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

        // Method to get all reports
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

        // Method to filter reports by importance rating
        private IQueryable<Report> FilterReportsByImportance(IQueryable<Report> reports, string importanceRating)
        {
            if (!string.IsNullOrEmpty(importanceRating))
            {
                Console.WriteLine(importanceRating);
                return reports.Where(x => x.ImportanceRating.ToString() == importanceRating);
            }
            return reports;
        }

        // Method to filter reports by date range
        private IQueryable<Report> FilterReportsByDate(IQueryable<Report> reports, DateTime? dateFrom, DateTime? dateTo)
        {
            if (dateFrom.HasValue)
            {
                reports = reports.Where(r => r.ReportDate >= dateFrom.Value);
            }
            if (dateTo.HasValue)
            {
                reports = reports.Where(r => r.ReportDate <= dateTo.Value);
            }
            return reports;
        }

        // Method to get report view by id
        private async Task<IActionResult?> GetReportViewById(int? id)
        {
            if (id == null) { return NotFound(); }

            var report = await _context.Report.FirstOrDefaultAsync(m => m.Id == id);

            if (report == null) { return NotFound(); }

            return View(report);
        }

        // Method to remove report
        private void RemoveReport(Report report)
        {
            if (report != null)
            {
                _context.Report.Remove(report);
            }
        }

        // Method to get total report count
        private int GetTotalReportCount()
        {
            return _context.Report.Count();
        }

        // Method to get counts of reports by status
        private Dictionary<ReportStatus, int> GetStatusCounts()
        {
            var statusCounts = new Dictionary<ReportStatus, int>();
            foreach (ReportStatus status in Enum.GetValues(typeof(ReportStatus)))
            {
                int count = _context.Report.Count(r => r.Status == status);
                statusCounts[status] = count;
            }
            return statusCounts;
        }
    }
}
