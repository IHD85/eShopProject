namespace Microsoft.eShopWeb.Web.MicroserviceModels;

public class BasketItemMS
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
