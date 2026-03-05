using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.RAG.Context;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sections
{
    /// <summary>
    /// Filters RAG chunks to only those falling within a semantic section's boundaries.
    /// </summary>
    public static class SectionChunkFilter
    {
        /// <summary>
        /// Filters context chunks to only those overlapping the given section's time/page range.
        /// </summary>
        public static List<ContextChunk> FilterChunksToSection(
            List<ContextChunk> chunks,
            SemanticSection section)
        {
            if (chunks == null || !chunks.Any()) return new List<ContextChunk>();

            // Time-based filtering (video/audio)
            if (section.StartSeconds.HasValue && section.EndSeconds.HasValue)
            {
                return chunks.Where(c => ChunkOverlapsTimeRange(c, section.StartSeconds.Value, section.EndSeconds.Value)).ToList();
            }

            // Page-based filtering (document)
            if (section.StartPage.HasValue && section.EndPage.HasValue)
            {
                return chunks.Where(c => ChunkOverlapsPageRange(c, section.StartPage.Value, section.EndPage.Value)).ToList();
            }

            return chunks;
        }

        private static bool ChunkOverlapsTimeRange(ContextChunk chunk, int startSeconds, int endSeconds)
        {
            // Try AdditionalData first (video chunks store start_time/end_time)
            if (chunk.AdditionalData != null)
            {
                if (TryGetFloat(chunk.AdditionalData, "start_time", out var chunkStart) &&
                    TryGetFloat(chunk.AdditionalData, "end_time", out var chunkEnd))
                {
                    return chunkStart < endSeconds && chunkEnd > startSeconds;
                }
            }

            // Fallback: parse PageOrTimestamp (e.g., "00:05:30 - 00:06:00")
            if (!string.IsNullOrEmpty(chunk.Metadata?.PageOrTimestamp))
            {
                var match = Regex.Match(chunk.Metadata.PageOrTimestamp, @"(\d{2}:\d{2}:\d{2})");
                if (match.Success && TryParseTimestamp(match.Value, out var ts))
                {
                    return ts >= startSeconds && ts <= endSeconds;
                }
            }

            return false;
        }

        private static bool ChunkOverlapsPageRange(ContextChunk chunk, int startPage, int endPage)
        {
            if (!string.IsNullOrEmpty(chunk.Metadata?.PageOrTimestamp))
            {
                var match = Regex.Match(chunk.Metadata.PageOrTimestamp, @"(\d+)");
                if (match.Success && int.TryParse(match.Value, out var page))
                {
                    return page >= startPage && page <= endPage;
                }
            }

            return false;
        }

        private static bool TryGetFloat(Dictionary<string, object> data, string key, out float value)
        {
            value = 0;
            if (!data.TryGetValue(key, out var obj)) return false;

            if (obj is float f) { value = f; return true; }
            if (obj is double d) { value = (float)d; return true; }
            if (obj is int i) { value = i; return true; }
            if (obj is long l) { value = l; return true; }
            if (obj is JsonElement je)
            {
                if (je.TryGetSingle(out var s)) { value = s; return true; }
                if (je.TryGetDouble(out var dd)) { value = (float)dd; return true; }
            }
            if (float.TryParse(obj?.ToString(), out var parsed)) { value = parsed; return true; }

            return false;
        }

        private static bool TryParseTimestamp(string timestamp, out int totalSeconds)
        {
            totalSeconds = 0;
            var parts = timestamp.Split(':');
            if (parts.Length == 3
                && int.TryParse(parts[0], out var h)
                && int.TryParse(parts[1], out var m)
                && int.TryParse(parts[2], out var s))
            {
                totalSeconds = h * 3600 + m * 60 + s;
                return true;
            }
            if (parts.Length == 2
                && int.TryParse(parts[0], out var m2)
                && int.TryParse(parts[1], out var s2))
            {
                totalSeconds = m2 * 60 + s2;
                return true;
            }
            return false;
        }
    }
}
