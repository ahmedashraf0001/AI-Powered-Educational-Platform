using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.Reranking
{
    public class RerankRequest
    {
        [JsonPropertyName("query")]
        public string Query { get; set; } = string.Empty;

        [JsonPropertyName("chunks")]
        public List<RerankChunk> Chunks { get; set; } = new();

        [JsonPropertyName("top_k")]
        public int? TopK { get; set; }

        [JsonPropertyName("return_content")]
        public bool ReturnContent { get; set; } = true;
    }

    public class RerankChunk
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
