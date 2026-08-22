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
builder.Services.AddCorsPolicy();
builder.Services.AddMongoDb(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddFileStorage(builder.Configuration);

var app = builder.Build();

await app.InitializeMongoIndexesAsync();

app.UseSwaggerDocumentation();

app.UseHttpsRedirection();

app.UseCors(CorsServiceExtensions.AngularAppPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub").RequireCors(CorsServiceExtensions.AngularAppPolicy);

app.Run();