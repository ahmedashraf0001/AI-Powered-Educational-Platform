
using AIEduPlatform.Core.DTOs.Pdf;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IPdfContentExtractor
    {
        string FileName { get; }
        string FilePath { get; }
        int PageCount { get; }

        void Dispose();
        Task<List<PageContent>> ExtractAllPagesAsync(CancellationToken cancellationToken = default);
        Task<PageContent> ExtractPageWithStructureAsync(int pageNumber, CancellationToken cancellationToken = default);
        void ResetSectionCounter();
    }
}