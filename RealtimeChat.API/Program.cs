using Microsoft.AspNetCore.HttpOverrides;
using RealtimeChat.API.Extentions;
using RealtimeChat.API.Hubs;
using RealtimeChat.API.Middlewares;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddMongoDb(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddFileStorage(builder.Configuration);
builder.Services.AddEmailSender(builder.Configuration);
builder.Services.AddRateLimitingPolicies();

var app = builder.Build();

await app.InitializeMongoIndexesAsync();

app.UseSwaggerDocumentation();

// Azure App Service terminates TLS at its reverse proxy and forwards plain HTTP internally,
// so Kestrel needs these headers to know the original request was HTTPS (avoids a redirect loop).
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

app.UseCors(CorsServiceExtensions.AngularAppPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionMiddleware>();

app.UseRateLimiter();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub").RequireCors(CorsServiceExtensions.AngularAppPolicy);

app.Run();