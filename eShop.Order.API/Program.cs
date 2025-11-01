using eShop.BuildingBlocks.EventBus;
using eShop.Order.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using eShop.Order.API.IntegrationEvents.Events;
using eShop.Order.API.IntegrationEvents.Handlers;

var builder = WebApplication.CreateBuilder(args);

// --- Konfiguration ---
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// --- Services ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Bevar PascalCase
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Health Checks ---
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("OrderDb")!)
    .AddRabbitMQ(sp =>
    {
        var config = builder.Configuration.GetSection("RabbitMQ");
        var env = builder.Environment.EnvironmentName;

        var host = env == "Development" ? "localhost" : (config["Host"] ?? "rabbitmq");

        var factory = new ConnectionFactory()
        {
            HostName = host,
            UserName = config["Username"] ?? "guest",
            Password = config["Password"] ?? "guest"
        };

        // Returnér en testforbindelse til HealthCheck
        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }, name: "rabbitmq_health");

// --- PostgreSQL ---
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrderDb"));
});

// --- RabbitMQ Connection (bruges af EventBus) ---
builder.Services.AddSingleton<IConnection>(sp =>
{
    var config = builder.Configuration.GetSection("RabbitMQ");
    var env = builder.Environment.EnvironmentName;

    var host = env == "Development" ? "localhost" : (config["Host"] ?? "rabbitmq");

    var factory = new ConnectionFactory()
    {
        HostName = host,
        UserName = config["Username"] ?? "guest",
        Password = config["Password"] ?? "guest"
    };

    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// --- EventBus og Handler ---
builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();
builder.Services.AddTransient<BasketCheckedOutIntegrationEventHandler>();

var app = builder.Build();

// --- Swagger ---
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Health & Controllers ---
app.MapControllers();
app.MapHealthChecks("/health");

// ✅ Subscribe til Basket event ved opstart
using (var scope = app.Services.CreateScope())
{
    var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
    eventBus.Subscribe<BasketCheckedOutIntegrationEvent, BasketCheckedOutIntegrationEventHandler>();
}

app.Run();
