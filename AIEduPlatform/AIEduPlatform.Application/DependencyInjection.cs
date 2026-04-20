using AIEduPlatform.Application.Common.Behaviors;
using AIEduPlatform.Application.Common.Services;
using AIEduPlatform.Application.Features.StudySessions.Commands.Chat.SendChatMessage;
using AIEduPlatform.Application.SignalR;
using AIEduPlatform.Core.DTOs.Recommedation;
using AIEduPlatform.Core.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AIEduPlatform.Application
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.Configure<UserTagConfiguration>(configuration.GetSection("UserTagSettings"));

            services.Configure<RecommendationWeightsDto>(configuration.GetSection("Recommendation"));

            services.Configure<CandidateGenerationDto>(configuration.GetSection("CandidateGeneration"));

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddOpenBehavior(typeof(MediatRBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly);

            services.AddSingleton<IMaterialIndexingQueue, MaterialIndexingQueue>();
            services.AddSingleton<ITagExtractionQueue, TagExtractionQueue>();
            services.AddSingleton<IAIGradingQueue, AIGradingQueue>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IUserTagService, UserTagService>();
            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IRecommendationService, RecommendationService>();
            return services;
        }
    }
}

