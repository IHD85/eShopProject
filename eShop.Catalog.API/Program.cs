using eShop.BuildingBlocks.EventBus;
using eShop.Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

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
    .AddNpgSql(builder.Configuration.GetConnectionString("CatalogDb")!)
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
        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }, name: "rabbitmq_health");

// --- PostgreSQL ---
builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDb"));
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

// --- EventBus ---
builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();

var app = builder.Build();

// --- Automatisk migration af CatalogDb ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate(); // ✅ Opretter tabeller (CatalogItems, CatalogBrands, CatalogTypes)
}

// --- Swagger ---
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Health & Controllers ---
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
