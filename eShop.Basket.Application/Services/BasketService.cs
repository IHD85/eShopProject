using eShop.Basket.Domain.Entities;
using eShop.Basket.Domain.IntegrationEvents;
using eShop.BuildingBlocks.EventBus;
using StackExchange.Redis;
using System.Text.Json;

namespace eShop.Basket.Application.Services
{
    public class BasketService
    {
        private readonly IDatabase _redis;
        private readonly IEventBus _eventBus;

        public BasketService(IConnectionMultiplexer redis, IEventBus eventBus)
        {
            _redis = redis.GetDatabase();
            _eventBus = eventBus;
        }

        // ✅ GET basket
        public async Task<ShoppingBasket?> GetBasketAsync(string customerId)
        {
            var key = $"basket:{customerId}";
            var data = await _redis.StringGetAsync(key);

            if (data.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<ShoppingBasket>(data!);
        }

        // ✅ POST / update basket
        public async Task<ShoppingBasket> UpdateBasketAsync(ShoppingBasket basket)
        {
            var key = $"basket:{basket.CustomerId}";
            var json = JsonSerializer.Serialize(basket);

            await _redis.StringSetAsync(key, json);
            return basket;
        }

        // ✅ DELETE basket
        public async Task<bool> DeleteBasketAsync(string customerId)
        {
            var key = $"basket:{customerId}";
            bool exists = await _redis.KeyExistsAsync(key);

            if (!exists)
                return false;

            await _redis.KeyDeleteAsync(key);
            return true;
        }

        // ✅ CHECKOUT: send RabbitMQ event
        public void Checkout(ShoppingBasket basket)
        {
            if (basket == null || string.IsNullOrEmpty(basket.CustomerId) || basket.Items == null || !basket.Items.Any())
                throw new ArgumentException("Invalid basket data");

            var totalPrice = basket.Items.Sum(i => i.Price * i.Quantity);

            // ✅ Sørg for at sende data korrekt
            var eventMessage = new BasketCheckedOutIntegrationEvent(
             basket.CustomerId,
             totalPrice
             );
            Console.WriteLine($"[DEBUG] Sending event: CustomerId={basket.CustomerId}, TotalPrice={totalPrice}");
            _eventBus.Publish(eventMessage);

            Console.WriteLine($"✅ Checkout event sent for customer {basket.CustomerId} with total {totalPrice}");
        }
    }
}
