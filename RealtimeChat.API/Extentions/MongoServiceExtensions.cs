using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RealtimeChat.Infrastructure.Mongo;

namespace RealtimeChat.API.Extentions;

public static class MongoServiceExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton<MongoDbContext>();
        services.AddScoped<MongoDbIndexInitializer>();

        return services;
    }

    public static async Task InitializeMongoIndexesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var indexInitializer = scope.ServiceProvider.GetRequiredService<MongoDbIndexInitializer>();
        await indexInitializer.CreateIndexesAsync();
    }
}
