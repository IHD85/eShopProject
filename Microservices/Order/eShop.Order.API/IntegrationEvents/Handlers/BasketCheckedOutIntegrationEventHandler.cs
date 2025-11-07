using eShop.Order.API.Dtos;
using eShop.Order.API.IntegrationEvents.Events;
using eShop.Order.Domain.Entities;
using eShop.Order.Infrastructure.Data;
using RabbitMQEventBus.Abstractions;

namespace eShop.Order.API.IntegrationEvents.Handlers
{
    public class BasketCheckedOutIntegrationEventHandler :
        IIntegrationEventHandler<BasketCheckedOutIntegrationEvent>
    {
        private readonly ILogger<BasketCheckedOutIntegrationEventHandler> _logger;
        private readonly OrderDbContext _dbContext;
        private readonly IEventBus _eventBus; 

        public BasketCheckedOutIntegrationEventHandler(
            ILogger<BasketCheckedOutIntegrationEventHandler> logger,
            OrderDbContext dbContext,
            IEventBus eventBus) 
        {
            _logger = logger;
            _dbContext = dbContext;
            _eventBus = eventBus;
        }

        public async Task Handle(BasketCheckedOutIntegrationEvent @event)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(@event.CustomerId))
                {
                    _logger.LogError("Received event with null (empty) CustomerId");
                    return;
                }

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
                        Order = order
                    });
                }

                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Order saved for customer '{@event.CustomerId}' with {order.Items.Count} items (Total: {@event.TotalPrice})");

                // 📤 Efter ordre gemt: Send event til RabbitMQ
                var orderCreatedEvent = new OrderCreatedIntegrationEvent(
                    order.Id.ToString(),
                    order.Items.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                );

                await _eventBus.PublishAsync(orderCreatedEvent);
                _logger.LogInformation($"Published OrderCreatedIntegrationEvent for OrderId={order.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order from BasketCheckedOutIntegrationEvent");
            }
        }
    }
}
