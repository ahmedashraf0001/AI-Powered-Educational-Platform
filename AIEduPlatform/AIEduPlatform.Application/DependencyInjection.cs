using AIEduPlatform.Application.Common.Behaviors;
using AIEduPlatform.Application.Common.Services;
using AIEduPlatform.Application.Features.StudySessions.Commands.Chat.SendChatMessage;
using AIEduPlatform.Application.SignalR;
using AIEduPlatform.Core.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AIEduPlatform.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddOpenBehavior(typeof(MediatRBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly);

            services.AddSingleton<IMaterialIndexingQueue, MaterialIndexingQueue>();
            services.AddScoped<IChatService, ChatService>();
            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
            services.AddScoped<INotificationService, NotificationService>();
            return services;
        }
    }
}
