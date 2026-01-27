namespace AIEduPlatform.Core.DTOs.Ollama.chunks
{
    /// <summary>
    /// Metadata describing the source of a context chunk
    /// </summary>
    public class ChunkMetadata
    {
        /// <summary>
        /// The title of the source material (e.g., "Introduction to Neural Networks.pdf")
        /// </summary>
        public string SourceTitle { get; set; } = string.Empty;

        /// <summary>
        /// The type of material (e.g., "pdf", "video_transcript", "audio_transcript", "notes")
        /// </summary>
        public string MaterialType { get; set; } = string.Empty;

        /// <summary>
        /// The page number (for PDFs) or timestamp range (for audio/video)
        /// </summary>
        public string PageOrTimestamp { get; set; } = string.Empty;

        /// <summary>
        /// The section or chapter name if available
        /// </summary>
        public string Section { get; set; } = string.Empty;

        /// <summary>
        /// The lecture this material belongs to
        /// </summary>
        public string LectureName { get; set; } = string.Empty;

        /// <summary>
        /// The course this material belongs to
        /// </summary>
        public string CourseName { get; set; } = string.Empty;

        /// <summary>
        /// Unique identifier of the source material
        /// </summary>
        public Guid MaterialId { get; set; }

        /// <summary>
        /// Unique identifier of the lecture
        /// </summary>
        public Guid LectureId { get; set; }
    }
}
