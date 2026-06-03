using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Infrastructure.Mongo;
using RealtimeChat.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();