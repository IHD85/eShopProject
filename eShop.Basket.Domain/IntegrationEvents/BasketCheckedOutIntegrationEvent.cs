using eShop.BuildingBlocks.EventBus;

namespace eShop.Basket.Domain.IntegrationEvents
{
    public class BasketCheckedOutIntegrationEvent : IntegrationEventBase
    {
        public string CustomerId { get; init; }
        public decimal TotalPrice { get; init; }
        public List<BasketItemDto> Items { get; init; } = new();

        public BasketCheckedOutIntegrationEvent(string customerId, decimal totalPrice, List<BasketItemDto> items)
        {
            CustomerId = customerId;
            TotalPrice = totalPrice;
            Items = items;
        }
    }

    public class BasketItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
