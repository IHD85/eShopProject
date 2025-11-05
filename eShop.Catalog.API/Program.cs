using eShop.BuildingBlocks.EventBus;
using eShop.BuildingBlocks.EventBus.Events;
using eShop.Catalog.API.IntegrationEvents.Handlers;
using eShop.Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// 🔹 Registrér Event Handler
builder.Services.AddTransient<OrderCreatedIntegrationEventHandler>();

// 🔹 JWT start
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true, // ✅ vigtigt for tokenens udløb
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] ?? "ThisIsASuperLongJwtSecretKey_ChangeMe123456789"))
        };

        // 🔍 midlertidig debug-logging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("🔴 JWT authentication failed:");
                Console.WriteLine(context.Exception.ToString());
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ Token validated successfully for user: " +
                                  context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
// 🔹 JWT slut

var app = builder.Build();

// --- Automatisk migration af CatalogDb ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate(); // ✅ Opretter tabeller (CatalogItems, CatalogBrands, CatalogTypes)
}

// --- Subscribe til RabbitMQ events ---
_ = Task.Run(async () =>
{
    var maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

            Console.WriteLine("📩 [Catalog.API] Subscribed to OrderCreatedIntegrationEvent");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Retry {i + 1}/{maxRetries}: RabbitMQ not ready - {ex.Message}");
            await Task.Delay(delay);
        }
    }
});

// --- Swagger ---
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Tilføj middleware
app.UseAuthentication();
app.UseAuthorization();

// --- Health & Controllers ---
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
