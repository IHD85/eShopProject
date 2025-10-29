using eShop.BuildingBlocks.EventBus;

namespace eShop.Basket.Domain.IntegrationEvents
{
    public class BasketCheckedOutIntegrationEvent : IntegrationEventBase
    {
        public string CustomerId { get; set; }
        public decimal Total { get; set; }

        public BasketCheckedOutIntegrationEvent(string customerId, decimal total)
        {
            CustomerId = customerId;
            Total = total;
        }
    }
}
