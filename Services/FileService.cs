using Minio;
using Minio.DataModel.Args;

namespace ReportSystem.Services
{
    public class FileService
    {
        private readonly IMinioClient _minio;
        private readonly string _bucketName;
        private readonly string[] _allowedExtensions = { ".xlsx", ".xls", ".txt", ".json", ".csv", ".pdf", ".docx", ".doc" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public FileService(IMinioClient minio, IConfiguration configuration)
        {
            _minio = minio;
            _bucketName = configuration["Minio:Bucket"] ?? "reports";
        }

        public async Task<(bool success, string? filePath, string? error)> SaveFileAsync(IFormFile file, int reportId)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return (false, null, $"File type {extension} is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");

            if (file.Length > MaxFileSize)
                return (false, null, $"File size exceeds maximum allowed size of {MaxFileSize / 1024 / 1024} MB");

            try
            {
                await EnsureBucketAsync();

                var safeName = SanitizeFileName(Path.GetFileName(file.FileName));
                var objectName = $"reports/{reportId}/{Guid.NewGuid()}_{safeName}";
                var contentType = GetContentType(file.FileName);

                using var stream = file.OpenReadStream();

                await _minio.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType));

                // Store MinIO object key in DB
                return (true, objectName, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error saving file to MinIO: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFileAsync(string? objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return false;

            try
            {
                await _minio.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool success, Stream? stream, string? contentType, string? error)> DownloadFileAsync(string objectName, string fileNameForContentType)
        {
            try
            {
                var ms = new MemoryStream();
                await _minio.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(s => s.CopyTo(ms)));

                ms.Position = 0;
                var contentType = GetContentType(fileNameForContentType);
                return (true, ms, contentType, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, $"Error downloading file from MinIO: {ex.Message}");
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

        private async Task EnsureBucketAsync()
        {
            var exists = await _minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
            if (!exists)
                await _minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}