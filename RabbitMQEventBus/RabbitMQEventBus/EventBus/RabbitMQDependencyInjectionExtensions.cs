using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQEventBus.Abstractions;
using RabbitMQEventBus.EventBus;

namespace RabbitMQEventBus.Extensions
{

    public interface IEventBusBuilder
    {
        IServiceCollection Services { get; }
    }

    public sealed class EventBusBuilder : IEventBusBuilder
    {
        public IServiceCollection Services { get; }

        public EventBusBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }

    public static class ServiceCollectionExtensions
    {
       
        public static IEventBusBuilder AddRabbitMQEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            var hostName = configuration["RabbitMQ:Host"];
            var userName = configuration["RabbitMQ:User"];
            var password = configuration["RabbitMQ:Password"];

            services.AddSingleton(new ConnectionFactory
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
            });

            services.Configure<EventBusSubscriptionInfo>(_ => { });
            services.AddSingleton<IEventBus, EventBusRabbitMQ>();

            services.AddHostedService(sp => (EventBusRabbitMQ)sp.GetRequiredService<IEventBus>());

            return new EventBusBuilder(services);
        }

      
        public static IEventBusBuilder ConfigureJsonOptions(this IEventBusBuilder eventBusBuilder, Action<JsonSerializerOptions> configure)
        {
            eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
            {
                configure(o.JsonSerializerOptions);
            });

            return eventBusBuilder;
        }

      
        public static IEventBusBuilder AddSubscription<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TH>(
            this IEventBusBuilder eventBusBuilder)
            where T : IntegrationEvent
            where TH : class, IIntegrationEventHandler<T>
        {
            eventBusBuilder.Services.AddKeyedScoped<IIntegrationEventHandler, TH>(typeof(T));

            eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
            {
                o.EventTypes[typeof(T).Name] = typeof(T);
            });

            return eventBusBuilder;
        }
    }
}
