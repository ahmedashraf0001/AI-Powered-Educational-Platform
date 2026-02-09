using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.ML.Services;
using AIEduPlatform.ML.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace AIEduPlatform.ML.MaterialProcessing
{
    /// <summary>
    /// Handles chunking of transcribed audio text into context chunks for RAG indexing.
    /// Separates audio transcription chunking concerns from document content extraction.
    /// </summary>
    public class AudioTranscriptionChunker : IAudioTranscriptionChunker
    {
        private int _chunkSize;
        private int _overlapSize;
        private readonly ILogger<AudioTranscriptionChunker> _logger;
        private readonly RagSettings _ragSettings;

        public AudioTranscriptionChunker(
            ILogger<AudioTranscriptionChunker> logger,
            IOptions<RagSettings> options)
        {
            _logger = logger;
            _ragSettings = options.Value;

            _chunkSize = _ragSettings.Chunking.DefaultChunkSize;
            _overlapSize = _ragSettings.Chunking.DefaultOverlapSize;

            _logger.LogInformation(
                "AudioTranscriptionChunker initialized: ChunkSize={ChunkSize}, OverlapSize={OverlapSize}",
                _chunkSize, _overlapSize);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
                        var overlap = ContentProcessingHelper.GetOverlapText(chunks.Last().Content, _overlapSize);
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

                    // After adding overlap, recalculate if current group will fit
                    spaceNeeded = currentChunk.Length > 0 ? 1 : 0;
                    totalLengthNeeded = currentChunk.Length + spaceNeeded + groupText.Length;

                    // If overlap + group would exceed chunk size, save overlap chunk and start fresh
                    if (totalLengthNeeded > _chunkSize && currentChunk.Length > 0)
                    {
                        _logger.LogDebug("ChunkTranscribedAudio: Overlap + next group exceeds chunk size. " +
                            "OverlapLength={OverlapLength}, GroupLength={GroupLength}, ChunkSize={ChunkSize}. " +
                            "Saving overlap as separate chunk.",
                            currentChunk.Length, groupText.Length, _chunkSize);

                        // Save the overlap as its own chunk
                        chunks.Add(CreateAudioChunk(
                            currentChunk.ToString().Trim(),
                            new List<TranscriptionSegment>(),
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
        /// Groups transcription segments by semantic breaks (pauses and punctuation)
        /// </summary>
        private List<List<TranscriptionSegment>> GroupSegmentsBySemantic(
            IReadOnlyList<TranscriptionSegment> segments)
        {
            if (segments == null || !segments.Any())
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

                    if (pause < 0)
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
        /// Creates a context chunk from audio transcription data
        /// </summary>
        private ContextChunk CreateAudioChunk(
            string content,
            List<TranscriptionSegment> segments,
            ChunkMetadata baseMetadata,
            int audioChunkIndex,
            int chunkIndex)
        {
            var metadata = ContentProcessingHelper.CloneMetadata(baseMetadata);

            var startTime = segments.Any() ? segments.First().StartTime : 0;
            var endTime = segments.Any() ? segments.Last().EndTime : 0;

            metadata.PageOrTimestamp = $"{ContentProcessingHelper.FormatTimestamp(startTime)} - {ContentProcessingHelper.FormatTimestamp(endTime)}";
            metadata.Section = "Audio Transcription";

            var wordCount = ContentProcessingHelper.CountWords(content);
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
    }
}
