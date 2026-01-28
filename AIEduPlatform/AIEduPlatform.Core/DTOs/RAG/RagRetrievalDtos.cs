using System.Text.Json.Serialization;
using AIEduPlatform.Core.Domain.Context;

namespace AIEduPlatform.Core.DTOs.RAG
{
    /// <summary>
    /// Request to retrieve relevant context chunks for a query
    /// </summary>
    public class RagRetrievalRequest
    {
        /// <summary>
        /// The query/question to find relevant content for
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Course ID to search within (required for scoping)
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// Optional: Specific lecture IDs to search within
        /// If null/empty, searches all lectures in the course
        /// </summary>
        public List<Guid>? LectureIds { get; set; }

        /// <summary>
        /// Optional: Specific material IDs to search within
        /// </summary>
        public List<Guid>? MaterialIds { get; set; }

        /// <summary>
        /// Maximum number of chunks to retrieve (before reranking)
        /// </summary>
        public int TopK { get; set; } = 20;

        /// <summary>
        /// Number of chunks to return after reranking
        /// </summary>
        public int FinalTopK { get; set; } = 5;

        /// <summary>
        /// Minimum similarity score threshold (0.0 to 1.0)
        /// </summary>
        public float MinScore { get; set; } = 0.3f;

        /// <summary>
        /// Whether to use reranking for better relevance
        /// </summary>
        public bool UseReranking { get; set; } = true;

        /// <summary>
        /// Optional: Filter by material types (e.g., "pdf", "video_transcript")
        /// </summary>
        public List<string>? MaterialTypes { get; set; }
    }

    /// <summary>
    /// Response from RAG retrieval - uses ContextChunk for consistency with prompt building
    /// </summary>
    public class RagRetrievalResponse
    {
        /// <summary>
        /// Whether retrieval was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// The original query
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Retrieved chunks ordered by relevance - uses ContextChunk for direct use in prompts
        /// </summary>
        public List<ContextChunk> Chunks { get; set; } = new List<ContextChunk>();

        /// <summary>
        /// Total chunks found before filtering
        /// </summary>
        public int TotalFound { get; set; }

        /// <summary>
        /// Whether reranking was applied
        /// </summary>
        public bool RerankingApplied { get; set; }

        /// <summary>
        /// Retrieval time in milliseconds
        /// </summary>
        public long RetrievalTimeMs { get; set; }

        /// <summary>
        /// Additional retrieval metadata (e.g., rerank scores)
        /// </summary>
        public RetrievalMetadata? Metadata { get; set; }
    }

    /// <summary>
    /// Additional metadata from retrieval operation
    /// </summary>
    public class RetrievalMetadata
    {
        /// <summary>
        /// Similarity scores before reranking (keyed by chunk index)
        /// </summary>
        public Dictionary<int, float>? SimilarityScores { get; set; }

        /// <summary>
        /// Rerank scores (keyed by chunk index)
        /// </summary>
        public Dictionary<int, float>? RerankScores { get; set; }

        /// <summary>
        /// Time spent on embedding the query in ms
        /// </summary>
        public long EmbeddingTimeMs { get; set; }

        /// <summary>
        /// Time spent on vector search in ms
        /// </summary>
        public long SearchTimeMs { get; set; }

        /// <summary>
        /// Time spent on reranking in ms
        /// </summary>
        public long RerankTimeMs { get; set; }
    }
}
