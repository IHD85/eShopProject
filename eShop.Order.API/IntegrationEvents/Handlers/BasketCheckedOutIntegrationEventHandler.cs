using eShop.BuildingBlocks.EventBus;
using eShop.Order.API.IntegrationEvents.Events;
using eShop.Order.Domain.Entities;
using eShop.Order.Infrastructure.Data;
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

                if (@event.Items == null || @event.Items.Count == 0)
                {
                    _logger.LogWarning($"⚠️ Received order with no items for customer '{@event.CustomerId}'");
                }

                //  Opret ny order og link items korrekt
                var order = new OrderEntity
                {
                    CustomerId = @event.CustomerId,
                    CreatedAt = DateTime.UtcNow,
                    TotalPrice = @event.TotalPrice,
                    Items = new List<OrderItem>()
                };

                foreach (var item in @event.Items ?? Enumerable.Empty<BasketItemDto>())
                {
                    order.Items.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        UnitPrice = item.Price,
                        Quantity = item.Quantity,
                        Order = order // vigtigt for EF relationen
                    });
                }

                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"🟢 Order saved for customer '{@event.CustomerId}' with {order.Items.Count} items (Total: {@event.TotalPrice})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to create order from BasketCheckedOutIntegrationEvent");
            }
        }
    }
}
