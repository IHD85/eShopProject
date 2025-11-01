using eShop.BuildingBlocks.EventBus;

namespace eShop.Order.API.IntegrationEvents.Events
{
    public class BasketCheckedOutIntegrationEvent : IntegrationEventBase
    {
        public string CustomerId { get; init; }
        public decimal TotalPrice { get; init; }

        public BasketCheckedOutIntegrationEvent(string customerId, decimal totalPrice)
        {
            CustomerId = customerId;
            TotalPrice = totalPrice;
        }
    }
}
