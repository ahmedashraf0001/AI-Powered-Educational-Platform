using FastEndpoints.Swagger;

namespace AIEduPlatform.Api.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.SwaggerDocument(o =>
            {
                o.DocumentSettings = s =>
                {
                    s.Title = "AIEduPlatform API";
                    s.Version = "v1";
                    s.Description = "AI-Powered Educational Platform API";
                };
                o.EnableJWTBearerAuth = true;
                o.AutoTagPathSegmentIndex = 100;
            });

            return services;
        }

        public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwaggerGen();
            return app;
        }
    }
}
