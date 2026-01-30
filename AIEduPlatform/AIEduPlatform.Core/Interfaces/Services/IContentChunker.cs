using AIEduPlatform.Core.DTOs.Pdf;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.ML.DocumentProcessing;

namespace AIEduPlatform.ML.Services
{
    public interface IContentChunker
    {
        void ResizeChunk(ChunkingOptions options);
        List<ContextChunk> ChunkPageContent(PageContent pageContent, ChunkMetadata baseMetadata);
    }
}