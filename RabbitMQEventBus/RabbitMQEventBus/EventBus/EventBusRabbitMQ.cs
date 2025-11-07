using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RabbitMQEventBus.EventBus
{
    class EventBusRabbitMQ : IEventBus, IDisposable, IHostedService
    {
        private IConnection _rabbitMQconnection;
        private IConfiguration _configuration;
        private IServiceProvider _serviceProvider;
        private readonly EventBusSubscriptionInfo _subscriptionInfo;
        private readonly string _queueName;

        //private IAsyncBasicConsumer _consumer;
        private IChannel _consumerChannel;

        private const string ExchangeName = "eshop_event_bus";

        public EventBusRabbitMQ(
    IServiceProvider serviceProvider,
    IOptions<EventBusSubscriptionInfo> subscriptionOptions,
    IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _subscriptionInfo = subscriptionOptions.Value;
            _configuration = configuration;

            _queueName = _configuration["RabbitMQ:QueueName"];
        }

        public void Dispose()
        {
            _consumerChannel?.Dispose();
            _rabbitMQconnection?.Dispose();
        }


        public async Task PublishAsync(IntegrationEvent @event)
        {
            var routingKey = @event.GetType().Name;

            using var channel = await _rabbitMQconnection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Direct);

            var body = SerializeMessage(@event);

            var basicProperties = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent,
            };

            try
            {
                await channel.BasicPublishAsync(
                    exchange: ExchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: basicProperties,
                    body: body);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            byte[] bodyCopy = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(bodyCopy);

            await ProcessEvent(eventName, message);

            await _consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);


            //TO-DO: Måske en dead letter exchange ved fejl.?? + consuming.
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            if (!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
            {
                Console.WriteLine("Unable to resolve event type for event name {EventName}", eventName);
                return;
            }

            var integrationEvent = DeserializeMessage(message, eventType);

            foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType))
            {
                await handler.Handle(integrationEvent);
            }


        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await CreateConnectionWithRetryAsync();
            
            
            _consumerChannel = await _rabbitMQconnection.CreateChannelAsync();

            await _consumerChannel.ExchangeDeclareAsync(exchange: ExchangeName,
                                            type: ExchangeType.Direct);

            await _consumerChannel.QueueDeclareAsync(queue: _queueName,
                                      durable: true,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);

            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

            consumer.ReceivedAsync += OnMessageReceived;

            await _consumerChannel.BasicConsumeAsync(
                 queue: _queueName,
                 autoAck: false,
                 consumer: consumer);

            foreach (var (eventName, _) in _subscriptionInfo.EventTypes)
            {
                await _consumerChannel.QueueBindAsync(
                    queue: _queueName,
                    exchange: ExchangeName,
                    routingKey: eventName);
            }

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {

        }

        private byte[] SerializeMessage(IntegrationEvent @event)
        {
            return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);
        }

        private IntegrationEvent DeserializeMessage(string message, Type eventType)
        {
            return JsonSerializer.Deserialize(message, eventType, _subscriptionInfo.JsonSerializerOptions) as IntegrationEvent;
        }
        
        private async Task<IConnection> CreateConnectionWithRetryAsync(
            int maxRetries = 10,
            int delaySeconds = 3)
        {
            var factory = _serviceProvider.GetRequiredService<ConnectionFactory>();
            _rabbitMQconnection = await factory.CreateConnectionAsync();
            int attempts = 0;

            while (true)
            {
                try
                {
                    return await factory.CreateConnectionAsync();

                }
                catch (BrokerUnreachableException) when (attempts < maxRetries)
                {
                    attempts++;
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
                catch (SocketException) when (attempts < maxRetries)
                {
                    attempts++;
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }
        }


    }
}
