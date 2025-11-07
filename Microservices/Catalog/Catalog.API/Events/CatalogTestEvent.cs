using RabbitMQEventBus.Events;

namespace Catalog.API.Events
{
    public record TestEvent(string Message) : IntegrationEvent;

}
