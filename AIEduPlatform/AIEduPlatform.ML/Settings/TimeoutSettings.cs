namespace AIEduPlatform.ML.Configurations
{
    public class TimeoutSettings
    {
        public TimeSpan EmbeddingTimeout { get; set; }
        public TimeSpan RerankingTimeout { get; set; }
        public TimeSpan OllamaTimeout { get; set; }
        public TimeSpan VisionTimeout { get; set; }
        public TimeSpan HealthCheckTimeout { get; set; }
    }
    
}
