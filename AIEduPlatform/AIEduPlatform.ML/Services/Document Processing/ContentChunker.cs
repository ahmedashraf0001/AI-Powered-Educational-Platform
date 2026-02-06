using AIEduPlatform.Core.DTOs.Pdf;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.ML.DocumentProcessing;
using AIEduPlatform.ML.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AIEduPlatform.ML.Services
{
    /// <summary>
    /// Enhanced chunker that produces clean, structured chunks for both text and audio
    /// </summary>
    public class ContentChunker : IContentChunker
    {
        private int _chunkSize;
        private int _overlapSize;
        private readonly ILogger<ContentChunker> _logger;
        private readonly RagSettings _ragSettings;
        public ContentChunker(ILogger<ContentChunker> logger, IOptions<RagSettings> options)
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
        /// Creates chunks from transcribed audio content with timestamps
        /// </summary>
        public List<ContextChunk> ChunkTranscribedAudio(
            string transcribedText,
            IReadOnlyList<TranscriptionSegment> segments,
            ChunkMetadata baseMetadata,
            int audioChunkIndex,
            double audioStartTime,
            double audioEndTime)
        {
            var chunks = new List<ContextChunk>();

            if (string.IsNullOrWhiteSpace(transcribedText))
            {
                _logger.LogDebug("ChunkTranscribedAudio: Empty transcribed text");
                return chunks;
            }

            // Group segments by semantic breaks
            var semanticGroups = GroupSegmentsBySemantic(segments);

            if (semanticGroups == null || !semanticGroups.Any())
            {
                _logger.LogWarning("ChunkTranscribedAudio: No semantic groups produced");
                return chunks;
            }

            var currentChunk = new StringBuilder();
            var currentSegments = new List<TranscriptionSegment>();
            int chunkIndex = 0;

            foreach (var group in semanticGroups)
            {
                var groupText = string.Join(" ", group.Select(s => s.Text?.Trim()).Where(t => !string.IsNullOrEmpty(t)));

                if (string.IsNullOrWhiteSpace(groupText))
                    continue;

                // Calculate space needed for proper concatenation
                var spaceNeeded = currentChunk.Length > 0 ? 1 : 0;
                var totalLengthNeeded = currentChunk.Length + spaceNeeded + groupText.Length;

                // Check if we need to start a new chunk
                if (totalLengthNeeded > _chunkSize && currentChunk.Length > 0)
                {
                    // Save current chunk
                    chunks.Add(CreateAudioChunk(
                        currentChunk.ToString().Trim(),
                        currentSegments,
                        baseMetadata,
                        audioChunkIndex,
                        chunkIndex++
                    ));

                    currentChunk.Clear();
                    currentSegments.Clear();

                    // Add overlap from previous chunk
                    if (_overlapSize > 0 && chunks.Count > 0)
                    {
                        var overlap = GetOverlapText(chunks.Last().Content);
                        if (!string.IsNullOrEmpty(overlap))
                        {
                            // Ensure overlap doesn't exceed chunk size
                            if (overlap.Length > _chunkSize)
                            {
                                overlap = overlap.Substring(overlap.Length - _chunkSize);
                            }

                            currentChunk.Append(overlap);
                            currentChunk.Append(" ");
                        }
                    }

                    // ✅ FIX: After adding overlap, recalculate if current group will fit
                    spaceNeeded = currentChunk.Length > 0 ? 1 : 0;
                    totalLengthNeeded = currentChunk.Length + spaceNeeded + groupText.Length;

                    // ✅ If overlap + group would exceed chunk size, save overlap chunk and start fresh
                    if (totalLengthNeeded > _chunkSize && currentChunk.Length > 0)
                    {
                        _logger.LogDebug("ChunkTranscribedAudio: Overlap + next group exceeds chunk size. " +
                            "OverlapLength={OverlapLength}, GroupLength={GroupLength}, ChunkSize={ChunkSize}. " +
                            "Saving overlap as separate chunk.",
                            currentChunk.Length, groupText.Length, _chunkSize);

                        // Save the overlap as its own chunk
                        chunks.Add(CreateAudioChunk(
                            currentChunk.ToString().Trim(),
                            new List<TranscriptionSegment>(), // Empty segments for overlap-only chunk
                            baseMetadata,
                            audioChunkIndex,
                            chunkIndex++
                        ));

                        currentChunk.Clear();
                        currentSegments.Clear();
                    }
                }

                // Handle groups larger than chunk size
                if (groupText.Length > _chunkSize)
                {
                    _logger.LogWarning("ChunkTranscribedAudio: Semantic group exceeds chunk size. " +
                        "GroupLength={Length}, ChunkSize={Size}. Group will be split.",
                        groupText.Length, _chunkSize);

                    // Split by words if group is too large
                    var words = groupText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var wordBuilder = new StringBuilder();

                    foreach (var word in words)
                    {
                        var wordSpaceNeeded = wordBuilder.Length > 0 ? 1 : 0;

                        if (wordBuilder.Length + wordSpaceNeeded + word.Length > _chunkSize && wordBuilder.Length > 0)
                        {
                            // Add accumulated words to current chunk
                            if (currentChunk.Length > 0)
                                currentChunk.Append(" ");
                            currentChunk.Append(wordBuilder.ToString());
                            wordBuilder.Clear();
                        }

                        if (wordBuilder.Length > 0)
                            wordBuilder.Append(" ");
                        wordBuilder.Append(word);
                    }

                    // Add remaining words
                    if (wordBuilder.Length > 0)
                    {
                        if (currentChunk.Length > 0)
                            currentChunk.Append(" ");
                        currentChunk.Append(wordBuilder.ToString());
                    }

                    currentSegments.AddRange(group);
                }
                else
                {
                    // Normal case: add group to current chunk
                    if (currentChunk.Length > 0)
                    {
                        currentChunk.Append(" ");
                    }
                    currentChunk.Append(groupText);
                    currentSegments.AddRange(group);
                }
            }

            // Add final chunk if any content remains
            if (currentChunk.Length > 0)
            {
                chunks.Add(CreateAudioChunk(
                    currentChunk.ToString().Trim(),
                    currentSegments,
                    baseMetadata,
                    audioChunkIndex,
                    chunkIndex
                ));
            }

            _logger.LogDebug("ChunkTranscribedAudio: Created {ChunkCount} chunks from audio chunk {AudioChunkIndex}",
                chunks.Count, audioChunkIndex);

            return chunks;
        }

        /// <summary>
        /// Groups transcription segments by semantic breaks
        /// </summary>
        private List<List<TranscriptionSegment>> GroupSegmentsBySemantic(
            IReadOnlyList<TranscriptionSegment> segments)
        {

            if (segments == null || !segments.Any())  // ✅ Add null check
            {
                return new List<List<TranscriptionSegment>>();
            }
            var groups = new List<List<TranscriptionSegment>>();
            var currentGroup = new List<TranscriptionSegment>();

            var silenceThreshold = _ragSettings.AudioProcessing.SilenceThresholdSeconds;

            for (int i = 0; i < segments.Count; i++)
            {
                currentGroup.Add(segments[i]);

                // Check if there's a significant pause before next segment
                if (i < segments.Count - 1 && segments[i] != null && segments[i + 1] != null)
                {
                    var currentEnd = segments[i].EndTime;
                    var nextStart = segments[i + 1].StartTime;
                    var pause = nextStart - currentEnd;

                    if (pause < 0)  // Additional validation
                    {
                        _logger.LogWarning("Negative pause detected between segments");
                        pause = 0;
                    }

                    // Check if segment ends with sentence-ending punctuation
                    var text = segments[i].Text?.TrimEnd() ?? "";
                    var endsWithPunctuation = text.EndsWith(".") ||
                                             text.EndsWith("?") ||
                                             text.EndsWith("!");

                    if (pause >= silenceThreshold ||
                        (pause >= 1.0 && endsWithPunctuation))
                    {
                        // Start new group
                        groups.Add(currentGroup);
                        currentGroup = new List<TranscriptionSegment>();
                    }
                }
            }

            // Add final group
            if (currentGroup.Any())
            {
                groups.Add(currentGroup);
            }

            return groups;
        }

        /// <summary>
        /// Creates a context chunk from audio transcription
        /// </summary>
        private ContextChunk CreateAudioChunk(
            string content,
            List<TranscriptionSegment> segments,
            ChunkMetadata baseMetadata,
            int audioChunkIndex,
            int chunkIndex)
        {
            var metadata = CloneMetadata(baseMetadata);

            var startTime = segments.Any() ? segments.First().StartTime : 0;
            var endTime = segments.Any() ? segments.Last().EndTime : 0;

            // Format timestamp for display
            metadata.PageOrTimestamp = $"{FormatTimestamp(startTime)} - {FormatTimestamp(endTime)}";
            metadata.Section = "Audio Transcription";

            var wordCount = CountWords(content);
            var duration = endTime - startTime;
            var speakingRate = segments.Any() && duration > 0
                ? wordCount / duration * 60
                : 0;

            return new ContextChunk
            {
                Content = content,
                Metadata = metadata,
                RelevanceScore = 0f,
                AdditionalData = new Dictionary<string, object>
                {
                    ["chunkType"] = "audio_transcription",
                    ["wordCount"] = wordCount,
                    ["chunkIndex"] = chunkIndex,
                    ["audioChunkIndex"] = audioChunkIndex,
                    ["startTime"] = startTime,
                    ["endTime"] = endTime,
                    ["duration"] = endTime - startTime,
                    ["segmentCount"] = segments.Count,
                    ["speakingRate"] = Math.Round(speakingRate, 1),
                    ["hasQuestions"] = content.Contains("?"),
                    ["hasEmphasis"] = content.Contains("!") || content.ToUpper() == content
                }
            };
        }

        /// <summary>
        /// Formats seconds into MM:SS timestamp
        /// </summary>
        private string FormatTimestamp(double totalSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(totalSeconds);

            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }

            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
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