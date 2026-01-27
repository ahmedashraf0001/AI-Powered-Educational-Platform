using AIEduPlatform.Core.Interfaces.Monitors;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using AIEduPlatform.ML.Services;
using AIEduPlatform.ML.Services.health;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddHttpClient<IEmbeddingService, EmbeddingServiceClient>(
                "EmbeddingService",
                client =>
                {
                    client.BaseAddress = new Uri(aiSettings.BaseUrls.EmbeddingService);
                    client.Timeout = aiSettings.Timeouts.EmbeddingTimeout;
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("User-Agent", "EducationalPlatform-API");
                });

            services.AddHttpClient<IRerankingService, RerankingServiceClient>(
                "RerankingService",
                client =>
                {
                    client.BaseAddress = new Uri(aiSettings.BaseUrls.RerankingService);
                    client.Timeout = aiSettings.Timeouts.RerankingTimeout;
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("User-Agent", "EducationalPlatform-API");
                });

            services.AddHttpClient<IOllamaService, OllamaServiceClient>(
                 "OllamaService",
                 client =>
                 {
                     client.BaseAddress = new Uri(aiSettings.BaseUrls.OllamaService);
                     client.Timeout = aiSettings.Timeouts.OllamaTimeout;
                     client.DefaultRequestHeaders.Add("Accept", "application/json");
                     client.DefaultRequestHeaders.Add("User-Agent", "EducationalPlatform-API");
                 });

            services.AddScoped<IAIServiceHealthMonitor, AIServiceHealthMonitor>();

            services.AddHealthChecks()
                .AddCheck<AIServiceHealthCheck>(
                    "ai_services",
                    tags: new[] { "ai", "external", "ready" });

            return services;
        }
    }
}
