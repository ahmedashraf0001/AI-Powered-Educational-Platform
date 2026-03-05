using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Responses
{
    public record SemanticSectionExtractionResult
    {
        [JsonPropertyName("sections")]
        public List<ExtractedSection> Sections { get; init; } = new();
    }

    public record ExtractedSection
    {
        [JsonPropertyName("title")]
        public string Title { get; init; } = string.Empty;

        [JsonPropertyName("summary")]
        public string Summary { get; init; } = string.Empty;

        // Video/Audio time-based fields
        [JsonPropertyName("start")]
        public string? Start { get; init; }

        [JsonPropertyName("end")]
        public string? End { get; init; }

        // Document page-based fields
        [JsonPropertyName("startPage")]
        public int? StartPage { get; init; }

        [JsonPropertyName("endPage")]
        public int? EndPage { get; init; }
    }
}
