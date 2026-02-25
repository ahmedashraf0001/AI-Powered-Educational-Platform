using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.Concept
{
    // DTOs
    public class ChunkConceptsResult
    {
        public Guid ChunkId { get; set; }
        public List<ExtractedConcept> Concepts { get; set; } = new();
        public List<ExtractedRelation> Relations { get; set; } = new();
    }

    public class ExtractedConcept
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    public class ExtractedRelation
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    // LLM response shape — only used internally for deserialization
    public class ConceptExtractionLlmResponse
    {
        [JsonPropertyName("concepts")]
        public List<ExtractedConcept> Concepts { get; set; } = new();

        [JsonPropertyName("relations")]
        public List<ExtractedRelation> Relations { get; set; } = new();
    }
    // Internal merge LLM response shape
    public class GraphMergeLlmResponse
    {
        [JsonPropertyName("concepts")]
        public List<MergedConcept> Concepts { get; set; } = new();

        [JsonPropertyName("relations")]
        public List<ExtractedRelation> Relations { get; set; } = new();
    }

    public class MergedConcept
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("aliases")]
        public List<string> Aliases { get; set; } = new();
    }
    public class QueryIntelligenceResult
    {
        public QueryIntent Intent { get; set; }
        public string RewrittenQuery { get; set; } = string.Empty;
        public List<string> TargetConcepts { get; set; } = new();
        public bool FallbackUsed { get; set; }
        public List<Guid>? TargetMaterialIds { get; set; }
    }

    public enum QueryIntent
    {
        ConceptDeepDive,
        Comparison,
        HowTo,
        FactLookup,
        Troubleshooting,
        Conversational
    }
    public class QueryIntelligenceLlmResponse
    {
        [JsonPropertyName("intent")]
        public string Intent { get; set; } = string.Empty;

        [JsonPropertyName("rewritten_query")]
        public string RewrittenQuery { get; set; } = string.Empty;

        [JsonPropertyName("target_concepts")]
        public List<string> TargetConcepts { get; set; } = new();
        [JsonPropertyName("target_material_ids")]
        public List<string>? TargetMaterialIds { get; set; }
    }
    public class MaterialContext
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }
}
