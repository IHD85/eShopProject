using eShop.Basket.Domain.Dtos;
using RabbitMQEventBus.Events;

namespace eShop.Basket.Domain.IntegrationEvents;

public record BasketCheckedOutIntegrationEvent(string CustomerId, decimal TotalPrice, List<BasketItemDto> Items) : IntegrationEvent;




