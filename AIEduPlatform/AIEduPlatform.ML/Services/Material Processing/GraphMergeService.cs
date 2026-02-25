using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Concept;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Prompts;
using Microsoft.Extensions.Logging;
using Pgvector;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIEduPlatform.ML.Services.Material_Processing
{
    public class GraphMergeService : IGraphMergeService
    {
        private readonly IOllamaServiceClient _llmService;
        private readonly IConceptRepository _conceptRepository;
        private readonly ILogger<GraphMergeService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public GraphMergeService(
            IOllamaServiceClient llmService,
            IConceptRepository conceptRepository,
            ILogger<GraphMergeService> logger)
        {
            _llmService = llmService;
            _conceptRepository = conceptRepository;
            _logger = logger;
        }

        public async Task MergeAndStoreGraphAsync(
            Guid courseId,
            Guid materialId,
            List<ChunkConceptsResult> extractions,
            IEmbeddingService embeddingService,
            CancellationToken ct = default)
        {
            var nonEmpty = extractions.Where(e => e.Concepts.Any()).ToList();
            if (!nonEmpty.Any())
            {
                _logger.LogInformation(
                    "GraphMergeService: no concepts extracted for course {CourseId}, skipping merge",
                    courseId);
                return;
            }

            _logger.LogInformation(
                "GraphMergeService: starting merge for course {CourseId}, " +
                "{ExtractionCount} chunk extractions, {TotalConcepts} raw concepts",
                courseId, nonEmpty.Count, nonEmpty.Sum(e => e.Concepts.Count));

            // Step 1: LLM merge pass
            var merged = await CallMergeAsync(nonEmpty, ct);
            if (merged == null || !merged.Concepts.Any())
            {
                _logger.LogWarning(
                    "GraphMergeService: merge returned empty graph for course {CourseId}", courseId);
                return;
            }

            // Step 2: Build alias → canonical name map for relation resolution
            var aliasMap = BuildAliasMap(merged.Concepts);

            // Step 3: Delete existing graph for this course (reindex scenario)
            await _conceptRepository.DeleteByCourseIdAsync(courseId, ct);

            // Step 4: Embed concept summaries and persist concept nodes
            var conceptEntities = new List<Concept>();
            var conceptLookup = new Dictionary<string, Concept>(StringComparer.OrdinalIgnoreCase);

            foreach (var mc in merged.Concepts ?? new List<MergedConcept>())
            {
                if (string.IsNullOrWhiteSpace(mc.Name) || string.IsNullOrWhiteSpace(mc.Summary))
                    continue;

                Vector embedding;
                try
                {
                    var embResponse = await embeddingService.GetEmbeddingAsync(
                        new EmbeddingRequest { Text = mc.Summary }, ct);
                    embedding = new Vector(embResponse.Embedding.ToArray());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "GraphMergeService: failed to embed concept '{Name}', skipping", mc.Name);
                    continue;
                }

                var entity = new Concept
                {
                    Id = Guid.NewGuid(),
                    Name = mc.Name.Trim(),
                    NormalizedName = Normalize(mc.Name),
                    Type = mc.Type,
                    Summary = mc.Summary.Trim(),
                    Embedding = embedding,
                    CourseId = courseId,
                    MaterialId = materialId
                };

                conceptEntities.Add(entity);
                conceptLookup[entity.Name] = entity;

                // Register aliases so relations can resolve them
                foreach (var alias in mc.Aliases ?? new List<string>())
                {
                    if (!string.IsNullOrWhiteSpace(alias))
                        conceptLookup.TryAdd(alias.Trim(), entity);
                }
            }

            await _conceptRepository.AddConceptsAsync(conceptEntities, ct);

            _logger.LogInformation(
                "GraphMergeService: persisted {Count} concept nodes for course {CourseId}",
                conceptEntities.Count, courseId);

            // Step 5: Persist relations
            var relationEntities = new List<ConceptRelation>();

            foreach (var rel in merged.Relations ?? new List<ExtractedRelation>())
            {
                var fromName = ResolveAlias(rel.From, aliasMap);
                var toName = ResolveAlias(rel.To, aliasMap);

                if (!conceptLookup.TryGetValue(fromName, out var fromConcept) ||
                    !conceptLookup.TryGetValue(toName, out var toConcept))
                {
                    _logger.LogDebug(
                        "GraphMergeService: skipping relation '{From}' → '{To}' — unresolved concept",
                        rel.From, rel.To);
                    continue;
                }

                if (fromConcept.Id == toConcept.Id) continue; // self-loop

                relationEntities.Add(new ConceptRelation
                {
                    Id = Guid.NewGuid(),
                    FromConceptId = fromConcept.Id,
                    ToConceptId = toConcept.Id,
                    RelationType = rel.Type
                });
            }

            if (relationEntities.Any())
                await _conceptRepository.AddRelationsAsync(relationEntities, ct);

            _logger.LogInformation(
                "GraphMergeService: persisted {Count} relations for course {CourseId}",
                relationEntities.Count, courseId);

            // Step 6: Build ConceptChunkMaps
            // For each original chunk extraction, map each extracted concept name
            // (or its alias) to the merged concept entity
            var chunkMaps = new List<ConceptChunkMap>();

            foreach (var extraction in nonEmpty)
            {
                foreach (var rawConcept in extraction.Concepts)
                {
                    var canonical = ResolveAlias(rawConcept.Name, aliasMap);

                    if (!conceptLookup.TryGetValue(canonical, out var conceptEntity))
                        continue;

                    chunkMaps.Add(new ConceptChunkMap
                    {
                        Id = Guid.NewGuid(),
                        ConceptId = conceptEntity.Id,
                        ChunkId = extraction.ChunkId
                    });
                }
            }

            // Deduplicate — same concept mapped to same chunk multiple times
            var distinctMaps = chunkMaps
                .GroupBy(m => new { m.ConceptId, m.ChunkId })
                .Select(g => g.First())
                .ToList();

            if (distinctMaps.Any())
                await _conceptRepository.AddChunkMapsAsync(distinctMaps, ct);

            _logger.LogInformation(
                "GraphMergeService: persisted {Count} concept-chunk mappings for course {CourseId}",
                distinctMaps.Count, courseId);
        }

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        private async Task<GraphMergeLlmResponse?> CallMergeAsync(
            List<ChunkConceptsResult> extractions,
            CancellationToken ct)
        {
            // To avoid overwhelming the context window, cap input
            var capped = extractions.Take(200).ToList();
            var extractionsJson = JsonSerializer.Serialize(capped);
            var promptResult = PromptBuilder.BuildGraphMergeMessages(extractionsJson);

            string? raw = null;
            try
            {
                var chatResponse = await _llmService.ChatAsync(promptResult, ct);
                raw = chatResponse.Message?.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GraphMergeService: LLM merge call failed");
                return null;
            }

            return TryParseMerge(raw);
        }

        private GraphMergeLlmResponse? TryParseMerge(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            var cleaned = Regex.Replace(raw.Trim(), @"^```(?:json)?\s*|\s*```$", "",
                RegexOptions.Multiline).Trim();

            var start = cleaned.IndexOf('{');
            var end = cleaned.LastIndexOf('}');
            if (start < 0 || end <= start) return null;

            cleaned = cleaned[start..(end + 1)];

            try
            {
                return JsonSerializer.Deserialize<GraphMergeLlmResponse>(cleaned, _jsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "GraphMergeService: failed to parse merge response");
                return null;
            }
        }

        private static Dictionary<string, string> BuildAliasMap(List<MergedConcept> concepts)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var concept in concepts)
            {
                map[concept.Name] = concept.Name;
                foreach (var alias in concept.Aliases ?? new List<string>())
                    map.TryAdd(alias, concept.Name);
            }
            return map;
        }

        private static string ResolveAlias(string name, Dictionary<string, string> aliasMap) =>
            aliasMap.TryGetValue(name, out var canonical) ? canonical : name;

        public static string Normalize(string name) =>
            Regex.Replace(name.ToLowerInvariant().Trim(), @"[\s\-_]+", "");
    }
}
