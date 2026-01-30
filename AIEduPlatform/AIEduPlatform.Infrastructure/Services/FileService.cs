using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadsPath;
        private readonly ILogger<FileService> _logger;
        private const string UploadsFolder = "uploads";

        public FileService(
            IConfiguration configuration,
            ILogger<FileService> logger)
        {
            _uploadsPath = configuration["FileStorage:BasePath"] 
                ?? Path.Combine(Directory.GetCurrentDirectory(), UploadsFolder);
            _logger = logger;
        }

        public async Task<FileUploadResult> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken cancellationToken = default)
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "File stream is empty or null."
                };
            }

            try
            {
                var uploadsPath = Path.Combine(_uploadsPath, folder);

                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(stream, cancellationToken);
                }

                var fileUrl = GetFileUrl(uniqueFileName, folder);

                _logger.LogInformation(
                    "File uploaded successfully. FileName: {FileName}, FileUrl: {FileUrl}, Size: {Size}",
                    uniqueFileName,
                    fileUrl,
                    fileStream.Length);

                return new FileUploadResult
                {
                    Success = true,
                    FileUrl = fileUrl,
                    FileName = uniqueFileName,
                    FileSize = fileStream.Length,
                    ContentType = contentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", fileName);

                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = $"Error uploading file: {ex.Message}"
                };
            }
        }

        public Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var filePath = Path.Combine(_uploadsPath, relativePath.Replace($"{UploadsFolder}{Path.DirectorySeparatorChar}", ""));

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File deleted successfully. FileUrl: {FileUrl}", fileUrl);
                    return Task.FromResult(true);
                }

                _logger.LogWarning("File not found for deletion. FileUrl: {FileUrl}", fileUrl);
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
                return Task.FromResult(false);
            }
        }

        public Task<Stream?> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var filePath = Path.Combine(_uploadsPath, relativePath.Replace($"{UploadsFolder}{Path.DirectorySeparatorChar}", ""));

                if (File.Exists(filePath))
                {
                    var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    return Task.FromResult<Stream?>(stream);
                }

                _logger.LogWarning("File not found for download. FileUrl: {FileUrl}", fileUrl);
                return Task.FromResult<Stream?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileUrl}", fileUrl);
                return Task.FromResult<Stream?>(null);
            }
        }

        public string GetFileUrl(string fileName, string folder)
        {
            return $"/{UploadsFolder}/{folder}/{fileName}";
        }

        public bool IsValidFileType(string fileName, IEnumerable<string> allowedExtensions)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(extension) && allowedExtensions.Contains(extension);
        }

        public bool IsValidFileSize(long fileSize, long maxSizeInBytes)
        {
            return fileSize > 0 && fileSize <= maxSizeInBytes;
        }
    }
}
