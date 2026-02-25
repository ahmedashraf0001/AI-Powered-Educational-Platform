using AIEduPlatform.Core.DTOs.Pdf;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;

namespace AIEduPlatform.ML.Services
{
    /// <summary>
    /// Handles chunking of document page content into context chunks for RAG indexing
    /// </summary>
    public interface IDocumentContentExtractor
    {
        void ResizeChunk(ChunkingOptions options);
        List<ContextChunk> ChunkPageContent(PageContent pageContent, ChunkMetadata baseMetadata);
    }
}