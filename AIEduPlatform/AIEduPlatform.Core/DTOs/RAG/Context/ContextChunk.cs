using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.RAG.Context
{
    /// <summary>
    /// Represents a chunk of context retrieved from course materials - matches PromptBuilder format
    /// </summary>
    public class ContextChunk
    {
        /// <summary>
        /// The actual text content of the chunk
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Metadata about the source of this chunk
        /// </summary>
        [JsonPropertyName("metadata")]
        public ChunkMetadata Metadata { get; set; } = new ChunkMetadata();

        /// <summary>
        /// Relevance score from vector similarity search (0.0 to 1.0)
        /// </summary>
        [JsonPropertyName("relevanceScore")]
        public float RelevanceScore { get; set; }

        /// <summary>
        /// Returns a structured JSON representation suitable for LLM prompts
        /// </summary>
        public override string ToString()
        {
            return ToJson();
        }

        /// <summary>
        /// Converts the chunk to formatted JSON string
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
        /// Returns a human-readable markdown format for LLM prompts
        /// </summary>
        public string ToMarkdown()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("```");
            sb.AppendLine($"Source: {Metadata.SourceTitle}");
            sb.AppendLine($"Type: {Metadata.MaterialType}");

            if (!string.IsNullOrWhiteSpace(Metadata.CourseName))
                sb.AppendLine($"Course: {Metadata.CourseName}");

            if (!string.IsNullOrWhiteSpace(Metadata.LectureName))
                sb.AppendLine($"Lecture: {Metadata.LectureName}");

            if (!string.IsNullOrWhiteSpace(Metadata.Section))
                sb.AppendLine($"Section: {Metadata.Section}");

            if (!string.IsNullOrWhiteSpace(Metadata.PageOrTimestamp))
                sb.AppendLine($"Location: {Metadata.PageOrTimestamp}");

            sb.AppendLine($"Relevance: {RelevanceScore:P0}");
            sb.AppendLine();
            sb.AppendLine(Content);
            sb.AppendLine("```");

            return sb.ToString();
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