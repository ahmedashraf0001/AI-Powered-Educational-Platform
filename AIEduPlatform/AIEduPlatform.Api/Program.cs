using AIEduPlatform.Api.BackgroundServices;
using AIEduPlatform.Api.Extensions;
using AIEduPlatform.Api.Middleware;
using AIEduPlatform.Application;
using AIEduPlatform.Application.SignalR;
using AIEduPlatform.Infrastructure;
using AIEduPlatform.ML;
using FastEndpoints;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
namespace AIEduPlatform.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            const long MaxUploadBytes = 100L * 1024 * 1024;

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = MaxUploadBytes;
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = MaxUploadBytes;
            });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddApplication(builder.Configuration);
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddMLServices(builder.Configuration);
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddCorsPolicy();
            builder.Services.AddFastEndpoints();
            builder.Services.AddSwaggerConfiguration();
            builder.Services.AddRateLimitingPolicies();

            builder.Services.AddHostedService<MaterialIndexingBackgroundService>();
builder.Services.AddHostedService<TagExtractionBackgroundService>();
            builder.Services.AddHostedService<CourseTagUpdateBackgroundService>();
            builder.Services.AddHostedService<StaleSessionCleanupService>();
            builder.Services.AddHostedService<AIGradingBackgroundService>();

            var app = builder.Build();

            await SeedRolesAsync(app.Services);

            app.UseSwaggerConfiguration(app.Environment);
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false
            });
            app.UseExceptionHandler();

            // Serve uploaded files (thumbnails, etc.)
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsPath),
                RequestPath = "/uploads"
            });

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            app.UseCors("AllowAll");
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseFastEndpoints(c =>
            {
                c.Serializer.Options.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                c.Serializer.Options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
            app.MapHub<MaterialIndexingHub>("/hubs/material-indexing");
            app.MapHub<StudentNotificationHub>("/hubs/student-notifications");
            app.Run();
        }

        private static async Task SeedRolesAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            string[] roles = ["Student", "Teacher"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
                }
            }
        }
    }
}




