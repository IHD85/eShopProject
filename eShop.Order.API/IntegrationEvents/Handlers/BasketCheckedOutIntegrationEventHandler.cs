using eShop.BuildingBlocks.EventBus;
using eShop.Order.API.IntegrationEvents.Events;
using eShop.Order.Domain.Entities;
using eShop.Order.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Linq;

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

                    var order = new OrderEntity
                    {
                        CustomerId = @event.CustomerId,
                        TotalPrice = @event.TotalPrice,
                        CreatedAt = DateTime.UtcNow,
                        Items = (@event.Items ?? new List<BasketItemDto>())
                        .Select(i => new OrderItem
                        {
                            ProductName = i.ProductName,
                            Quantity = i.Quantity,
                            UnitPrice = i.Price
                        }).ToList()
                                };
                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"✅ Order created for customer '{@event.CustomerId}' with {order.Items.Count} items, total {@event.TotalPrice}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to create order from BasketCheckedOutIntegrationEvent");
            }
        }
    }
}
