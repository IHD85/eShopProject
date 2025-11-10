using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

namespace Microsoft.eShopWeb.Web.Interfaces;

public interface IBasketClient
{
    public Task<Basket> GetBasketByBuyerId(string buyerId);  
    public Task<Basket> CreateBasketForBuyerAsync(Basket basket);  
}
