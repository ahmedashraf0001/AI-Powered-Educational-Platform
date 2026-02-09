using AIEduPlatform.Core.DTOs.Pdf;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.ML.Services;
using AIEduPlatform.ML.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.RegularExpressions;

using static AIEduPlatform.ML.MaterialProcessing.ContentProcessingHelper;

namespace AIEduPlatform.ML.MaterialProcessing
{
    public class DocumentContentExtractor : IDocumentContentExtractor
    {
        private int _chunkSize;
        private int _overlapSize;
        private readonly ILogger<DocumentContentExtractor> _logger;
        private readonly RagSettings _ragSettings;
        public DocumentContentExtractor(ILogger<DocumentContentExtractor> logger, IOptions<RagSettings> options)
        {
            _logger = logger;
            _ragSettings = options.Value;

            _chunkSize = _ragSettings.Chunking.DefaultChunkSize;
            _overlapSize = _ragSettings.Chunking.DefaultOverlapSize;

            _logger.LogInformation(
                "ContentChunker initialized: ChunkSize={ChunkSize}, OverlapSize={OverlapSize}",
                _chunkSize, _overlapSize);
        }
        public void ResizeChunk(ChunkingOptions options)
        {
            if (options.ChunkSize <= 0)
                throw new ArgumentException("Chunk size must be positive", nameof(options.ChunkSize));

            if (options.ChunkOverlap < 0)
                throw new ArgumentException("Overlap size cannot be negative", nameof(options.ChunkOverlap));

            if (options.ChunkOverlap >= options.ChunkSize)
                throw new ArgumentException("Overlap size must be less than chunk size", nameof(options.ChunkOverlap));

            _chunkSize = options.ChunkSize;
            _overlapSize = options.ChunkOverlap;
        }

        /// <summary>
        /// Creates clean, structured chunks from page content
        /// </summary>
        public List<ContextChunk> ChunkPageContent(
            PageContent pageContent,
            ChunkMetadata baseMetadata)
        {
            var chunks = new List<ContextChunk>();

            var paragraphs = SplitIntoParagraphs(pageContent.Content);

            var currentChunk = new StringBuilder();
            int chunkIndex = 0;

            foreach (var paragraph in paragraphs)
            {
                var paragraphLength = paragraph.Length + 2;

                if (currentChunk.Length + paragraphLength > _chunkSize && currentChunk.Length > 0)
                {
                    chunks.Add(CreateChunk(
                        currentChunk.ToString().Trim(),
                        pageContent,
                        baseMetadata,
                        chunkIndex++
                    ));

                    currentChunk.Clear();

                    if (_overlapSize > 0 && chunks.Count > 0)
                    {
                        var overlap = ContentProcessingHelper.GetOverlapText(chunks.Last().Content, _overlapSize);
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
            var metadata = ContentProcessingHelper.CloneMetadata(baseMetadata);
            metadata.PageOrTimestamp = $"Page {pageContent.PageNumber}";
            metadata.Section = pageContent.PrimarySection;

            var wordCount = ContentProcessingHelper.CountWords(content);
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

    }
}