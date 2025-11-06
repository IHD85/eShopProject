using eShop.Identity.Application.Services;
using eShop.Identity.Infrastructure.Data;
using eShop.Identity.Infrastructure.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client; // kun for health check


var builder = WebApplication.CreateBuilder(args);

// --- Konfiguration ---
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Health Checks ---
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("IdentityDb")!)
    .AddRabbitMQ(sp =>
    {
        var env = builder.Environment.EnvironmentName;
        var config = builder.Configuration.GetSection("RabbitMQ");
        var host = env == "Development" ? "localhost" : (config["Host"] ?? "rabbitmq");

        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = config["Username"] ?? "guest",
            Password = config["Password"] ?? "guest"
        };
        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }, name: "rabbitmq_health");

// --- PostgreSQL ---
builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityDb"));
});

// --- JWT-konfiguration ---
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretJwtKey12345!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "eShopGateway";
builder.Services.AddSingleton(new TokenGenerator(jwtKey, jwtIssuer));

// --- Services ---
builder.Services.AddScoped<IdentityService>();

// --- Authentication ---
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

// --- Automatisk migration af IdentityDb ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    db.Database.Migrate(); // ✅ opretter tabeller automatisk i Docker og lokalt
}

// --- Swagger ---
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Middleware ---
app.UseAuthentication();
app.UseAuthorization();

// --- Endpoints ---
app.MapControllers();
app.MapHealthChecks("/health"); // ✅ HealthCheck endpoint

app.Run();
