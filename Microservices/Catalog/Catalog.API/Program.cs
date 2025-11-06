using Catalog.API.Data;
using Catalog.API.Events;
using Catalog.API.Services.EventBusBackgroundConsumer;
using Catalog.API.Services.EventBusService;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQEventBus.Abstractions;
using RabbitMQEventBus.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<TestEventHandler>();

builder.Services.AddRabbitMQEventBus(builder.Configuration)
    .AddSubscription<TestEvent, TestEventHandler>();
//builder.Services.AddSingleton<EventBusService>();
//builder.Services.AddSingleton<IEventBusService>(sp => sp.GetRequiredService<EventBusService>());

//builder.Services.AddHostedService<EventBusBackgroundConsumer>();




builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

//var eventBus = app.Services.GetRequiredService<EventBusService>();
//await eventBus.InitializeAsync();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else if (app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
