
namespace AIEduPlatform.ML.DocumentProcessing
{
    public interface IPdfContentExtractor
    {
        string FileName { get; }
        string FilePath { get; }
        int PageCount { get; }

        void Dispose();
        Task<List<PageContent>> ExtractAllPagesAsync();
        Task<PageContent> ExtractPageWithStructureAsync(int pageNumber);
        void ResetSectionCounter();
    }
}