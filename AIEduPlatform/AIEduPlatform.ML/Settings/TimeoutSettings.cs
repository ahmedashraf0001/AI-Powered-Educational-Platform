namespace AIEduPlatform.ML.Configurations
{
    public class TimeoutSettings
    {
        public TimeSpan EmbeddingTimeout { get; set; }
        public TimeSpan RerankingTimeout { get; set; }
        public TimeSpan TranscriptionTimeout { get; set; }
        public TimeSpan VisionTimeout { get; set; }
        public TimeSpan VideoTimeout { get; set; }
        public TimeSpan HealthCheckTimeout { get; set; }
    }
    
}
