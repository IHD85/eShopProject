using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace eShop.BuildingBlocks.EventBus
{
    public class RabbitMqEventBus : IEventBus, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly ILogger<RabbitMqEventBus> _logger;

        public RabbitMqEventBus(ILogger<RabbitMqEventBus> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            // Opret forbindelse og channel (RabbitMQ v7)
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            // Opret exchange (RabbitMQ v7)
            _channel.ExchangeDeclareAsync(
                exchange: "eshop_exchange",
                type: ExchangeType.Topic,
                durable: true
            ).GetAwaiter().GetResult();
        }

        public void Publish(IntegrationEventBase @event)
        {
            var routingKey = @event.GetType().Name;
            var body = JsonSerializer.SerializeToUtf8Bytes(@event);

            // ✅ Korrekt metodekald i v7: ingen properties-parameter
            _channel.BasicPublishAsync(
                exchange: "eshop_exchange",
                routingKey: routingKey,
                mandatory: false,
                body: body
            ).GetAwaiter().GetResult();

            _logger.LogInformation("Published event: {EventName}", routingKey);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
                await _channel.CloseAsync();

            if (_connection is not null)
                await _connection.CloseAsync();
        }
    }
}
