using eShop.Order.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using eShop.Order.API.IntegrationEvents.Events;
using eShop.Order.API.IntegrationEvents.Handlers;
using System.Text.Json.Serialization;
using eShop.Order.API.Extensions;
using RabbitMQEventBus.Abstractions;
using RabbitMQEventBus.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


builder.ConfigureOpenTelemetry();

builder.Services.AddHealthChecks();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrderDb"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null
        ));
});



builder.Services.AddRabbitMQEventBus(builder.Configuration)
    .AddSubscription<BasketCheckedOutIntegrationEvent, BasketCheckedOutIntegrationEventHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate(); 
}

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
