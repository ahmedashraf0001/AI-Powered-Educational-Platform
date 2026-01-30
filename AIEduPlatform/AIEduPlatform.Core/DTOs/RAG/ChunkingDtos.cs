using AIEduPlatform.Core.DTOs.RAG.Context;

namespace AIEduPlatform.Core.DTOs.RAG
{
    public class ChunkingOptions
    {
        /// <summary>
        /// Target size for each chunk in characters
        /// </summary>
        public int ChunkSize { get; set; } = 1000;

        /// <summary>
        /// Overlap between chunks in characters
        /// </summary>
        public int ChunkOverlap { get; set; } = 200;
    }

    /// <summary>
    /// Result of chunking a document - outputs ContextChunk directly
    /// </summary>
    public class ChunkingResult
    {
        /// <summary>
        /// The resulting chunks with full metadata - ready for indexing or prompt building
        /// </summary>
        public List<ContextChunk> Chunks { get; set; } = new();

        /// <summary>
        /// Total number of chunks created
        /// </summary>
        public int TotalChunks => Chunks.Count;
    }
}
