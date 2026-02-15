using System.Reflection;
using AIEduPlatform.Application.Common.Behaviors;
using AIEduPlatform.Application.Common.Services;
using AIEduPlatform.Application.Features.StudySessions.Commands.Chat.SendChatMessage;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

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

            return services;
        }
    }
}
