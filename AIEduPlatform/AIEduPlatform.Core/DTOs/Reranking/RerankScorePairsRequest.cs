using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.Reranking
{
    public class RerankScorePairsRequest
    {
        [JsonPropertyName("pairs")]
        public List<RerankScorePair> Pairs { get; set; } = new();

        [JsonPropertyName("batch_size")]
        public int BatchSize { get; set; }
    }

    public class RerankScorePair
    {
        [JsonPropertyName("query")]
        public string Query { get; set; } = string.Empty;

        [JsonPropertyName("passage")]
        public string Passage { get; set; } = string.Empty;
    }
}
