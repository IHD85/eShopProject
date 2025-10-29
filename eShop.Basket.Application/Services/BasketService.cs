using eShop.Basket.Domain.Entities;
using eShop.Basket.Domain.IntegrationEvents;
using eShop.BuildingBlocks.EventBus;

namespace eShop.Basket.Application.Services
{
    public class BasketService
    {
        private readonly IEventBus _eventBus;

        public BasketService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Checkout(ShoppingBasket basket)
        {
            var @event = new BasketCheckedOutIntegrationEvent(basket.CustomerId, basket.Total());
            _eventBus.Publish(@event);
        }
    }
}
