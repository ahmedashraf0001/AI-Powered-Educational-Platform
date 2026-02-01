using AIEduPlatform.ML.Services;

namespace AIEduPlatform.ML.Configurations
{
    public static class AIServiceValidator
    {
        public static void ValidateSettings(AIServiceSettings settings)
        {
            if (settings.BaseUrls == null)
                throw new InvalidOperationException("AIService.BaseUrls configuration is missing");

            if (string.IsNullOrWhiteSpace(settings.BaseUrls.EmbeddingService))
                throw new InvalidOperationException("AIService.BaseUrls.EmbeddingService is not configured");

            if (string.IsNullOrWhiteSpace(settings.BaseUrls.RerankingService))
                throw new InvalidOperationException("AIService.BaseUrls.RerankingService is not configured");

            if (string.IsNullOrWhiteSpace(settings.BaseUrls.OllamaService))
                throw new InvalidOperationException("AIService.BaseUrls.OllamaService is not configured");

            if (string.IsNullOrWhiteSpace(settings.BaseUrls.VisionService))
                throw new InvalidOperationException("AIService.BaseUrls.VisionService is not configured");

            if (settings.Embeddings?.Urls == null)
                throw new InvalidOperationException("AIService.Embeddings.Urls configuration is missing");

            if (settings.Reranker?.Urls == null)
                throw new InvalidOperationException("AIService.Reranker.Urls configuration is missing");

            if (settings.Ollama?.Urls == null)
                throw new InvalidOperationException("AIService.Ollama.Urls configuration is missing");

            if (settings.Vision?.Urls == null)
                throw new InvalidOperationException("AIService.Vision.Urls configuration is missing");
        }
    }
}
