using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.Embedding
{
    public class BatchEmbeddingResponse
    {
        [JsonPropertyName("results")]
        public List<EmbeddingResult> Results { get; set; } = new();

        [JsonPropertyName("total_chunks")]
        public int TotalChunks { get; set; }

        [JsonPropertyName("successful")]
        public int Successful { get; set; }

        [JsonPropertyName("failed")]
        public int Failed { get; set; }

        [JsonPropertyName("dimension")]
        public int Dimension { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("errors_summary")]
        public List<ErrorSummary> ErrorsSummary { get; set; } = new();
    }

    public class EmbeddingResult
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("embedding")]
        public List<float>? Embedding { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("text_length")]
        public int TextLength { get; set; }

        [JsonPropertyName("was_truncated")]
        public bool WasTruncated { get; set; }
    }

    public class ErrorSummary
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("text_preview")]
        public string TextPreview { get; set; }
    }
}
