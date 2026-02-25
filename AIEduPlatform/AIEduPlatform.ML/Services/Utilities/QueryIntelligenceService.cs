using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.Concept;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Prompts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIEduPlatform.ML.Services.Utilities
{
    public class QueryIntelligenceService : IQueryIntelligenceService
    {
        private readonly IOllamaServiceClient _llmService;
        private readonly ILogger<QueryIntelligenceService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly Dictionary<string, QueryIntent> IntentMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["concept_deep_dive"] = QueryIntent.ConceptDeepDive,
                ["comparison"] = QueryIntent.Comparison,
                ["how_to"] = QueryIntent.HowTo,
                ["fact_lookup"] = QueryIntent.FactLookup,
                ["troubleshooting"] = QueryIntent.Troubleshooting,
                ["conversational"] = QueryIntent.Conversational
            };

        public QueryIntelligenceService(
            IOllamaServiceClient llmService,
            ILogger<QueryIntelligenceService> logger)
        {
            _llmService = llmService;
            _logger = logger;
        }

        public async Task<QueryIntelligenceResult> AnalyzeAsync(
            string query,
            List<OllamaMessage>? conversationHistory = null,
            List<MaterialContext>? materials = null,
            CancellationToken ct = default)
        {
            var fallback = BuildFallback(query);

            if (string.IsNullOrWhiteSpace(query))
                return fallback;

            var promptResult = PromptBuilder.BuildQueryIntelligenceMessages(
                query, conversationHistory, materials);

            string? raw = null;
            try
            {
                var chatResponse = await _llmService.ChatAsync(promptResult, ct);
                raw = chatResponse.Message?.Content;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "QueryIntelligenceService: LLM call failed, falling back to original query");
                return fallback;
            }

            var parsed = TryParse(raw);
            if (parsed == null)
            {
                _logger.LogWarning("QueryIntelligenceService: parse failed, falling back");
                return fallback;
            }

            var result = new QueryIntelligenceResult
            {
                Intent = IntentMap.TryGetValue(parsed.Intent, out var intent)
                    ? intent : QueryIntent.FactLookup,

                RewrittenQuery = string.IsNullOrWhiteSpace(parsed.RewrittenQuery)
                    ? query : parsed.RewrittenQuery,

                TargetConcepts = parsed.TargetConcepts
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c.Trim())
                    .ToList(),

                TargetMaterialIds = parsed.TargetMaterialIds?
                    .Where(id => Guid.TryParse(id, out _))
                    .Select(Guid.Parse)
                    .ToList()
            };

            _logger.LogInformation(
                "QueryIntelligenceService: Intent={Intent}, Concepts=[{Concepts}], " +
                "RewrittenQuery='{Rewritten}', TargetMaterialIds=[{MaterialIds}]",
                result.Intent,
                string.Join(", ", result.TargetConcepts),
                result.RewrittenQuery,
                result.TargetMaterialIds != null ? string.Join(", ", result.TargetMaterialIds) : "null");

            return result;
        }

        private QueryIntelligenceLlmResponse? TryParse(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            var cleaned = Regex.Replace(raw.Trim(), @"^```(?:json)?\s*|\s*```$", "",
                RegexOptions.Multiline).Trim();

            var start = cleaned.IndexOf('{');
            var end = cleaned.LastIndexOf('}');
            if (start < 0 || end <= start) return null;

            try
            {
                return JsonSerializer.Deserialize<QueryIntelligenceLlmResponse>(
                    cleaned[start..(end + 1)], _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "QueryIntelligenceService: JSON parse error");
                return null;
            }
        }

        private static QueryIntelligenceResult BuildFallback(string query) => new()
        {
            Intent = QueryIntent.FactLookup,
            RewrittenQuery = query,
            TargetConcepts = new List<string>(),
            TargetMaterialIds = null,
            FallbackUsed = true
        };
    }
}
