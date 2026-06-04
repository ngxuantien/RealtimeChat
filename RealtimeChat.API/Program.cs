using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Application.Service;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Infrastructure.Mongo;
using RealtimeChat.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new()
    {
        Title = "Realtime Chat API",
        Version = "v1"
    });
});

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<MongoDbIndexInitializer>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var indexInitializer = scope.ServiceProvider
        .GetRequiredService<MongoDbIndexInitializer>();

    await indexInitializer.CreateIndexesAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(option =>
    {
        option.SwaggerEndpoint("/swagger/v1/swagger.json", "Realtime Chat API v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();