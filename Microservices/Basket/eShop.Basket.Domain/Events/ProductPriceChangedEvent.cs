using RabbitMQEventBus.Events;

namespace eShop.Basket.Domain.Events;

public record ProductPriceChangedEvent(int ProductId, decimal NewPrice) : IntegrationEvent;