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
        /// <summary>
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
                bool isNewSection = IsSectionBoundary(paragraph);

                // Force a chunk break if:
                // 1. We hit a new section heading (always break, keeps sections isolated)
                // 2. Current chunk exceeds size limit
                bool shouldBreak = currentChunk.Length > 0 &&
                                   (isNewSection || currentChunk.Length + paragraphLength > _chunkSize);

                if (shouldBreak)
                {
                    chunks.Add(CreateChunk(
                        currentChunk.ToString().Trim(),
                        pageContent,
                        baseMetadata,
                        chunkIndex++
                    ));

                    currentChunk.Clear();

                    if (_overlapSize > 0 && chunks.Count > 0 && !isNewSection)
                    {
                        // Only add overlap for size-based breaks, not section breaks
                        // Section breaks are clean — no overlap needed
                        var overlap = ContentProcessingHelper.GetOverlapText(
                            chunks.Last().Content, _overlapSize);
                        if (!string.IsNullOrEmpty(overlap))
                        {
                            currentChunk.Append(overlap);
                            currentChunk.Append("\n\n");
                        }
                    }
                }

                if (currentChunk.Length > 0)
                    currentChunk.Append("\n\n");

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
        /// Detects if a paragraph is a section heading that should trigger a chunk break.
        /// Mirrors the heading detection logic in PdfContentExtractor.IsHeading().
        /// </summary>
        private bool IsSectionBoundary(string paragraph)
        {
            _logger.LogDebug("IsSectionBoundary called for: {FirstLine}",
      paragraph.Split('\n')[0].Trim());

            if (string.IsNullOrWhiteSpace(paragraph))
                return false;

            // Only look at the first line — headings are never multi-line
            var firstLine = paragraph.Split('\n')[0].Trim();

            // Too long to be a heading
            if (firstLine.Length > 100)
                return false;

            // Numbered heading: "1. Introduction" or "1.1 Overview"
            if (Regex.IsMatch(firstLine, @"^\d+\.?\d*\s+[A-Z]"))
                return true;

            // Chapter / Section prefix
            if (Regex.IsMatch(firstLine, @"^(Chapter|Section|Part)\s+\d+", RegexOptions.IgnoreCase))
                return true;

            // Title Case short line (3-8 words, each word capitalized)
            // e.g. "Advantages of RS485" or "Data Link Layer of the OSI Model"
            var words = firstLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length >= 2 && words.Length <= 10 && char.IsUpper(firstLine[0]))
            {
                // At least half the words start with uppercase (ignores connectors like "of", "the")
                var upperStartCount = words.Count(w => w.Length > 0 && char.IsUpper(w[0]));
                if (upperStartCount >= Math.Ceiling(words.Length / 2.0))
                    return true;
            }

            // All caps short heading: "ADVANTAGES OF RS485"
            if (firstLine.Length >= 5 && firstLine.Length <= 80)
            {
                var letterCount = firstLine.Count(char.IsLetter);
                var upperCount = firstLine.Count(char.IsUpper);
                if (letterCount > 0 && (upperCount / (double)letterCount) > 0.7)
                    return true;
            }

            return false;
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
            // First split on double newlines
            var initial = content
                .Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            // Then re-split each paragraph on single newlines if a heading is detected mid-paragraph
            var result = new List<string>();
            foreach (var paragraph in initial)
            {
                var lines = paragraph.Split('\n');
                var currentBlock = new StringBuilder();

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (currentBlock.Length > 0 && IsSectionBoundary(trimmed))
                    {
                        result.Add(currentBlock.ToString().Trim());
                        currentBlock.Clear();
                    }
                    if (currentBlock.Length > 0)
                        currentBlock.Append('\n');
                    currentBlock.Append(trimmed);
                }

                if (currentBlock.Length > 0)
                    result.Add(currentBlock.ToString().Trim());
            }

            return result;
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