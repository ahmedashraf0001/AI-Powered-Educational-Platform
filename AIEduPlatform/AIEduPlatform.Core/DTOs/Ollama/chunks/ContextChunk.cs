using System;

namespace AIEduPlatform.Core.DTOs.Ollama.chunks
{
    /// <summary>
    /// Represents a chunk of context retrieved from course materials with metadata
    /// </summary>
    public class ContextChunk
    {
        /// <summary>
        /// The actual text content of the chunk
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Metadata about the source of this chunk
        /// </summary>
        public ChunkMetadata Metadata { get; set; } = new ChunkMetadata();

        /// <summary>
        /// Relevance score from vector similarity search (0.0 to 1.0)
        /// </summary>
        public float RelevanceScore { get; set; }
    }
}
