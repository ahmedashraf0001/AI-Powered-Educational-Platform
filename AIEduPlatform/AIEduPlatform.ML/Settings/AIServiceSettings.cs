using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.ML.Configurations
{
    public class AIServiceSettings
    {
        public BaseUrlsSettings BaseUrls { get; set; }
        public EmbeddingSettings Embeddings { get; set; }
        public RerankerSettings Reranker { get; set; }
        public OllamaSettings Ollama { get; set; }
        public GroqSettings? Groq { get; set; }
        public string ActiveProvider { get; set; } = "groq";
        public VisionSettings Vision { get; set; }
        public VideoSettings Video { get; set; }
        public TranscriptionSettings Transcription { get; set; }
        public TimeoutSettings Timeouts { get; set; }
        public RetrySettings Retry { get; set; }
    }  
}
