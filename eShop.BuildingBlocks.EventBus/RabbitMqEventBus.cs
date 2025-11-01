using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;


namespace eShop.BuildingBlocks.EventBus
{
    public class RabbitMqEventBus : IEventBus
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly IServiceProvider _serviceProvider;
        private const string ExchangeName = "eshop_exchange";

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = null, // 🔹 Bevar PascalCase
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

        public void Publish(IntegrationEventBase @event)
        {
            var routingKey = @event.GetType().Name.ToLowerInvariant();

            // vigtig: brug runtime-typen
            var message = JsonSerializer.Serialize(@event, @event.GetType(), _jsonOptions);

            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: routingKey,
                mandatory: true,
                body: body
            ).GetAwaiter().GetResult();

            Console.WriteLine($"📤 Published event {routingKey}: {message}");
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEventBase
            where TH : IIntegrationEventHandler<T>
        {
            var routingKey = typeof(T).Name.ToLowerInvariant();
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
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var eventObj = JsonSerializer.Deserialize<T>(json, _jsonOptions); // ✅ Brug samme options

                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<TH>();
                await handler.Handle(eventObj!);
            };

            _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: true,
                consumer: consumer
            ).GetAwaiter().GetResult();
        }
    }
}
