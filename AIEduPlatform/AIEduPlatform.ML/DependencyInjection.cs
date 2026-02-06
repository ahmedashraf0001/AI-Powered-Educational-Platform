using AIEduPlatform.Core.Interfaces.Monitors;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using AIEduPlatform.ML.Services;
using AIEduPlatform.ML.Services.health;
using AIEduPlatform.ML.Services.Models;
using AIEduPlatform.ML.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace AIEduPlatform.ML
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMLServices(this IServiceCollection services, IConfiguration configuration)
        {
            var aiSettings = configuration
                .GetSection("AIService")
                .Get<AIServiceSettings>();

            if (aiSettings == null)
            {
                throw new InvalidOperationException("AIService configuration section is missing");
            }

            AIServiceValidator.ValidateSettings(aiSettings);

            services.AddSingleton(aiSettings);

            services.Configure<AIServiceSettings>(
                configuration.GetSection("AIService"));

            services.Configure<RagSettings>(
                configuration.GetSection("RagSettings"));

            services.AddHttpClient<IOllamaServiceClient, OllamaServiceClient>(
                "OllamaService",
                client =>
                {
                    client.BaseAddress = new Uri(aiSettings.BaseUrls.OllamaService);
                    client.Timeout= Timeout.InfiniteTimeSpan;
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("User-Agent", "EducationalPlatform-API");
                })
                .AddPolicyHandler(GetRetryPolicy(aiSettings.Retry))
                .AddPolicyHandler(GetCircuitBreakerPolicy());
            services.AddHttpClient<ITranscriptionService, TranscriptionServiceClient>(
                "TranscriptionService",
                client =>
                {
                    client.BaseAddress = new Uri(aiSettings.BaseUrls.TranscriptionService);
                    client.Timeout = aiSettings.Timeouts.TranscriptionTimeout;
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("User-Agent", "EducationalPlatform-API");
                })
                .AddPolicyHandler(GetRetryPolicy(aiSettings.Retry))
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<IEmbeddingService, EmbeddingServiceClient>(
                "EmbeddingService",
                client =>
                {
                    client.BaseAddress = new Uri(aiSettings.BaseUrls.EmbeddingService);
                    client.Timeout = aiSettings.Timeouts.EmbeddingTimeout;
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("User-Agent", "EducationalPlatform-API");
                })
                .AddPolicyHandler(GetRetryPolicy(aiSettings.Retry))
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<IRerankingService, RerankingServiceClient>(
                "RerankingService",
                client =>
                {
                    client.BaseAddress = new Uri(aiSettings.BaseUrls.RerankingService);
                    client.Timeout = aiSettings.Timeouts.RerankingTimeout;
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("User-Agent", "EducationalPlatform-API");
                })
                .AddPolicyHandler(GetRetryPolicy(aiSettings.Retry))
                .AddPolicyHandler(GetCircuitBreakerPolicy());


            services.AddHttpClient<IVisionService, VisionServiceClient>(
                 "VisionService",
                 client =>
                 {
                     client.BaseAddress = new Uri(aiSettings.BaseUrls.VisionService);
                     client.Timeout = aiSettings.Timeouts.VisionTimeout;
                     client.DefaultRequestHeaders.Add("Accept", "application/json");
                     client.DefaultRequestHeaders.Add("User-Agent", "EducationalPlatform-API");
                 })
                 .AddPolicyHandler(GetRetryPolicy(aiSettings.Retry))
                 .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddScoped<IAIServiceHealthMonitor, AIServiceHealthMonitor>();

            services.AddScoped<IRAGService, RAGService>();

            services.AddSingleton<IContentChunker, ContentChunker>();


            services.AddHealthChecks()
                .AddCheck<AIServiceHealthCheck>(
                    "ai_services",
                    tags: new[] { "ai", "external", "ready" });

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(RetrySettings retrySettings)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retrySettings.MaxRetries,
                    retryAttempt => TimeSpan.FromMilliseconds(
                        retrySettings.RetryDelayMilliseconds * Math.Pow(2, retryAttempt - 1)));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30));
        }
    }
}
