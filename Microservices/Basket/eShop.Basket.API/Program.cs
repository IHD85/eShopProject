using System.Net.Sockets;
using eShop.Basket.API.EventHandlers;
using StackExchange.Redis;
using eShop.Basket.Application.Services;
using eShop.Basket.Domain.Events;
using RabbitMQEventBus.Extensions;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<BasketService>();
builder.Services.AddHealthChecks();
builder.Services.AddRabbitMQEventBus(builder.Configuration)
    .AddSubscription<ProductPriceChangedEvent, ProductPriceChangedEventHandler>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetSection("Redis");
    var redisHost = config["Host"] ?? "localhost";
    var redisPort = config["Port"] ?? "6379";

var connectionString = $"{redisHost}:{redisPort},allowAdmin=true";

    const int maxRetries = 10;
    const int delaySeconds = 3;

    int attempts = 0;

    while (true)
    {
        try
        {
            return ConnectionMultiplexer.Connect(connectionString);
        }
        catch (RedisConnectionException) when (attempts < maxRetries)
        {
            attempts++;
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
        }
        catch (SocketException) when (attempts < maxRetries)
        {
            attempts++;
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
        }
    }
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; 
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseAuthorization();




app.MapControllers();

app.MapHealthChecks("/health");

app.MapGet("/", () => $"Basket.API running in {app.Environment.EnvironmentName}");

app.Run();
