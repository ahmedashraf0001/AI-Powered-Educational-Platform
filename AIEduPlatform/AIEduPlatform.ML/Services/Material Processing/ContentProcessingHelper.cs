using AIEduPlatform.Core.DTOs.RAG.Context;
using System.Text.RegularExpressions;

namespace AIEduPlatform.ML.MaterialProcessing
{
    /// <summary>
    /// Shared utility methods for content processing across document and audio chunking
    /// </summary>
    public static class ContentProcessingHelper
    {
        /// <summary>
        /// Creates a deep copy of chunk metadata
        /// </summary>
        public static ChunkMetadata CloneMetadata(ChunkMetadata source)
        {
            return new ChunkMetadata
            {
                SourceTitle = source.SourceTitle,
                MaterialType = source.MaterialType,
                PageOrTimestamp = source.PageOrTimestamp,
                Section = source.Section,
                LectureName = source.LectureName,
                CourseName = source.CourseName,
                MaterialId = source.MaterialId,
                CourseId = source.CourseId,
                LectureId = source.LectureId
            };
        }

        /// <summary>
        /// Counts the number of words in a text string
        /// </summary>
        public static int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        /// <summary>
        /// Gets overlap text from the end of previous chunk content, breaking at sentence boundaries
        /// </summary>
        public static string GetOverlapText(string previousContent, int overlapSize)
        {
            if (string.IsNullOrEmpty(previousContent) || previousContent.Length <= overlapSize)
                return string.Empty;

            // Get last overlapSize characters, but break at sentence boundary
            var overlapStart = previousContent.Length - overlapSize;
            var overlapText = previousContent.Substring(overlapStart);

            // Find first sentence boundary
            var sentenceBreak = Regex.Match(overlapText, @"[.!?]\s+");
            if (sentenceBreak.Success)
            {
                return overlapText.Substring(sentenceBreak.Index + sentenceBreak.Length);
            }

            return overlapText;
        }

        /// <summary>
        /// Formats seconds into HH:MM:SS or MM:SS timestamp
        /// </summary>
        public static string FormatTimestamp(double totalSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(totalSeconds);

            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }

            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}
