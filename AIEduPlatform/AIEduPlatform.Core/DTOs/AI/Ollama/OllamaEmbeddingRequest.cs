using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Ollama
{
    /// <summary>
    /// Request body for Ollama /api/embed endpoint
    /// </summary>
    public class OllamaEmbeddingRequest
    {
        /// <summary>
        /// The embedding model to use (e.g., "nomic-embed-text", "mxbai-embed-large")
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = "nomic-embed-text";

        /// <summary>
        /// Text to generate embeddings for (single string or array)
        /// </summary>
        [JsonPropertyName("input")]
        public object Input { get; set; } = string.Empty;

        /// <summary>
        /// Truncate input to fit context length
        /// </summary>
        [JsonPropertyName("truncate")]
        public bool? Truncate { get; set; }

        /// <summary>
        /// Keep model loaded in memory
        /// </summary>
        [JsonPropertyName("keep_alive")]
        public string? KeepAlive { get; set; }

        /// <summary>
        /// Additional options
        /// </summary>
        [JsonPropertyName("options")]
        public OllamaOptions? Options { get; set; }

        /// <summary>
        /// Create request for single text
        /// </summary>
        public static OllamaEmbeddingRequest ForText(string text, string model = "nomic-embed-text") =>
            new() { Model = model, Input = text };

        /// <summary>
        /// Create request for multiple texts (batch)
        /// </summary>
        public static OllamaEmbeddingRequest ForTexts(List<string> texts, string model = "nomic-embed-text") =>
            new() { Model = model, Input = texts };
    }

    /// <summary>
    /// Response from Ollama /api/embed endpoint
    /// </summary>
    public class OllamaEmbeddingResponse
    {
        /// <summary>
        /// The model used for embedding
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// The embedding vectors (one per input text)
        /// </summary>
        [JsonPropertyName("embeddings")]
        public List<float[]> Embeddings { get; set; } = new List<float[]>();

        /// <summary>
        /// Total duration in nanoseconds
        /// </summary>
        [JsonPropertyName("total_duration")]
        public long TotalDuration { get; set; }

        /// <summary>
        /// Time to load model in nanoseconds
        /// </summary>
        [JsonPropertyName("load_duration")]
        public long LoadDuration { get; set; }

        /// <summary>
        /// Number of tokens processed
        /// </summary>
        [JsonPropertyName("prompt_eval_count")]
        public int PromptEvalCount { get; set; }
    }
}
