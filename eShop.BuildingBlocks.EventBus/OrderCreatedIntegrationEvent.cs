using eShop.BuildingBlocks.EventBus;

namespace eShop.BuildingBlocks.EventBus.Events
{
    public class OrderCreatedIntegrationEvent : IntegrationEventBase
    {
        public string OrderId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();

        public OrderCreatedIntegrationEvent(string orderId, List<OrderItemDto> items)
        {
            OrderId = orderId;
            Items = items;
        }
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
