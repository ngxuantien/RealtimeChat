using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Infrastructure.Email;

namespace RealtimeChat.API.Extentions;

public static class EmailServiceExtensions
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
