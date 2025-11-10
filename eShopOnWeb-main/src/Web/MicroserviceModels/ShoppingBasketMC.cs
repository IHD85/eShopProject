namespace Microsoft.eShopWeb.Web.MicroserviceModels;

public class ShoppingBasketMC
{
    public string CustomerId { get; set; } = default!;
    public List<BasketItemMS> Items { get; set; } = new();

    public decimal Total() => Items.Sum(i => i.Price * i.Quantity);
}
