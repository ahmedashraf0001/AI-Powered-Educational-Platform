using AIEduPlatform.Core.Domain.Context;
using AIEduPlatform.ML.DocumentProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AIEduPlatform.ML.Services
{
    /// <summary>
    /// Enhanced chunker that produces clean, structured chunks
    /// </summary>
    public class ContentChunker : IContentChunker
    {
        private readonly int _chunkSize;
        private readonly int _overlapSize;

        public ContentChunker(int chunkSize = 800, int overlapSize = 150)
        {
            if (chunkSize <= 0)
                throw new ArgumentException("Chunk size must be positive", nameof(chunkSize));

            if (overlapSize < 0)
                throw new ArgumentException("Overlap size cannot be negative", nameof(overlapSize));

            if (overlapSize >= chunkSize)
                throw new ArgumentException("Overlap size must be less than chunk size", nameof(overlapSize));

            _chunkSize = chunkSize;
            _overlapSize = overlapSize;
        }

        /// <summary>
        /// Creates clean, structured chunks from page content
        /// </summary>
        public List<ContextChunk> ChunkPageContent(
            PageContent pageContent,
            ChunkMetadata baseMetadata)
        {
            var chunks = new List<ContextChunk>();

            // Split content into paragraphs
            var paragraphs = SplitIntoParagraphs(pageContent.Content);

            var currentChunk = new StringBuilder();
            int chunkIndex = 0;

            foreach (var paragraph in paragraphs)
            {
                var paragraphLength = paragraph.Length + 2; // +2 for \n\n

                // If adding this paragraph exceeds chunk size
                if (currentChunk.Length + paragraphLength > _chunkSize && currentChunk.Length > 0)
                {
                    // Save current chunk
                    chunks.Add(CreateChunk(
                        currentChunk.ToString().Trim(),
                        pageContent,
                        baseMetadata,
                        chunkIndex++
                    ));

                    // Start new chunk with overlap
                    currentChunk.Clear();

                    if (_overlapSize > 0 && chunks.Count > 0)
                    {
                        var overlap = GetOverlapText(chunks.Last().Content);
                        if (!string.IsNullOrEmpty(overlap))
                        {
                            currentChunk.Append(overlap);
                            currentChunk.Append("\n\n");
                        }
                    }
                }

                // Add paragraph
                if (currentChunk.Length > 0)
                {
                    currentChunk.Append("\n\n");
                }
                currentChunk.Append(paragraph);
            }

            // Add final chunk
            if (currentChunk.Length > 0)
            {
                chunks.Add(CreateChunk(
                    currentChunk.ToString().Trim(),
                    pageContent,
                    baseMetadata,
                    chunkIndex
                ));
            }

            return chunks;
        }

        /// <summary>
        /// Creates a properly formatted context chunk
        /// </summary>
        private ContextChunk CreateChunk(
            string content,
            PageContent pageContent,
            ChunkMetadata baseMetadata,
            int chunkIndex)
        {
            var metadata = CloneMetadata(baseMetadata);
            metadata.PageOrTimestamp = $"Page {pageContent.PageNumber}";
            metadata.Section = pageContent.PrimarySection;

            var wordCount = CountWords(content);
            var headingLevel = DetectHeadingInChunk(content);

            return new ContextChunk
            {
                Content = content,
                Metadata = metadata,
                RelevanceScore = 0f,
                AdditionalData = new Dictionary<string, object>
                {
                    ["chunkType"] = headingLevel > 0 ? "section_heading" : "text",
                    ["headingLevel"] = headingLevel,
                    ["wordCount"] = wordCount,
                    ["chunkIndex"] = chunkIndex,
                    ["pageNumber"] = pageContent.PageNumber,
                    ["hasCode"] = ContainsCode(content),
                    ["hasBulletPoints"] = ContainsBulletPoints(content)
                }
            };
        }

        /// <summary>
        /// Splits content into semantic paragraphs
        /// </summary>
        private List<string> SplitIntoParagraphs(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<string>();

            // Split on double newlines (paragraph breaks)
            var paragraphs = content
                .Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            return paragraphs;
        }

        /// <summary>
        /// Gets overlap text from previous chunk
        /// </summary>
        private string GetOverlapText(string previousContent)
        {
            if (string.IsNullOrEmpty(previousContent) || previousContent.Length <= _overlapSize)
                return string.Empty;

            // Get last _overlapSize characters, but break at sentence boundary
            var overlapStart = previousContent.Length - _overlapSize;
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
        /// Detects if chunk starts with a heading
        /// </summary>
        private int DetectHeadingInChunk(string content)
        {
            var lines = content.Split('\n').Take(2).ToArray();
            if (lines.Length == 0)
                return 0;

            var firstLine = lines[0].Trim();

            // Check for numbered headings
            if (Regex.IsMatch(firstLine, @"^\d+\.\s+[A-Z]"))
                return 1;

            if (Regex.IsMatch(firstLine, @"^\d+\.\d+\s+[A-Z]"))
                return 2;

            // Check if all caps (and not too long)
            if (firstLine.Length < 100 && firstLine.All(c => !char.IsLetter(c) || char.IsUpper(c)))
                return 1;

            return 0;
        }

        private bool ContainsCode(string content)
        {
            // Simple heuristic: contains common code patterns
            var codePatterns = new[]
            {
                @"[{}\[\]();].*[{}\[\]();]",  // Multiple brackets/parens
                @"\bfor\s*\(",
                @"\bif\s*\(",
                @"\bwhile\s*\(",
                @"==|!=|<=|>=",
                @"[a-zA-Z_][a-zA-Z0-9_]*\s*\(",  // Function calls
            };

            return codePatterns.Any(pattern => Regex.IsMatch(content, pattern));
        }

        private bool ContainsBulletPoints(string content)
        {
            return content.Contains("•") ||
                   Regex.IsMatch(content, @"^\s*[-*]\s+", RegexOptions.Multiline);
        }

        private int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private ChunkMetadata CloneMetadata(ChunkMetadata source)
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
    }
}