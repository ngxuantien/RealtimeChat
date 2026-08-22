using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Application.Service;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Infrastructure.Repositories;

namespace RealtimeChat.API.Extentions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IConversationMemberService, ConversationMemberService>();
        services.AddScoped<IMessageService, MessageService>();

        return services;
    }
}
