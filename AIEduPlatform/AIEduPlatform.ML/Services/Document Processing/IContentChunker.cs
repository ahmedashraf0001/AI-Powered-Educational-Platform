using AIEduPlatform.Core.Domain.Context;
using AIEduPlatform.ML.DocumentProcessing;

namespace AIEduPlatform.ML.Services
{
    public interface IContentChunker
    {
        List<ContextChunk> ChunkPageContent(PageContent pageContent, ChunkMetadata baseMetadata);
    }
}