using RabbitMQEventBus.Events;

namespace Catalog.API.Events;

public record ProductPriceChangedEvent(int ProductId, decimal NewPrice) : IntegrationEvent;