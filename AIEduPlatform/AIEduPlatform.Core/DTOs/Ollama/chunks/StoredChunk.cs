namespace AIEduPlatform.Core.DTOs.Ollama.chunks
{
    /// <summary>
    /// Represents a stored chunk with its embedding vector
    /// </summary>
    public class StoredChunk
    {
        /// <summary>
        /// Unique identifier for this chunk
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The course this chunk belongs to
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// The material this chunk was extracted from
        /// </summary>
        public Guid MaterialId { get; set; }

        /// <summary>
        /// The lecture this chunk belongs to
        /// </summary>
        public Guid LectureId { get; set; }

        /// <summary>
        /// The text content of the chunk
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// The embedding vector for this chunk
        /// </summary>
        public List<float> Embedding { get; set; } = new List<float>();

        /// <summary>
        /// Metadata about the chunk source
        /// </summary>
        public ChunkMetadata Metadata { get; set; } = new ChunkMetadata();

        /// <summary>
        /// When this chunk was created/indexed
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Chunk sequence number within the material
        /// </summary>
        public int ChunkIndex { get; set; }

        /// <summary>
        /// Total chunks in the material
        /// </summary>
        public int TotalChunks { get; set; }
    }
}
