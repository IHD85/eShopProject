using eShop.Order.API.Dtos;
using RabbitMQEventBus.Events;

namespace eShop.Order.API.IntegrationEvents.Events
{
    public record OrderCreatedIntegrationEvent(string OrderId, List<OrderItemDto> Items) : IntegrationEvent;
}
