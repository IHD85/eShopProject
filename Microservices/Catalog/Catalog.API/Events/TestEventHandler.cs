using RabbitMQEventBus.Abstractions;

namespace Catalog.API.Events
{
    public class TestEventHandler : IIntegrationEventHandler<TestEvent>
    {
        public Task Handle(TestEvent @event)
        {
            Console.WriteLine($"Received THE TEST! {@event.Message}");
            return Task.CompletedTask;
        }
    }
}
