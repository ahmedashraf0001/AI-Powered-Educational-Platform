using AIEduPlatform.Core.DTOs.RAG.Context;

namespace AIEduPlatform.Core.DTOs.RAG
{
    /// <summary>
    /// Options for text chunking/splitting
    /// </summary>
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

        /// <summary>
        /// Separators for splitting (in order of priority)
        /// </summary>
        public List<string>? Separators { get; set; }

        /// <summary>
        /// Minimum chunk size (chunks smaller than this will be merged)
        /// </summary>
        public int MinChunkSize { get; set; } = 100;

        /// <summary>
        /// Whether to preserve paragraph boundaries
        /// </summary>
        public bool PreserveParagraphs { get; set; } = true;
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
        /// Original document length in characters
        /// </summary>
        public int OriginalLength { get; set; }


        /// <summary>
        /// Total number of chunks created
        /// </summary>
        public int TotalChunks => Chunks.Count;
    }
}
