using Catalog.API.Data;
using Catalog.API.Events;
using Catalog.API.Extensions;
using Catalog.API.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using RabbitMQEventBus.Extensions;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
        .WriteTo.OpenTelemetry(
            endpoint: builder.Configuration.GetValue<string>("Otlp:Endpoint") ?? "http://otel-lgtm:4317/",
            protocol: Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc)
        .Enrich.FromLogContext();
});

builder.ConfigureOpenTelemetry();




builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null
        ));
});

builder.Services.AddTransient<TestEventHandler>();

builder.Services.AddRabbitMQEventBus(builder.Configuration)
    .AddSubscription<TestEvent, TestEventHandler>();
//builder.Services.AddSingleton<EventBusService>();
//builder.Services.AddSingleton<IEventBusService>(sp => sp.GetRequiredService<EventBusService>());
builder.Services.AddHealthChecks();
//builder.Services.AddHostedService<EventBusBackgroundConsumer>();




builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



var app = builder.Build();

//var eventBus = app.Services.GetRequiredService<EventBusService>();
//await eventBus.InitializeAsync();


// Configure the HTTP request pipeline.

    app.MapOpenApi();
    app.MapScalarApiReference();


// app.UseHttpsRedirection();

app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate(); 
}
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();


