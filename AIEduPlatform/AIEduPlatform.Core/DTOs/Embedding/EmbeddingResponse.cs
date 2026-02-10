using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.Embedding
{
    public class EmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public List<float> Embedding { get; set; } = new();

        [JsonPropertyName("dimension")]
        public int Dimension { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
    }
}
