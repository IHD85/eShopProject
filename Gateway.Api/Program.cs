using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks()
    .AddCheck("gateway_alive", () => HealthCheckResult.Healthy("Gateway is running"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "eShop Gateway v1");

        c.SwaggerEndpoint("http://catalog-api:8080/swagger/v1/swagger.json", "Catalog API");
        c.SwaggerEndpoint("http://basket-api:8080/swagger/v1/swagger.json", "Basket API");
        c.SwaggerEndpoint("http://order-api:8080/swagger/v1/swagger.json", "Order API");
        c.SwaggerEndpoint("http://identity-api:8080/swagger/v1/swagger.json", "Identity API");

        c.RoutePrefix = "swagger"; // Swagger vises på /swagger
    });
}

// --- Test route ---
app.MapGet("/", () => "YARP Gateway is running...");

app.MapGet("/health", () => Results.Ok("Healthy"));

app.MapReverseProxy();

app.Run();
