using AIEduPlatform.Core.DTOs.RAG.Context;

namespace AIEduPlatform.Core.DTOs.RAG
{
    /// <summary>
    /// Request to index/store content chunks for RAG - uses ContextChunk directly
    /// </summary>
    public class RagIndexRequest
    {
        /// <summary>
        /// The material ID being indexed
        /// </summary>
        public Guid CourseId { get; set; }
        public bool Reindex { get; set; } = false;
        public ChunkingOptions? options { get; set; }
    }

    /// <summary>
    /// Response from indexing operation
    /// </summary>
    public class RagIndexResponse
    {
        /// <summary>
        /// Whether indexing was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Number of chunks indexed
        /// </summary>
        public int ChunksIndexed { get; set; }

        /// <summary>
        /// Material ID that was indexed
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// Time taken to index in milliseconds
        /// </summary>
        public long IndexTimeMs { get; set; }

        /// <summary>
        /// Time taken for embedding generation in ms
        /// </summary>
        public long EmbeddingTimeMs { get; set; }

        public int ChunksFailed { get; set; } = 0;
        public double FailureRatio { get; set; } = 0;
    }

    /// <summary>
    /// Request to delete indexed content
    /// </summary>
    public class RagDeleteRequest
    {
        /// <summary>
        /// Material ID to delete chunks for
        /// </summary>
        public Guid? MaterialId { get; set; }

        /// <summary>
        /// Lecture ID to delete all chunks for
        /// </summary>
        public Guid? LectureId { get; set; }

        /// <summary>
        /// Course ID to delete all chunks for
        /// </summary>
        public Guid? CourseId { get; set; }

    }

    /// <summary>
    /// Response from delete operation
    /// </summary>
    public class RagDeleteResponse
    {
        /// <summary>
        /// Whether deletion was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? Error { get; set; }
    }
}