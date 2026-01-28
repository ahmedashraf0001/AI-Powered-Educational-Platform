using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.Domain.Context
{
    /// <summary>
    /// Metadata describing the source of a context chunk - matches PromptBuilder format
    /// </summary>
    public class ChunkMetadata
    {
        /// <summary>
        /// The title of the source material (e.g., "Introduction to Neural Networks.pdf")
        /// </summary>
        [JsonPropertyName("sourceTitle")]
        public string SourceTitle { get; set; } = string.Empty;

        /// <summary>
        /// The type of material (e.g., "pdf", "video_transcript", "audio_transcript", "notes")
        /// </summary>
        [JsonPropertyName("materialType")]
        public string MaterialType { get; set; } = string.Empty;

        /// <summary>
        /// The page number (for PDFs) or timestamp range (for audio/video)
        /// </summary>
        [JsonPropertyName("pageOrTimestamp")]
        public string PageOrTimestamp { get; set; } = string.Empty;

        /// <summary>
        /// The section or chapter name if available
        /// </summary>
        [JsonPropertyName("section")]
        public string Section { get; set; } = string.Empty;

        /// <summary>
        /// The lecture this material belongs to
        /// </summary>
        [JsonPropertyName("lectureName")]
        public string LectureName { get; set; } = string.Empty;

        /// <summary>
        /// The course this material belongs to
        /// </summary>
        [JsonPropertyName("courseName")]
        public string CourseName { get; set; } = string.Empty;

        /// <summary>
        /// Unique identifier of the source material
        /// </summary>
        [JsonPropertyName("materialId")]
        public Guid MaterialId { get; set; }

        /// <summary>
        /// Unique identifier of the course
        /// </summary>
        [JsonPropertyName("courseId")]
        public Guid CourseId { get; set; }

        /// <summary>
        /// Unique identifier of the lecture
        /// </summary>
        [JsonPropertyName("lectureId")]
        public Guid LectureId { get; set; }

        /// <summary>
        /// Returns a structured JSON representation suitable for LLM prompts
        /// </summary>
        public override string ToString()
        {
            return ToJson();
        }

        /// <summary>
        /// Converts the metadata to formatted JSON string
        /// </summary>
        public string ToJson(bool indented = true)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = indented,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return JsonSerializer.Serialize(this, options);
        }

        /// <summary>
        /// Returns a human-readable summary
        /// </summary>
        public string ToHumanReadable()
        {
            string Safe(string value, string fallback = "-") =>
                string.IsNullOrWhiteSpace(value) ? fallback : value;

            return $"[{Safe(MaterialType, "unknown").ToUpper()}] " +
                   $"Course: {Safe(CourseName)} | " +
                   $"Lecture: {Safe(LectureName)} | " +
                   $"Section: {Safe(Section)} | " +
                   $"Source: {Safe(SourceTitle)} | " +
                   $"Page/Time: {Safe(PageOrTimestamp)} | " +
                   $"MaterialId: {MaterialId} | " +
                   $"CourseId: {CourseId} | " +
                   $"LectureId: {LectureId}";
        }

        /// <summary>
        /// Returns a compact single-line JSON representation
        /// </summary>
        public string ToCompactJson()
        {
            return ToJson(indented: false);
        }
    }
}