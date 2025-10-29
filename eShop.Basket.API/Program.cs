using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Indlæs konfiguration (inkl. Docker)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// --- Services ---

// Redis cache
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetSection("Redis");
    var redisHost = config["Host"] ?? "localhost";
    var redisPort = config["Port"] ?? "6379";
    return ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}");
});

// ✅ RabbitMQ connection - v7.0.0 kræver IConnectionFactory interface
builder.Services.AddSingleton<IConnection>(sp =>
{
    var config = builder.Configuration.GetSection("RabbitMQ");

    // Opret factory via interface
    IConnectionFactory factory = new ConnectionFactory()
    {
        HostName = config["Host"] ?? "localhost",
        UserName = config["Username"] ?? "guest",
        Password = config["Password"] ?? "guest"
    };

    // ✅ Ny måde i v7.0.0: brug IConnectionFactoryExtensions
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// Controllers, Swagger og HealthCheck
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddCheck("basket_api_alive", () => HealthCheckResult.Healthy("Basket API is running"))
    .AddRedis(
        $"{builder.Configuration["Redis:Host"]}:{builder.Configuration["Redis:Port"]}",
        name: "redis_health")
    .AddRabbitMQ(sp =>
    {
        var config = builder.Configuration.GetSection("RabbitMQ");
        IConnectionFactory factory = new ConnectionFactory()
        {
            HostName = config["Host"] ?? "localhost",
            UserName = config["Username"] ?? "guest",
            Password = config["Password"] ?? "guest"
        };
        return factory.CreateConnectionAsync().GetAwaiter().GetResult(); // ✅ samme ændring her
    },
    name: "rabbitmq_health");

var app = builder.Build();

// Swagger i Docker og lokal udvikling
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// HealthCheck endpoint
app.MapHealthChecks("/health");

// Root test
app.MapGet("/", () => $"Basket.API running in {app.Environment.EnvironmentName}");

app.Run();
