using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Tilføj YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Tilføj HealthChecks
builder.Services.AddHealthChecks()
    .AddCheck("gateway_alive", () => HealthCheckResult.Healthy("Gateway is running"));

var app = builder.Build();

// Test route
app.MapGet("/", () => "YARP Gateway is running...");

// HealthCheck endpoint
app.MapGet("/health", () => Results.Ok("Healthy")); // ✅ Docker kræver et klart "Healthy" svar (HTTP 200)

// Reverse Proxy routes
app.MapReverseProxy();

app.Run();
