namespace RealtimeChat.API.Extentions;

public static class CorsServiceExtensions
{
    public const string AngularAppPolicy = "AllowAngularApp";

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(AngularAppPolicy, policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
