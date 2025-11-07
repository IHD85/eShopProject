
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Catalog.API.Events;
using Catalog.API.Entitites;

namespace Catalog.API.Services.EventBusBackgroundConsumer
{
    public class EventBusBackgroundConsumer : BackgroundService
    {
        private readonly IConfiguration _config;
        private IConnection _connection;
        private IChannel _channel;

        public EventBusBackgroundConsumer(IConfiguration config)
        {
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:Host"],
                UserName = _config["RabbitMQ:User"],
                Password = _config["RabbitMQ:Password"],
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(_config["RabbitMQ:Exchange"], ExchangeType.Fanout, durable:true);

            var queue = (await _channel.QueueDeclareAsync()).QueueName;
            await _channel.QueueBindAsync(queue, _config["RabbitMQ:Exchange"], "routeMe");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) => 
                {
                    string json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var evt = JsonSerializer.Deserialize<Item>(json);
                    Console.WriteLine($"Received {evt.Name}!");


                    await Task.Yield();
                };

            await _channel.BasicConsumeAsync(queue, autoAck: true, consumer);
        }
    }
}
