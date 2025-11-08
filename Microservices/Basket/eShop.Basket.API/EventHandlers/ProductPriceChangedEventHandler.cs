using System.Text.Json;
using eShop.Basket.Domain.Entities;
using eShop.Basket.Domain.Events;
using RabbitMQEventBus.Abstractions;
using StackExchange.Redis;

namespace eShop.Basket.API.EventHandlers;
public class ProductPriceChangedEventHandler : IIntegrationEventHandler<ProductPriceChangedEvent>
{
    private readonly IDatabase _redis;
    private readonly IConfiguration _configuration;

    public ProductPriceChangedEventHandler(
        IConfiguration configuration,
        IConnectionMultiplexer redis)
    {
        _configuration = configuration;
        _redis = redis.GetDatabase();   
    }

    public async Task Handle(ProductPriceChangedEvent @event)
    {
        var server = _redis.Multiplexer.GetServer(
            $"{_configuration["Redis:Host"]}:{_configuration["Redis:Port"]}");

        var keys = server.Keys(pattern: "basket:*");

        foreach (var key in keys)
        {
            var data = await _redis.StringGetAsync(key);
            if (data.IsNullOrEmpty) continue;

            var basket = JsonSerializer.Deserialize<ShoppingBasket>(data);
            bool modified = false;

            foreach (var item in basket.Items)
            {
                if (item.ProductId == @event.ProductId)
                {
                    item.Price = @event.NewPrice;
                    modified = true;
                }
            }

            if (modified)
            {
                await _redis.StringSetAsync(key, JsonSerializer.Serialize(basket));
            }
        }
    }
}