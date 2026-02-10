using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.Embedding
{
    public class BatchEmbeddingRequest
    {
        [JsonPropertyName("texts")]
        public List<EmbeddingChunk> Texts { get; set; } = new();

        [JsonPropertyName("normalize")]
        public bool Normalize { get; set; } = true;

        [JsonPropertyName("continue_on_error")]
        public bool ContinueOnError { get; set; } = false;
    }

    public class EmbeddingChunk
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
