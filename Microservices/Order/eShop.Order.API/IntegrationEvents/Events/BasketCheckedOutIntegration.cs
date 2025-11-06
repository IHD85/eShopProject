using eShop.BuildingBlocks.EventBus;

namespace eShop.Order.API.IntegrationEvents.Events
{
    public class BasketCheckedOutIntegrationEvent : IntegrationEventBase
    {
        public required string CustomerId { get; set; }
        public decimal TotalPrice { get; init; }
        public List<BasketItemDto> Items { get; init; } = new();
    }

    public class BasketItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
