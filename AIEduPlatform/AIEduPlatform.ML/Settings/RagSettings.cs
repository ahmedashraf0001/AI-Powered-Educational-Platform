using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.ML.Settings
{
    public class RagSettings
    {
        public int MaxRetryAttempts { get; set; }
        public int EmbeddingDelayMs { get; set; }
        public float MinRerankScore { get; set; }
        public double MaxAcceptableFailureRatio { get; set; }


        public ConcurrencySettings Concurrency { get; set; } = new();

        public AudioProcessingSettings AudioProcessing { get; set; } = new();

        public VideoProcessingSettings VideoProcessing { get; set; } = new();

        public ChunkingSettings Chunking { get; set; } = new();
    }
    public class VideoProcessingSettings
    {
        public int TargetFrames { get; set; } = 200;
        public int MinFrames { get; set; } = 20;
        public int MaxFrames { get; set; } = 500;
        public float MinIntervalSeconds { get; set; } = 1.0f;
        public float MaxIntervalSeconds { get; set; } = 30.0f;
        public long MaxFileSizeBytes { get; set; } = 2L * 1024 * 1024 * 1024; 
        public int MaxDurationSeconds { get; set; } = 7200;
    }

    public class ConcurrencySettings
    {
        /// <summary>
        /// Maximum number of documents being indexed simultaneously
        /// </summary>
        public int MaxConcurrentDocuments { get; set; } = 5;

        /// <summary>
        /// Maximum number of PDF pages being processed simultaneously
        /// </summary>
        public int MaxConcurrentPages { get; set; } = 20;

        /// <summary>
        /// Maximum number of audio chunks being transcribed simultaneously
        /// </summary>
        public int MaxConcurrentTranscriptions { get; set; } = 3;

        /// <summary>
        /// Maximum number of materials being indexed in parallel (per course)
        /// </summary>
        public int MaxConcurrentMaterials { get; set; } = 3;
    }

    public class AudioProcessingSettings
    {
        /// <summary>
        /// Duration of each audio chunk in seconds before transcription
        /// </summary>
        public int ChunkDurationSeconds { get; set; } = 60;

        /// <summary>
        /// Number of audio chunks to process in a batch
        /// </summary>
        public int TranscriptionBatchSize { get; set; } = 10;

        /// <summary>
        /// Silence threshold in seconds for semantic grouping
        /// </summary>
        public double SilenceThresholdSeconds { get; set; } = 2.0;
    }

    public class ChunkingSettings
    {
        /// <summary>
        /// Default chunk size in characters
        /// </summary>
        public int DefaultChunkSize { get; set; } = 800;

        /// <summary>
        /// Default overlap size in characters between chunks
        /// </summary>
        public int DefaultOverlapSize { get; set; } = 150;
    }
}
