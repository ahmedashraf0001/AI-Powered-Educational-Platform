using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.Vision
{
    public class VisionAnalysisRequest
    {
        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("include_details")]
        public bool IncludeDetails { get; set; }

        [JsonPropertyName("include_metadata")]
        public bool IncludeMetadata { get; set; }
    }

    public class VisionAnalysisResponse
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("detailed_caption")]
        public string DetailedCaption { get; set; } = string.Empty;

        [JsonPropertyName("llm_context")]
        public string LlmContext { get; set; } = string.Empty;

        [JsonPropertyName("prompt_used")]
        public string PromptUsed { get; set; } = string.Empty;

        [JsonPropertyName("processing_time_ms")]
        public double ProcessingTimeMs { get; set; }

        [JsonPropertyName("image_dimensions")]
        public ImageDimensions? ImageDimensions { get; set; }

        [JsonPropertyName("model_name")]
        public string ModelName { get; set; } = string.Empty;
    }

    public class ImageDimensions
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}
