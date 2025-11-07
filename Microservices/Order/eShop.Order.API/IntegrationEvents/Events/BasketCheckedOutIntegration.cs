using RabbitMQEventBus.Events;

namespace eShop.Order.API.IntegrationEvents.Events
{
    public record BasketCheckedOutIntegrationEvent(string CustomerId, decimal TotalPrice, List<BasketItemDto> Items) : IntegrationEvent;

    public class BasketItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
