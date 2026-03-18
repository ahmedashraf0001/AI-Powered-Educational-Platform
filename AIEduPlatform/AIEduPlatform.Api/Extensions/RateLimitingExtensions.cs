using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace AIEduPlatform.Api.Extensions
{
    public static class RateLimitingExtensions
    {
        public const string LoginPolicy = "login";
        public const string AiEndpointsPolicy = "ai";
        public const string FileUploadPolicy = "file-upload";
        public const string DefaultPolicy = "default";

        public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Login / Register — prevent brute-force (5 requests per 60 seconds per IP)
                options.AddPolicy(LoginPolicy, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromSeconds(60),
                            QueueLimit = 0
                        }));

                // AI / Chat endpoints — expensive, limit per user (10 requests per 60 seconds)
                options.AddPolicy(AiEndpointsPolicy, httpContext =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: httpContext.User.FindFirst("uid")?.Value
                                      ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                      ?? "unknown",
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 10,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(60),
                            TokensPerPeriod = 10,
                            QueueLimit = 2,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        }));

                // File upload — limit per user (20 per 5 minutes)
                options.AddPolicy(FileUploadPolicy, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.FindFirst("uid")?.Value
                                      ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                      ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 20,
                            Window = TimeSpan.FromMinutes(5),
                            QueueLimit = 0
                        }));

                // Global default — per IP rate limit
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 500,
                            Window = TimeSpan.FromSeconds(60),
                            QueueLimit = 0
                        }));
            });

            return services;
        }
    }
}
