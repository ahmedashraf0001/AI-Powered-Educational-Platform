using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Common
{
    /// <summary>
    /// Source reference matching AI prompt JSON format
    /// </summary>
    public class AiSourceReference
    {
        /// <summary>
        /// Title of the source material
        /// </summary>
        [JsonPropertyName("sourceTitle")]
        public string SourceTitle { get; set; } = string.Empty;

        /// <summary>
        /// Page number, timestamp, or location within the material
        /// </summary>
        [JsonPropertyName("sourceLocation")]
        public string SourceLocation { get; set; } = string.Empty;

        /// <summary>
        /// Section name within the source (optional)
        /// </summary>
        [JsonPropertyName("sourceSection")]
        public string? SourceSection { get; set; }

        /// <summary>
        /// Material ID for linking (internal use, not in AI response)
        /// </summary>
        [JsonIgnore]
        public Guid? MaterialId { get; set; }

        /// <summary>
        /// Relevance score from retrieval (internal use, not in AI response)
        /// </summary>
        [JsonIgnore]
        public float? RelevanceScore { get; set; }
    }
}
