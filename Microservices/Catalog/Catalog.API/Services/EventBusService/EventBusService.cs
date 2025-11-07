//namespace Catalog.API.Services.EventBusService
//{
//    using System.Text;
//    using System.Text.Json;
//    using System.Threading.Channels;
//    using RabbitMQ.Client;

//    public class EventBusService : IEventBusService
//    {
//        private readonly IConfiguration _config;
//        private IConnection _connection;
//        private IChannel _channel;

//        public EventBusService(IConfiguration config)
//        {
//            _config = config;

//        }

//        public async Task InitializeAsync()
//        {
//            var factory = new ConnectionFactory
//            {
//                HostName = _config["RabbitMQ:Host"],
//                UserName = _config["RabbitMQ:User"],
//                Password = _config["RabbitMQ:Password"]
//            };

//            _connection = await factory.CreateConnectionAsync();
//            _channel = await _connection.CreateChannelAsync();

//            await _channel.ExchangeDeclareAsync(
//                exchange: _config["RabbitMQ:Exchange"],
//                type: ExchangeType.Fanout,
//                durable: true
//            );
//        }

//        public async Task PublishAsync<T>(T message, string routingKey)
//        {

//            string json = JsonSerializer.Serialize(message);
//            var bytes = Encoding.UTF8.GetBytes(json);

//            await _channel.BasicPublishAsync(
//                exchange: _config["RabbitMQ:Exchange"],
//                routingKey: routingKey,
//                body: bytes
//            );

//        }

//        public async ValueTask DisposeAsync()
//        {
//            await _channel.CloseAsync();
//            await _connection.CloseAsync();
//        }


//    }
//}
