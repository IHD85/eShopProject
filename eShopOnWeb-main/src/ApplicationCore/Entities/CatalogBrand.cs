using System.Text.Json.Serialization;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Entities;

public class CatalogBrand : BaseEntity, IAggregateRoot
{
    [JsonPropertyName("brandName")]
    public string Brand { get; private set; }
    public int? Id { get; private set; }

    public CatalogBrand(string brand)
    {
        Brand = brand;
    }
}
