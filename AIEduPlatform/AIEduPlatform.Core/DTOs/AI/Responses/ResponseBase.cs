using AIEduPlatform.Core.DTOs.AI.Common;

namespace AIEduPlatform.Core.DTOs.AI.Responses
{
    /// <summary>
    /// Base response wrapper for all AI prompt operations
    /// </summary>
    public class ResponseBase
    {
        /// <summary>
        /// Whether the request was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Error message if the request failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Token usage statistics
        /// </summary>
        public AiTokenUsage TokenUsage { get; set; } = new AiTokenUsage();

        /// <summary>
        /// Model used for generation
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Time taken to generate the response in milliseconds
        /// </summary>
        public long GenerationTimeMs { get; set; }

        /// <summary>
        /// The raw response content from the AI (before parsing)
        /// </summary>
        public string? RawContent { get; set; }

        /// <summary>
        /// Sources used from the context chunks
        /// </summary>
        public List<AiSourceReference> Sources { get; set; } = new List<AiSourceReference>();
    }

    /// <summary>
    /// Generic response wrapper for strongly-typed AI outputs
    /// </summary>
    /// <typeparam name="T">The type of the parsed AI response</typeparam>
    public class Response<T> : ResponseBase where T : class
    {
        /// <summary>
        /// The parsed AI response data
        /// </summary>
        public T? Data { get; set; }
    }
}
