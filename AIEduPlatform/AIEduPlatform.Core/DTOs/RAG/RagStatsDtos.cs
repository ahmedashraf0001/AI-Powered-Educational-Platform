namespace AIEduPlatform.Core.DTOs.RAG
{
    /// <summary>
    /// Statistics about indexed RAG content for a course
    /// </summary>
    public class RagIndexStats
    {
        /// <summary>
        /// Course ID
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// Course name
        /// </summary>
        public string CourseName { get; set; } = string.Empty;

        /// <summary>
        /// Total number of indexed chunks
        /// </summary>
        public int TotalChunks { get; set; }

        /// <summary>
        /// Total number of indexed materials
        /// </summary>
        public int TotalMaterials { get; set; }

        /// <summary>
        /// Total number of lectures with indexed content
        /// </summary>
        public int TotalLectures { get; set; }

        /// <summary>
        /// Breakdown by material type
        /// </summary>
        public Dictionary<string, MaterialTypeStats> ByMaterialType { get; set; } = new();

        /// <summary>
        /// Breakdown by lecture
        /// </summary>
        public List<LectureIndexStats> ByLecture { get; set; } = new();

        /// <summary>
        /// Total approximate token count (estimated)
        /// </summary>
        public long EstimatedTokenCount { get; set; }

        /// <summary>
        /// Last indexing timestamp
        /// </summary>
        public DateTime? LastIndexedAt { get; set; }
    }

    /// <summary>
    /// Statistics for a specific material type
    /// </summary>
    public class MaterialTypeStats
    {
        /// <summary>
        /// Material type (pdf, video_transcript, etc.)
        /// </summary>
        public string MaterialType { get; set; } = string.Empty;

        /// <summary>
        /// Number of materials of this type
        /// </summary>
        public int MaterialCount { get; set; }

        /// <summary>
        /// Number of chunks from this type
        /// </summary>
        public int ChunkCount { get; set; }
    }

    /// <summary>
    /// Index statistics for a single lecture
    /// </summary>
    public class LectureIndexStats
    {
        /// <summary>
        /// Lecture ID
        /// </summary>
        public Guid LectureId { get; set; }

        /// <summary>
        /// Lecture name
        /// </summary>
        public string LectureName { get; set; } = string.Empty;

        /// <summary>
        /// Number of indexed materials
        /// </summary>
        public int MaterialCount { get; set; }

        /// <summary>
        /// Number of indexed chunks
        /// </summary>
        public int ChunkCount { get; set; }

        /// <summary>
        /// Materials in this lecture
        /// </summary>
        public List<MaterialIndexInfo> Materials { get; set; } = new();
    }

    /// <summary>
    /// Index information for a single material
    /// </summary>
    public class MaterialIndexInfo
    {
        /// <summary>
        /// Material ID
        /// </summary>
        public Guid MaterialId { get; set; }

        /// <summary>
        /// Material title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Material type
        /// </summary>
        public string MaterialType { get; set; } = string.Empty;

        /// <summary>
        /// Number of chunks
        /// </summary>
        public int ChunkCount { get; set; }

        /// <summary>
        /// When it was indexed
        /// </summary>
        public DateTime IndexedAt { get; set; }

        /// <summary>
        /// Whether indexing is complete
        /// </summary>
        public bool IsComplete { get; set; }
    }
}
