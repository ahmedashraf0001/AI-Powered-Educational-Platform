using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Pdf;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;

namespace AIEduPlatform.Core.Interfaces.Services
{
    /// <summary>
    /// Service for Retrieval-Augmented Generation (RAG) operations.
    /// Handles indexing, retrieval, and context preparation for AI prompts.
    /// Uses ContextChunk and ChunkMetadata for consistency with prompt building.
    /// </summary>
    public interface IRAGService
    {
        #region Retrieval Operations

        /// <summary>
        /// Retrieves relevant context chunks for a query using vector similarity search
        /// </summary>
        /// <param name="request">The retrieval request with query and filters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Retrieved chunks ordered by relevance</returns>
        Task<RagRetrievalResponse> RetrieveAsync(
            RagRetrievalRequest request,
            CancellationToken cancellationToken = default);

        #endregion

        #region Indexing Operations

        /// <summary>
        /// Indexes ContextChunks for later retrieval
        /// </summary>
        /// <param name="request">The index request with ContextChunks to store</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Indexing result</returns>
        Task<RagIndexResponse> IndexAsync(
            RagIndexRequest request,
            CancellationToken cancellationToken = default);


        #endregion

        #region Delete Operations

        /// <summary>
        /// Deletes indexed chunks
        /// </summary>
        /// <param name="request">Delete request specifying what to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Delete result</returns>
        Task<RagDeleteResponse> DeleteAsync(
            RagDeleteRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all chunks for a material
        /// </summary>
        /// <param name="materialId">Material ID to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Delete result</returns>
        Task<RagDeleteResponse> DeleteMaterialAsync(
            Guid materialId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all chunks for a lecture
        /// </summary>
        /// <param name="lectureId">Lecture ID to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Delete result</returns>
        Task<RagDeleteResponse> DeleteLectureAsync(
            Guid lectureId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all chunks for a course
        /// </summary>
        /// <param name="courseId">Course ID to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Delete result</returns>
        Task<RagDeleteResponse> DeleteCourseAsync(
            Guid courseId,
            CancellationToken cancellationToken = default);

        #endregion

        #region Utility Operations

        /// <summary>
        /// Chunks a document into ContextChunks with provided metadata
        /// </summary>
        /// <param name="content">The content to chunk</param>
        /// <param name="metadata">Metadata to attach to each chunk</param>
        /// <param name="options">Chunking options</param>
        /// <returns>Chunking result with ContextChunks ready for indexing</returns>
        ChunkingResult ChunkDocument(
            PageContent content,
            ChunkMetadata metadata,
            ChunkingOptions? options = null);

        /// <summary>
        /// Checks if a material has been indexed
        /// </summary>
        /// <param name="materialId">Material ID to check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if indexed, false otherwise</returns>
        Task<bool> IsMaterialIndexedAsync(
            Guid materialId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the chunk count for a material
        /// </summary>
        /// <param name="materialId">Material ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of chunks indexed</returns>
        Task<int> GetChunkCountAsync(
            Guid materialId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets indexing statistics for a course
        /// </summary>
        /// <param name="courseId">Course ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Statistics about indexed content</returns>
        Task<RagIndexStats> GetIndexStatsAsync(
            Guid courseId,
            CancellationToken cancellationToken = default);

        #endregion
    }
}
