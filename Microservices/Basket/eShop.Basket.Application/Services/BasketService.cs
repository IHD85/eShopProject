using eShop.Basket.Domain.Dtos;
using eShop.Basket.Domain.Entities;
using eShop.Basket.Domain.IntegrationEvents;
using Microsoft.Extensions.Logging;
using RabbitMQEventBus.Abstractions;
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

        public async Task<ShoppingBasket?> GetBasketAsync(string customerId)
        {
            var key = $"basket:{customerId}";
            var data = await _redis.StringGetAsync(key);

            if (data.IsNullOrEmpty)
            {
                _logger.LogWarning($"Basket not found for {customerId}");
                return null;
            }

            var basket = JsonSerializer.Deserialize<ShoppingBasket>(data!);
            _logger.LogInformation($"Loaded basket for {customerId} with {basket?.Items.Count ?? 0} items");
            return basket;
        }

        public async Task<ShoppingBasket> UpdateBasketAsync(ShoppingBasket basket)
        {
            var key = $"basket:{basket.CustomerId}";
            var json = JsonSerializer.Serialize(basket);

            await _redis.StringSetAsync(key, json);
            _logger.LogInformation($"Updated basket for {basket.CustomerId} with {basket.Items.Count} items");
            return basket;
        }

        public async Task<bool> DeleteBasketAsync(string customerId)
        {
            var key = $"basket:{customerId}";
            bool exists = await _redis.KeyExistsAsync(key);

            if (!exists)
            {
                _logger.LogWarning($"Attempt to delete non-existing basket {customerId}");
                return false;
            }

            await _redis.KeyDeleteAsync(key);
            _logger.LogInformation($"Deleted basket for {customerId}");
            return true;
        }

        public async Task Checkout(ShoppingBasket basket)
        {
            if (basket == null || string.IsNullOrEmpty(basket.CustomerId))
            {
                _logger.LogError("Checkout failed: invalid basket");
                throw new ArgumentException("Invalid basket data");
            }

            if (basket.Items == null || !basket.Items.Any())
            {
                _logger.LogWarning($"Checkout failed: empty basket for {basket.CustomerId}");
                throw new ArgumentException("Basket is empty");
            }

            var totalPrice = basket.Items.Sum(i => i.Price * i.Quantity);
            _logger.LogInformation($"Starting checkout for {basket.CustomerId} with {basket.Items.Count} items (Total {totalPrice})");
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

            await _eventBus.PublishAsync(eventMessage);
            _logger.LogInformation($"Published {eventMessage.GetType().Name} for {basket.CustomerId} (Total {totalPrice})");
            await DeleteBasketAsync(basket.CustomerId);
        }
    }
}
