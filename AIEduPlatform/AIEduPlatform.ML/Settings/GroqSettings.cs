using AIEduPlatform.Core.DTOs.AI.Ollama;

namespace AIEduPlatform.ML.Configurations
{
    public class GroqSettings 
    {
        public string ApiKey { get; set; }
        public string Model { get; set; }
        public GroqOptions? Options { get; init; }
        public GroqUrlsSettings Urls { get; set; }
        public HealthEndpointsSettings Health { get; set; }

    }
}
