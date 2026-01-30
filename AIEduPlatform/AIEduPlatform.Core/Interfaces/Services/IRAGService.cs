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
        //#region Retrieval Operations

        ///// <summary>
        ///// Retrieves relevant context chunks for a query using vector similarity search
        ///// </summary>
        ///// <param name="request">The retrieval request with query and filters</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Retrieved chunks ordered by relevance</returns>
        //Task<RagRetrievalResponse> RetrieveAsync(
        //    RagRetrievalRequest request,
        //    CancellationToken cancellationToken = default);

        ///// <summary>
        ///// Retrieves context for a specific course and query (convenience method)
        ///// Returns ContextChunks ready for use in prompts
        ///// </summary>
        ///// <param name="query">The search query</param>
        ///// <param name="courseId">Course to search within</param>
        ///// <param name="topK">Number of chunks to return</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Retrieved ContextChunks ready for prompt building</returns>
        //Task<List<ContextChunk>> RetrieveContextAsync(
        //    string query,
        //    Guid courseId,
        //    int topK = 5,
        //    CancellationToken cancellationToken = default);

        ///// <summary>
        ///// Retrieves context for a specific course and query
        ///// </summary>
        ///// <param name="query">The search query</param>
        ///// <param name="courseId">Course to search within</param>
        ///// <param name="topK">Number of chunks to return</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Retrieved chunks</returns>
        //Task<RagRetrievalResponse> RetrieveForCourseAsync(
        //    string query,
        //    Guid courseId,
        //    int topK = 5,
        //    CancellationToken cancellationToken = default);

        ///// <summary>
        ///// Retrieves context for specific lectures
        ///// </summary>
        ///// <param name="query">The search query</param>
        ///// <param name="lectureIds">Lecture IDs to search within</param>
        ///// <param name="topK">Number of chunks to return</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Retrieved chunks</returns>
        //Task<RagRetrievalResponse> RetrieveForLecturesAsync(
        //    string query,
        //    IEnumerable<Guid> lectureIds,
        //    int topK = 5,
        //    CancellationToken cancellationToken = default);

        //#endregion

        //#region Indexing Operations

        ///// <summary>
        ///// Indexes ContextChunks for later retrieval
        ///// </summary>
        ///// <param name="request">The index request with ContextChunks to store</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Indexing result</returns>
        //Task<RagIndexResponse> IndexAsync(
        //    RagIndexRequest request,
        //    CancellationToken cancellationToken = default);

        ///// <summary>
        ///// Indexes a document by chunking and storing it using ChunkMetadata
        ///// </summary>
        ///// <param name="metadata">Full chunk metadata for all resulting chunks</param>
        ///// <param name="content">The full document content</param>
        ///// <param name="chunkingOptions">Options for chunking</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Indexing result</returns>
        //Task<RagIndexResponse> IndexDocumentAsync(
        //    ChunkMetadata metadata,
        //    string content,
        //    ChunkingOptions? chunkingOptions = null,
        //    CancellationToken cancellationToken = default);

        ///// <summary>
        ///// Re-indexes an existing material (delete old + index new)
        ///// </summary>
        ///// <param name="request">The index request</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Indexing result</returns>
        //Task<RagIndexResponse> ReindexAsync(
        //    RagIndexRequest request,
        //    CancellationToken cancellationToken = default);

        //#endregion

        //#region Delete Operations

        ///// <summary>
        ///// Deletes indexed chunks
        ///// </summary>
        ///// <param name="request">Delete request specifying what to delete</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Delete result</returns>
        //Task<RagDeleteResponse> DeleteAsync(
        //    RagDeleteRequest request,
        //    CancellationToken cancellationToken = default);

        ///// <summary>
        ///// Deletes all chunks for a material
        ///// </summary>
        ///// <param name="materialId">Material ID to delete</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Delete result</returns>
        //Task<RagDeleteResponse> DeleteMaterialAsync(
        //    Guid materialId,
        //    CancellationToken cancellationToken = default);

        ///// <summary>
        ///// Deletes all chunks for a lecture
        ///// </summary>
        ///// <param name="lectureId">Lecture ID to delete</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Delete result</returns>
        //Task<RagDeleteResponse> DeleteLectureAsync(
        //    Guid lectureId,
        //    CancellationToken cancellationToken = default);

        ///// <summary>
        ///// Deletes all chunks for a course
        ///// </summary>
        ///// <param name="courseId">Course ID to delete</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Delete result</returns>
        //Task<RagDeleteResponse> DeleteCourseAsync(
        //    Guid courseId,
        //    CancellationToken cancellationToken = default);

        //#endregion

        //#region Utility Operations

        ///// <summary>
        ///// Chunks a document into ContextChunks with provided metadata
        ///// </summary>
        ///// <param name="content">The content to chunk</param>
        ///// <param name="metadata">Metadata to attach to each chunk</param>
        ///// <param name="options">Chunking options</param>
        ///// <returns>Chunking result with ContextChunks ready for indexing</returns>
        //ChunkingResult ChunkDocument(
        //    string content,
        //    ChunkMetadata metadata,
        //    ChunkingOptions? options = null);

        ///// <summary>
        ///// Checks if a material has been indexed
        ///// </summary>
        ///// <param name="materialId">Material ID to check</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>True if indexed, false otherwise</returns>
        //Task<bool> IsMaterialIndexedAsync(
        //    Guid materialId,
        //    CancellationToken cancellationToken = default);

        ///// <summary>
        ///// Gets the chunk count for a material
        ///// </summary>
        ///// <param name="materialId">Material ID</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Number of chunks indexed</returns>
        //Task<int> GetChunkCountAsync(
        //    Guid materialId,
        //    CancellationToken cancellationToken = default);

        ///// <summary>
        ///// Gets indexing statistics for a course
        ///// </summary>
        ///// <param name="courseId">Course ID</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Statistics about indexed content</returns>
        //Task<RagIndexStats> GetIndexStatsAsync(
        //    Guid courseId,
        //    CancellationToken cancellationToken = default);

        //#endregion
    }
}
