using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;

namespace AIEduPlatform.ML.Services
{
    /// <summary>
    /// Handles chunking of transcribed audio content into context chunks for RAG indexing
    /// </summary>
    public interface IAudioTranscriptionChunker
    {
        /// <summary>
        /// Configures chunk sizing options
        /// </summary>
        void ResizeChunk(ChunkingOptions options);

        /// <summary>
        /// Creates chunks from transcribed audio content with timestamps
        /// </summary>
        List<ContextChunk> ChunkTranscribedAudio(
            string transcribedText,
            IReadOnlyList<TranscriptionSegment> segments,
            ChunkMetadata baseMetadata,
            int audioChunkIndex,
            double audioStartTime,
            double audioEndTime);
    }
}
