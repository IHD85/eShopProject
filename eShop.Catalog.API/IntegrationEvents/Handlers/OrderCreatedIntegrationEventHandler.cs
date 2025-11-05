using eShop.BuildingBlocks.EventBus;
using eShop.BuildingBlocks.EventBus.Events;
using eShop.Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eShop.Catalog.API.IntegrationEvents.Handlers
{
    public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        private readonly CatalogDbContext _context;

        public OrderCreatedIntegrationEventHandler(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task Handle(OrderCreatedIntegrationEvent @event)
        {
            foreach (var item in @event.Items)
            {
                var product = await _context.CatalogItems.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.AvailableStock = Math.Max(0, product.AvailableStock - item.Quantity);
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ Lager opdateret for ordre {@event.OrderId}");
        }
    }
}
