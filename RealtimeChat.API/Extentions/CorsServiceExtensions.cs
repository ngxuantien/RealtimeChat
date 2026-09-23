namespace RealtimeChat.API.Extentions;

public static class CorsServiceExtensions
{
    public const string AngularAppPolicy = "AllowAngularApp";

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (allowedOrigins == null || allowedOrigins.Length == 0)
            allowedOrigins = ["http://localhost:4200"];

        services.AddCors(options =>
        {
            options.AddPolicy(AngularAppPolicy, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
