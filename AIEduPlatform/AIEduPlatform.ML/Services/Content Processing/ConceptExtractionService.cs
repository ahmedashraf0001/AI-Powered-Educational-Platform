using AIEduPlatform.Core.DTOs.AI.Responses;
using AIEduPlatform.Core.DTOs.Concept;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Prompts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIEduPlatform.ML.Services.Material_Processing
{
    public class ConceptExtractionService : IConceptExtractionService
    {
        private readonly IOllamaServiceClient _llmService;
        private readonly ILogger<ConceptExtractionService> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        private static readonly HashSet<string> ValidConceptTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "core_concept", "sub_concept", "component", "process", "standard", "layer"
        };

        private static readonly HashSet<string> ValidRelationTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "uses", "defines", "implements", "extends", "part_of", "contrasts_with"
        };
        public ConceptExtractionService(IOllamaServiceClient llmService, ILogger<ConceptExtractionService> logger)
        {
            _llmService = llmService;
            _logger = logger;
        }

        public async Task<ChunkConceptsResult> ExtractFromChunkAsync(string chunkContent, Guid chunkId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(chunkContent) || chunkContent.Length < 30)
            {
                _logger.LogDebug("Skipping concept extraction for short chunk {ChunkId}", chunkId);
                return EmptyResult(chunkId);
            }
            var promptResult = PromptBuilder.BuildConceptExtractionMessages(chunkContent);
            string? raw = null;
            try
            {
                var chatResponse = await _llmService.ChatAsync(promptResult, ct);
                raw = chatResponse.Message?.Content;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "LLM call failed for concept extraction on chunk {ChunkId}", chunkId);
                return EmptyResult(chunkId);
            }
            var parsed = TryParse(raw, chunkId);
            if (parsed == null)
                return EmptyResult(chunkId);

            var result = new ChunkConceptsResult { ChunkId = chunkId };

            // Validate and normalize concepts
            foreach (var concept in parsed.Concepts)
            {
                if (string.IsNullOrWhiteSpace(concept.Name) ||
                    string.IsNullOrWhiteSpace(concept.Summary))
                    continue;

                result.Concepts.Add(new ExtractedConcept
                {
                    Name = concept.Name.Trim(),
                    Type = ValidConceptTypes.Contains(concept.Type)
                        ? concept.Type.ToLowerInvariant()
                        : "sub_concept",
                    Summary = concept.Summary.Trim()
                });
            }

            // Only keep relations where both ends exist in the extracted concepts
            var conceptNames = result.Concepts
                .Select(c => c.Name.ToLowerInvariant())
                .ToHashSet();

            foreach (var relation in parsed.Relations ?? new List<ExtractedRelation>())
            {
                if (string.IsNullOrWhiteSpace(relation.From) ||
                    string.IsNullOrWhiteSpace(relation.To))
                    continue;

                if (!conceptNames.Contains(relation.From.ToLowerInvariant()) ||
                    !conceptNames.Contains(relation.To.ToLowerInvariant()))
                {
                    _logger.LogDebug(
                        "Dropping relation '{From}' → '{To}' — one or both concepts not in extracted set",
                        relation.From, relation.To);
                    continue;
                }

                result.Relations.Add(new ExtractedRelation
                {
                    From = relation.From.Trim(),
                    To = relation.To.Trim(),
                    Type = ValidRelationTypes.Contains(relation.Type)
                        ? relation.Type.ToLowerInvariant()
                        : "uses"
                });
            }

            _logger.LogDebug(
                "Concept extraction for chunk {ChunkId}: {ConceptCount} concepts, {RelationCount} relations",
                chunkId, result.Concepts.Count, result.Relations.Count);

            return result;
        }
        private ConceptExtractionLlmResponse? TryParse(string? raw, Guid chunkId)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                _logger.LogWarning("Empty LLM response for chunk {ChunkId}", chunkId);
                return null;
            }

            // Strip markdown code fences if present
            var cleaned = Regex.Replace(raw.Trim(), @"^```(?:json)?\s*|\s*```$", "",
                RegexOptions.Multiline).Trim();

            // Find the first { } block in case there's preamble text
            var start = cleaned.IndexOf('{');
            var end = cleaned.LastIndexOf('}');
            if (start < 0 || end < 0 || end <= start)
            {
                _logger.LogWarning("No JSON object found in LLM response for chunk {ChunkId}. Raw: {Raw}",
                    chunkId, raw[..Math.Min(200, raw.Length)]);
                return null;
            }

            cleaned = cleaned[start..(end + 1)];

            try
            {
                return JsonSerializer.Deserialize<ConceptExtractionLlmResponse>(cleaned, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex,
                    "JSON deserialization failed for chunk {ChunkId}. Cleaned: {Cleaned}",
                    chunkId, cleaned[..Math.Min(300, cleaned.Length)]);
                return null;
            }
        }

        private static ChunkConceptsResult EmptyResult(Guid chunkId) => new() { ChunkId = chunkId };
    }
}
