using AIEduPlatform.Core.Interfaces.Monitors;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.Infrastructure.Repositories;
using AIEduPlatform.ML.Configurations;
using AIEduPlatform.ML.MaterialProcessing;
using AIEduPlatform.ML.Services;
using AIEduPlatform.ML.Services.health;
using AIEduPlatform.ML.Services.Material_Processing;
using AIEduPlatform.ML.Services.Models;
using AIEduPlatform.ML.Services.RAG;
using AIEduPlatform.ML.Services.Utilities;
using AIEduPlatform.ML.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Xabe.FFmpeg;

namespace AIEduPlatform.ML
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMLServices(this IServiceCollection services, IConfiguration configuration)
        {
            var aiSettings = configuration
                .GetSection("AIService")
                .Get<AIServiceSettings>();

            var ragSettings = configuration
                .GetSection("RagSettings")
                .Get<RagSettings>();

            if (aiSettings == null)
            {
                throw new InvalidOperationException("AIService configuration section is missing");
            }
            if (ragSettings == null)
            {
                throw new InvalidOperationException("ragSettings configuration section is missing");
            }
            AIServiceValidator.ValidateSettings(aiSettings);

            services.AddSingleton(aiSettings);

            services.Configure<AIServiceSettings>(
                configuration.GetSection("AIService"));

            services.Configure<RagSettings>(
                configuration.GetSection("RagSettings"));

            services.AddHttpClient<OllamaServiceClient>(
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

            // Register Groq HTTP client (only if configured)
            var groqBaseUrl = aiSettings.BaseUrls.GroqService ?? "https://api.groq.com";
            services.AddHttpClient<GroqServiceClient>(
                "GroqService",
                client =>
                {
                    client.BaseAddress = new Uri(groqBaseUrl);
                    client.Timeout = Timeout.InfiniteTimeSpan;
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("User-Agent", "EducationalPlatform-API");
                    if (!string.IsNullOrWhiteSpace(aiSettings.Groq?.ApiKey))
                    {
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {aiSettings.Groq.ApiKey}");
                    }
                })
                .AddPolicyHandler(GetRetryPolicy(aiSettings.Retry))
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Register LLM provider manager (singleton — tracks active provider at runtime)
            services.AddSingleton<ILlmProviderManager, LlmProviderManager>();

            // Factory: resolve IOllamaServiceClient to Ollama or Groq based on active provider
            services.AddTransient<IOllamaServiceClient>(sp =>
            {
                var manager = sp.GetRequiredService<ILlmProviderManager>();
                return manager.ActiveProvider switch
                {
                    "groq" => sp.GetRequiredService<GroqServiceClient>(),
                    _ => sp.GetRequiredService<OllamaServiceClient>()
                };
            });
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


            services.AddHttpClient<IVideoService, VideoServiceClient>(
                 "VideoService",
                 client =>
                 {
                     client.BaseAddress = new Uri(aiSettings.BaseUrls.VideoService);
                     client.Timeout = aiSettings.Timeouts.VideoTimeout;
                     client.DefaultRequestHeaders.Add("Accept", "application/json");
                     client.DefaultRequestHeaders.Add("User-Agent", "EducationalPlatform-API");
                 })
                 .AddPolicyHandler(GetRetryPolicy(aiSettings.Retry))
                 .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddScoped<IAIServiceHealthMonitor, AIServiceHealthMonitor>();

            services.AddScoped<IRAGService, RAGService>();



            services.AddSingleton<IDocumentContentExtractor, DocumentContentExtractor>();

            services.AddSingleton<IAudioTranscriptionChunker, AudioTranscriptionChunker>();

            services.AddScoped<DocumentIndexingHelper>();

            services.AddScoped<AudioIndexingHelper>();

            services.AddScoped<ImageIndexingHelper>();

            services.AddScoped<VideoIndexingHelper>();
            services.AddScoped<IConceptExtractionService, ConceptExtractionService>();
            services.AddScoped<IGraphMergeService, GraphMergeService>();
            services.AddScoped<IQueryIntelligenceService, QueryIntelligenceService>();
            services.AddScoped<IConceptRepository, ConceptRepository>();

            services.AddSingleton<IRerankConcurrencyLimiter>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<RagSettings>>().Value;

                return new RerankConcurrencyLimiter(
                    settings.Concurrency.MaxConcurrentReranking
                );
            });

            FFmpeg.SetExecutablesPath(configuration["FFmpegPath"]);

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
