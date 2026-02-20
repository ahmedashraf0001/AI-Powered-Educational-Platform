using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AIEduPlatform.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadsPath;
        private readonly ILogger<FileService> _logger;
        private const string UploadsFolder = "uploads";

        private static readonly ConcurrentDictionary<string, byte> _createdDirectories = new();

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

                EnsureDirectoryExists(uploadsPath);

                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                await using (var stream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,  
                    bufferSize: 4096,
                    FileOptions.Asynchronous))
                {
                    await fileStream.CopyToAsync(stream, cancellationToken);
                }

                return new FileUploadResult
                {
                    Success = true,
                    FileUrl = $"/{UploadsFolder}/{folder}/{uniqueFileName}",
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
                var physicalPath = ResolvePhysicalPath(fileUrl);

                if (File.Exists(physicalPath))
                {
                    try
                    {
                        File.Delete(physicalPath);
                        _logger.LogInformation("File deleted successfully. FileUrl: {FileUrl}", fileUrl);
                        return Task.FromResult(true);
                    }
                    catch (IOException ex) when (ex.Message.Contains("being used by another process"))
                    {
                        _logger.LogWarning("File is in use, cannot delete. FileUrl: {FileUrl}", fileUrl);
                        return Task.FromResult(false);
                    }
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

        public Task<FileStream?> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var physicalPath = ResolvePhysicalPath(fileUrl);

                if (File.Exists(physicalPath))
                {
                    var stream = new FileStream(
                        physicalPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read, 
                        bufferSize: 4096,
                        FileOptions.Asynchronous);

                    return Task.FromResult<FileStream?>(stream);
                }

                _logger.LogWarning("File not found for download. FileUrl: {FileUrl}", fileUrl);
                return Task.FromResult<FileStream?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileUrl}", fileUrl);
                return Task.FromResult<FileStream?>(null);
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

        public string ResolvePhysicalPath(string fileUrl)
        {
            var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var pathWithinUploads = relativePath.StartsWith($"{UploadsFolder}{Path.DirectorySeparatorChar}")
                ? relativePath[($"{UploadsFolder}{Path.DirectorySeparatorChar}".Length)..]
                : relativePath;
            return Path.Combine(_uploadsPath, pathWithinUploads);
        }

        private void EnsureDirectoryExists(string path)
        {
            if (_createdDirectories.ContainsKey(path))
                return;

            try
            {
                Directory.CreateDirectory(path);
                _createdDirectories.TryAdd(path, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating directory: {Path}", path);
                throw;
            }
        }

        public Task<long> GetFileSizeAsync(string fileUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                var physicalPath = ResolvePhysicalPath(fileUrl);

                if (File.Exists(physicalPath))
                {
                    var fileInfo = new FileInfo(physicalPath);
                    _logger.LogDebug("File size retrieved: {FileUrl}, Size: {Size} bytes", fileUrl, fileInfo.Length);
                    return Task.FromResult(fileInfo.Length);
                }

                _logger.LogWarning("File not found for size check. FileUrl: {FileUrl}", fileUrl);
                return Task.FromResult(0L);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file size: {FileUrl}", fileUrl);
                return Task.FromResult(0L);
            }
        }
    }
}