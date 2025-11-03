using eShop.Basket.Domain.Entities;
using eShop.Basket.Domain.IntegrationEvents;
using eShop.BuildingBlocks.EventBus;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace eShop.Basket.Application.Services
{
    public class BasketService
    {
        private readonly IDatabase _redis;
        private readonly IEventBus _eventBus;
        private readonly ILogger<BasketService> _logger;

        public BasketService(IConnectionMultiplexer redis, IEventBus eventBus, ILogger<BasketService> logger)
        {
            _redis = redis.GetDatabase();
            _eventBus = eventBus;
            _logger = logger;
        }

        // ✅ GET basket
        public async Task<ShoppingBasket?> GetBasketAsync(string customerId)
        {
            var key = $"basket:{customerId}";
            var data = await _redis.StringGetAsync(key);

            if (data.IsNullOrEmpty)
            {
                _logger.LogWarning("Basket not found for {CustomerId}", customerId);
                return null;
            }

            var basket = JsonSerializer.Deserialize<ShoppingBasket>(data!);
            _logger.LogInformation("Loaded basket for {CustomerId} with {ItemCount} items",
                customerId, basket?.Items.Count ?? 0);
            return basket;
        }

        // ✅ POST / update basket
        public async Task<ShoppingBasket> UpdateBasketAsync(ShoppingBasket basket)
        {
            var key = $"basket:{basket.CustomerId}";
            var json = JsonSerializer.Serialize(basket);

            await _redis.StringSetAsync(key, json);
            _logger.LogInformation("Updated basket for {CustomerId} with {ItemCount} items",
                basket.CustomerId, basket.Items.Count);
            return basket;
        }

        // ✅ DELETE basket
        public async Task<bool> DeleteBasketAsync(string customerId)
        {
            var key = $"basket:{customerId}";
            bool exists = await _redis.KeyExistsAsync(key);

            if (!exists)
            {
                _logger.LogWarning("Attempt to delete non-existing basket {CustomerId}", customerId);
                return false;
            }

            await _redis.KeyDeleteAsync(key);
            _logger.LogInformation("Deleted basket for {CustomerId}", customerId);
            return true;
        }

        // ✅ CHECKOUT: send RabbitMQ event
        public void Checkout(ShoppingBasket basket)
        {
            if (basket == null || string.IsNullOrEmpty(basket.CustomerId))
            {
                _logger.LogError("Checkout failed: invalid basket");
                throw new ArgumentException("Invalid basket data");
            }

            if (basket.Items == null || !basket.Items.Any())
            {
                _logger.LogWarning("Checkout failed: empty basket for {CustomerId}", basket.CustomerId);
                throw new ArgumentException("Basket is empty");
            }

            var totalPrice = basket.Items.Sum(i => i.Price * i.Quantity);
            _logger.LogInformation("Starting checkout for {CustomerId} with {Count} items (Total {Total})",
                basket.CustomerId, basket.Items.Count, totalPrice);

            var eventMessage = new BasketCheckedOutIntegrationEvent(
                basket.CustomerId,
                totalPrice,
                basket.Items.Select(i => new BasketItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            );

            _eventBus.Publish(eventMessage);
            _logger.LogInformation("Published BasketCheckedOutIntegrationEvent for {CustomerId} (Total {Total})",
                basket.CustomerId, totalPrice);
        }
    }
}
