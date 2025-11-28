using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Data;
using ReportSystem.Enums;
using ReportSystem.Models;
using ReportSystem.Services;
using System.Security.Claims;

namespace ReportSystem.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    public class ReportsController : Controller
    {
        private readonly ReportSystemDbContext _context;
        private readonly FileService _fileService;

        // Constructor that sets the ReportSystemContext and FileService
        public ReportsController(ReportSystemDbContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: Reports
        public async Task<IActionResult> Index(string importanceRating, string reportStatus, string searchString, DateTime? dateFrom, DateTime? dateTo)
        {
            var reports = FilterReportsByDate(FilterReportsByImportance(FilterReportsByStatus(SearchReports(searchString), reportStatus), importanceRating), dateFrom, dateTo)
                .Include(r => r.CreatedByUser)
                .Include(r => r.LastModifiedByUser);

            var reportStatusVM = new ReportStatusViewModel
            {
                Statuses = new SelectList(Enum.GetValues(typeof(ReportStatus))),
                Ratings = new SelectList(Enum.GetValues(typeof(ImportanceRating))),
                Reports = await reports.ToListAsync(),
                TotalCount = GetFilteredReportCount(reports),
                StatusCounts = GetStatusCounts(reports),
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
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Status,ImportanceRating")] Report report, List<IFormFile>? attachments)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                report.ReportDate = DateTime.UtcNow;
                report.CreatedBy = userId;
                report.LastModifiedBy = userId;
                report.LastModifiedAt = DateTime.UtcNow;

                _context.Add(report);
                await _context.SaveChangesAsync();

                if (attachments != null && attachments.Any())
                {
                    var hasErrors = await SaveAttachmentsAsync(report.Id, attachments, userId);

                    if (hasErrors)
                    {
                        return View(report);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(report);
        }

        // GET: Reports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) { return NotFound(); }

            var report = await _context.Report
                .Include(r => r.Attachments)
                .Include(r => r.CreatedByUser)
                .Include(r => r.LastModifiedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (report == null) { return NotFound(); }

            return View(report);
        }

        // POST: Reports/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReportDate,Description,Status,ImportanceRating")] Report report, List<IFormFile>? attachments)
        {
            if (id != report.Id) { return NotFound(); }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingReport = await _context.Report.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
                    if (existingReport != null)
                    {
                        report.CreatedBy = existingReport.CreatedBy;
                        report.ReportDate = existingReport.ReportDate;
                    }

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    report.LastModifiedBy = userId;
                    report.LastModifiedAt = DateTime.UtcNow;

                    _context.Update(report);
                    await _context.SaveChangesAsync();

                    if (attachments != null && attachments.Any())
                    {
                        var hasErrors = await SaveAttachmentsAsync(id, attachments, userId);

                        if (hasErrors)
                        {
                            var reportWithAttachments = await _context.Report
                                .Include(r => r.Attachments).ThenInclude(a => a.UploadedByUser)
                                .Include(r => r.CreatedByUser)
                                .Include(r => r.LastModifiedByUser)
                                .FirstOrDefaultAsync(r => r.Id == id);
                            return View(reportWithAttachments);
                        }
                    }

                    TempData["SuccessMessage"] = "Report updated successfully!";
                    return RedirectToAction(nameof(Edit), new { id = id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportExists(report.Id)) { return NotFound(); }
                    throw;
                }
            }

            // Reload report with attachments if ModelState is invalid
            var reportWithData = await _context.Report
                .Include(r => r.Attachments).ThenInclude(a => a.UploadedByUser)
                .Include(r => r.CreatedByUser)
                .Include(r => r.LastModifiedByUser)
                .FirstOrDefaultAsync(r => r.Id == id);

            return View(reportWithData ?? report);
        }

        // GET: Reports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) { return NotFound(); }

            var report = await _context.Report
                .Include(r => r.Attachments)
                .Include(r => r.CreatedByUser)
                .Include(r => r.LastModifiedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (report == null) { return NotFound(); }

            return View(report);
        }

        // POST: Reports/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, bool notUsed)
        {
            var report = await _context.Report
                .Include(r => r.Attachments)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report != null)
            {
                foreach (var attachment in report.Attachments)
                {
                    await _fileService.DeleteFileAsync(attachment.FilePath);
                }

                RemoveReport(report);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // DELETE: Reports/DeleteAttachment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var attachment = await _context.ReportAttachments.FindAsync(id);
            if (attachment == null)
                return NotFound();

            var reportId = attachment.ReportId;

            await _fileService.DeleteFileAsync(attachment.FilePath);

            _context.ReportAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = reportId });
        }

        // GET: Reports/DownloadAttachment/5
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var attachment = await _context.ReportAttachments.FindAsync(id);
            if (attachment == null)
                return NotFound();

            var result = await _fileService.DownloadFileAsync(attachment.FilePath!, attachment.FileName!);
            if (!result.success || result.stream == null)
                return NotFound();

            return File(result.stream, result.contentType ?? _fileService.GetContentType(attachment.FileName!), attachment.FileName);
        }

        private async Task<bool> SaveAttachmentsAsync(int reportId, List<IFormFile> files, string? userId)
        {
            bool hasErrors = false;

            foreach (var file in files)
            {
                var result = await _fileService.SaveFileAsync(file, reportId);
                if (result.success)
                {
                    var attachment = new ReportAttachment
                    {
                        ReportId = reportId,
                        FileName = file.FileName,
                        FilePath = result.filePath,
                        FileSize = file.Length,
                        ContentType = file.ContentType,
                        UploadedAt = DateTime.UtcNow,
                        UploadedBy = userId
                    };

                    _context.ReportAttachments.Add(attachment);
                }
                else
                {
                    ModelState.AddModelError("attachments", result.error ?? "Error uploading file");
                    hasErrors = true;
                }
            }

            if (!hasErrors)
            {
                await _context.SaveChangesAsync();
            }

            return hasErrors;
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

            var report = await _context.Report
                .Include(r => r.Attachments).ThenInclude(a => a.UploadedByUser)
                .Include(r => r.CreatedByUser)
                .Include(r => r.LastModifiedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

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

        // Method to get filtered report count
        private int GetFilteredReportCount(IQueryable<Report> reports)
        {
            return reports.Count();
        }

        // Method to get counts of reports by status
        private Dictionary<ReportStatus, int> GetStatusCounts(IQueryable<Report> reports)
        {
            var statusCounts = new Dictionary<ReportStatus, int>();

            foreach (ReportStatus status in Enum.GetValues(typeof(ReportStatus)))
            {
                statusCounts[status] = reports.Count(r => r.Status == status);
            }

            return statusCounts;
        }
    }
}