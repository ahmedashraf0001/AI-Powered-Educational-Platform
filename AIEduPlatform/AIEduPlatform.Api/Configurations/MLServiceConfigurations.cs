using AIEduPlatform.Core.Interfaces.Monitors;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Services;
using AIEduPlatform.ML.Services.health;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.ML.Configurations
{
    internal static class MLServiceConfigurations
    {
        public static void ConfigureMLServiceSettings(this WebApplicationBuilder builder)
        {
            var aiSettings = builder.Configuration
                .GetSection("AIService")
                .Get<AIServiceSettings>();

            if (aiSettings == null)
            {
                throw new InvalidOperationException("AIService configuration section is missing");
            }

            ValidateAIServiceSettings(aiSettings);

            builder.Services.AddSingleton(aiSettings);

            builder.Services.Configure<AIServiceSettings>(
                builder.Configuration.GetSection("AIService"));

            builder.Services.AddHttpClient<IEmbeddingService, EmbeddingServiceClient>(
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

            builder.Services.AddHttpClient<IRerankingService, RerankingServiceClient>(
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

            builder.Services.AddHttpClient<IOllamaService, OllamaServiceClient>(
                 "OllamaService",
                 client =>
                 {
                     client.BaseAddress = new Uri(aiSettings.BaseUrls.OllamaService);
                     client.Timeout = aiSettings.Timeouts.OllamaTimeout;
                     client.DefaultRequestHeaders.Add("Accept", "application/json");
                     client.DefaultRequestHeaders.Add("User-Agent", "EducationalPlatform-API");
                 })
                 .AddPolicyHandler(GetRetryPolicy(aiSettings.Retry))
                 .AddPolicyHandler(GetCircuitBreakerPolicy());

            builder.Services.AddScoped<IAIServiceHealthMonitor, AIServiceHealthMonitor>();

    
            builder.Services.AddHealthChecks()
                .AddCheck<AIServiceHealthCheck>(
                    "ai_services",
                    tags: new[] { "ai", "external", "ready" });

        }
        private static void ValidateAIServiceSettings(AIServiceSettings settings)
        {
            if (settings.BaseUrls == null)
                throw new InvalidOperationException("AIService.BaseUrls configuration is missing");

            if (string.IsNullOrWhiteSpace(settings.BaseUrls.EmbeddingService))
                throw new InvalidOperationException("AIService.BaseUrls.EmbeddingService is not configured");

            if (string.IsNullOrWhiteSpace(settings.BaseUrls.RerankingService))
                throw new InvalidOperationException("AIService.BaseUrls.RerankingService is not configured");

            if (string.IsNullOrWhiteSpace(settings.BaseUrls.OllamaService))
                throw new InvalidOperationException("AIService.BaseUrls.OllamaService is not configured");

            if (settings.Embeddings?.Urls == null)
                throw new InvalidOperationException("AIService.Embeddings.Urls configuration is missing");

            if (settings.Reranker?.Urls == null)
                throw new InvalidOperationException("AIService.Reranker.Urls configuration is missing");

            if (settings.Ollama?.Urls == null)
                throw new InvalidOperationException("AIService.Ollama.Urls configuration is missing");
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
                    //onRetry: (outcome, timespan, retryCount, context) =>
                    //{
                    //    var logger = context.GetLogger();
                    //    logger?.LogWarning(
                    //        "Retry {RetryCount} after {Delay}ms due to {Result}",
                    //        retryCount,
                    //        timespan.TotalMilliseconds,
                    //        outcome.Result?.StatusCode ?? System.Net.HttpStatusCode.InternalServerError);
                    //});
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, breakDelay) =>
                    {
                        // Log circuit breaker opened
                    },
                    onReset: () =>
                    {
                        // Log circuit breaker reset
                    });
        }
    }
}
