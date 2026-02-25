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
        // ============================================================================
        // MATERIAL-LEVEL CONCURRENCY (Coarse-grained parallelism)
        // ============================================================================

        /// <summary>
        /// Maximum number of materials being indexed in parallel per course.
        /// This is the top-level concurrency control.
        /// Recommended: 3-5 for balanced throughput without overwhelming the system.
        /// </summary>
        public int MaxConcurrentMaterials { get; set; } = 3;

        // ============================================================================
        // EXTERNAL SERVICE RATE LIMITING (Fine-grained parallelism)
        // ============================================================================

        /// <summary>
        /// Maximum concurrent calls to the embedding service across ALL materials.
        /// CRITICAL: This is the most frequently called service (every chunk needs embedding).
        /// Set based on your embedding API rate limits.
        /// Recommended: 10-20 for good throughput with most embedding APIs.
        /// </summary>
        public int MaxConcurrentEmbeddings { get; set; } = 10;

        /// <summary>
        /// Maximum concurrent calls to the vision service (OCR, image analysis).
        /// Used by: DocumentIndexingHelper (PDF pages with images), ImageIndexingHelper.
        /// Vision APIs are typically more expensive/slower than embeddings.
        /// Recommended: 3-5 to avoid overwhelming vision service.
        /// </summary>
        public int MaxConcurrentVisionCalls { get; set; } = 5;

        /// <summary>
        /// Maximum concurrent audio transcription requests.
        /// Used by: AudioIndexingHelper for transcribing audio chunks.
        /// Transcription is resource-intensive and usually has strict rate limits.
        /// Recommended: 2-3 for most transcription services (Whisper, etc.).
        /// </summary>
        public int MaxConcurrentTranscriptions { get; set; } = 2;

        /// <summary>
        /// Maximum concurrent video analysis requests.
        /// Used by: VideoIndexingHelper for analyzing video frames and transcription.
        /// Video analysis is the most resource-intensive operation.
        /// Recommended: 1-2 to prevent resource exhaustion.
        /// </summary>
        public int MaxConcurrentVideoAnalysis { get; set; } = 2;

        /// <summary>
        /// Maximum concurrent reranking requests during retrieval.
        /// Used by: RAGService.RetrieveAsync when reranking search results.
        /// Only applies during query time (not indexing).
        /// Recommended: 3-5 for typical reranking services.
        /// </summary>
        public int MaxConcurrentReranking { get; set; } = 3;

        // ============================================================================
        // INTERNAL PROCESSING CONCURRENCY (Optional fine-tuning)
        // ============================================================================

        /// <summary>
        /// Maximum number of PDF pages being processed simultaneously per document.
        /// This controls parallelism WITHIN a single document indexing operation.
        /// Note: Already bounded by MaxConcurrentMaterials at a higher level.
        /// Set to 0 to disable (process pages sequentially within each document).
        /// Recommended: 10-20 if you want parallelism within documents, 0 for simpler flow.
        /// </summary>
        public int MaxConcurrentPagesPerDocument { get; set; } = 10;

        /// <summary>
        /// Maximum number of audio chunks being processed simultaneously per audio file.
        /// This controls parallelism WITHIN a single audio indexing operation.
        /// Note: Already bounded by MaxConcurrentMaterials and MaxConcurrentTranscriptions.
        /// Recommended: Usually not needed since MaxConcurrentTranscriptions handles this.
        /// Set to 0 to disable.
        /// </summary>
        public int MaxConcurrentAudioChunksPerFile { get; set; } = 0;

        public int MaxConcurrentConceptExtractions {  get; set; } = 5;

    }

    // ============================================================================
    // USAGE NOTES
    // ============================================================================
    /*
     * CONFIGURATION EXAMPLES:
     * 
     * 1. CONSERVATIVE (for limited API quotas or shared services):
     *    MaxConcurrentMaterials = 2
     *    MaxConcurrentEmbeddings = 5
     *    MaxConcurrentVisionCalls = 2
     *    MaxConcurrentTranscriptions = 1
     *    MaxConcurrentVideoAnalysis = 1
     *    MaxConcurrentReranking = 2
     * 
     * 2. BALANCED (default - good for most scenarios):
     *    MaxConcurrentMaterials = 3
     *    MaxConcurrentEmbeddings = 10
     *    MaxConcurrentVisionCalls = 5
     *    MaxConcurrentTranscriptions = 2
     *    MaxConcurrentVideoAnalysis = 2
     *    MaxConcurrentReranking = 3
     * 
     * 3. AGGRESSIVE (for high-quota APIs and powerful infrastructure):
     *    MaxConcurrentMaterials = 5
     *    MaxConcurrentEmbeddings = 20
     *    MaxConcurrentVisionCalls = 10
     *    MaxConcurrentTranscriptions = 3
     *    MaxConcurrentVideoAnalysis = 3
     *    MaxConcurrentReranking = 5
     * 
     * KEY PRINCIPLE:
     * - MaxConcurrentMaterials controls TOP-LEVEL parallelism (how many materials at once)
     * - Service semaphores (Embeddings, Vision, etc.) control CROSS-MATERIAL resource usage
     * - This allows 3 materials to share a pool of 10 embedding slots efficiently
     * 
     * BOTTLENECK ANALYSIS:
     * - If materials wait for embeddings → increase MaxConcurrentEmbeddings
     * - If embedding API rate limits hit → decrease MaxConcurrentEmbeddings
     * - If overall throughput is low → increase MaxConcurrentMaterials
     * - If system resources maxed out → decrease MaxConcurrentMaterials
     */

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
