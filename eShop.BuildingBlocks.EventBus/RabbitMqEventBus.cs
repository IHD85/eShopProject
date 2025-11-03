using eShop.BuildingBlocks.EventBus;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;


public class RabbitMqEventBus : IEventBus
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly IServiceProvider _serviceProvider;
    private const string ExchangeName = "eshop_exchange";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public RabbitMqEventBus(IConnection connection, IServiceProvider serviceProvider)
    {
        _connection = connection;
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _serviceProvider = serviceProvider;

        _channel.ExchangeDeclareAsync(
            ExchangeName,
            ExchangeType.Topic,
            durable: true
        ).GetAwaiter().GetResult();
    }

    private static string GetEventRoutingKey<T>()
    {
        return typeof(T).Name switch
        {
            "BasketCheckedOutIntegrationEvent" => "basket.checkedout",
            _ => typeof(T).Name.ToLowerInvariant()
        };
    }

    public void Publish(IntegrationEventBase @event)
    {
        var eventType = @event.GetType();                   // ✅ korrekt type
        var routingKey = GetEventRoutingKey(eventType);

        var json = JsonSerializer.Serialize(@event, eventType, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: routingKey,
            mandatory: true,
            body: body
        ).GetAwaiter().GetResult();

        Console.WriteLine($"📤 Published event {routingKey}: {json}");
    }

    private static string GetEventRoutingKey(Type type)
    {
        return type.Name switch
        {
            "BasketCheckedOutIntegrationEvent" => "basket.checkedout",
            _ => type.Name.ToLowerInvariant()
        };
    }

    public void Subscribe<T, TH>()
        where T : IntegrationEventBase
        where TH : IIntegrationEventHandler<T>
    {
        var routingKey = GetEventRoutingKey(typeof(T));
        var queueName = $"{routingKey}.queue";

        _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        ).GetAwaiter().GetResult();

        _channel.QueueBindAsync(queueName, ExchangeName, routingKey)
            .GetAwaiter().GetResult();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            Console.WriteLine($"📥 Received {routingKey}: {json}");
            var eventObj = JsonSerializer.Deserialize<T>(json, _jsonOptions);

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<TH>();
            await handler.Handle(eventObj!);
        };

        _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: consumer
        ).GetAwaiter().GetResult();

        Console.WriteLine($"📩 Subscribed to {typeof(T).Name} on queue '{queueName}'");
    }
}
