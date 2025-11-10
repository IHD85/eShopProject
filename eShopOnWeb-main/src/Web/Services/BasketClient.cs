using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.MicroserviceModels;

namespace Microsoft.eShopWeb.Web.Services;

public class BasketClient : IBasketClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BasketClient> _logger;


    public BasketClient(ILogger<BasketClient> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<Basket> CreateBasketForBuyerAsync(Basket basket)
    {
        var basketMs = new ShoppingBasketMC();

        foreach (var item in basket.Items)
        {
            basketMs.Items.Add(new BasketItemMS
            {
                Price = item.UnitPrice,
                ProductId = item.CatalogItemId,
                Quantity = item.Quantity,

            });

            await _httpClient.PostAsJsonAsync<ShoppingBasketMC>("basket/basket", basketMs);


        }

        return basket;
    }

    public async Task<Basket> GetBasketByBuyerId(string buyerId)
    {
        var basket = await _httpClient.GetFromJsonAsync<ShoppingBasketMC>($"basket/Basket/{buyerId}");

        Basket receivedBasket = new Basket(buyerId);
        foreach (var item in basket.Items)
        {
            receivedBasket.AddItem(item.ProductId, item.Price, item.Quantity);
        }

        return receivedBasket;
        
    }
}
