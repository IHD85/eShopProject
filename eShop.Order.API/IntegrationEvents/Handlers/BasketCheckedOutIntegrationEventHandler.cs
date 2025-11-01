using eShop.BuildingBlocks.EventBus;
using eShop.Order.API.IntegrationEvents.Events;
using eShop.Order.Infrastructure.Data;
using eShop.Order.Infrastructure.Entities;
using Microsoft.Extensions.Logging;


namespace eShop.Order.API.IntegrationEvents.Handlers
{
    public class BasketCheckedOutIntegrationEventHandler :
         IIntegrationEventHandler<BasketCheckedOutIntegrationEvent>
    {
        private readonly ILogger<BasketCheckedOutIntegrationEventHandler> _logger;
        private readonly OrderDbContext _dbContext;

        public BasketCheckedOutIntegrationEventHandler(
            ILogger<BasketCheckedOutIntegrationEventHandler> logger,
            OrderDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Handle(BasketCheckedOutIntegrationEvent @event)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(@event.CustomerId))
                {
                    _logger.LogError("❌ Received event with null CustomerId");
                    return;
                }

                _logger.LogInformation(
                    $"📦 Creating order for customer '{@event.CustomerId}' with total {@event.TotalPrice}");

                var order = new OrderEntity
                {
                    CustomerId = @event.CustomerId,
                    TotalPrice = @event.TotalPrice,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"✅ Order created for customer '{@event.CustomerId}' with total {@event.TotalPrice}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to create order from BasketCheckedOutIntegrationEvent");
            }
        }
    }
}
