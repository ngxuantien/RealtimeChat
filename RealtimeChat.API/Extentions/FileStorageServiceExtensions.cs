using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Infrastructure.Storage;

namespace RealtimeChat.API.Extentions;

public static class FileStorageServiceExtensions
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CloudinaryOptions>(configuration.GetSection("Cloudinary"));
        services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();

        return services;
    }
}
