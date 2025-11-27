namespace ReportSystem.Services
{
    public class FileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".xlsx", ".xls", ".txt", ".json", ".csv", ".pdf", ".docx", ".doc" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<(bool success, string? filePath, string? error)> SaveFileAsync(IFormFile file, int reportId)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return (false, null, $"File type {extension} is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
            }

            if (file.Length > MaxFileSize)
            {
                return (false, null, $"File size exceeds maximum allowed size of {MaxFileSize / 1024 / 1024} MB");
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "reports", reportId.ToString());
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = Path.Combine("uploads", "reports", reportId.ToString(), uniqueFileName);
                return (true, relativePath, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error saving file: {ex.Message}");
            }
        }

        public bool DeleteFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".txt" => "text/plain",
                ".json" => "application/json",
                ".csv" => "text/csv",
                _ => "application/octet-stream"
            };
        }
    }
}