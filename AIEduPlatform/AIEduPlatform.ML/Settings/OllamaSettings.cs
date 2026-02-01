using AIEduPlatform.Core.DTOs.AI.Ollama;

namespace AIEduPlatform.ML.Configurations
{
    public class OllamaSettings
    {
        public string Model { get; set; } 
        public string KeepAlive { get; init; }
        public OllamaOptions? Options { get; init; }
        public OllamaUrlsSettings Urls { get; set; }
        public HealthEndpointsSettings Health { get; set; }
    }
}
