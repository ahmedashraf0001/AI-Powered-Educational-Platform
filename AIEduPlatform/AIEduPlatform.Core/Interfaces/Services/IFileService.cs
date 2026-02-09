namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IFileService
    {
        Task<FileUploadResult> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);

        Task<FileStream?> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default);

        string GetFileUrl(string fileName, string folder);

        bool IsValidFileType(string fileName, IEnumerable<string> allowedExtensions);

        bool IsValidFileSize(long fileSize, long maxSizeInBytes);
        Task<long> GetFileSizeAsync(string fileUrl, CancellationToken cancellationToken = default);
    }

    public record FileUploadResult
    {
        public bool Success { get; init; }
        public string? FileUrl { get; init; }
        public string? FileName { get; init; }
        public string? ErrorMessage { get; init; }
        public long FileSize { get; init; }
        public string? ContentType { get; init; }
    }
}
