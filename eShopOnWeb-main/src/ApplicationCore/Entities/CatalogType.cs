using System.Text.Json.Serialization;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Entities;

public class CatalogType : BaseEntity, IAggregateRoot
{
    [JsonPropertyName("typeName")]
    public string Type { get; private set; }
    public int? Id {  get; private set; }
    public CatalogType(string type)
    {
        Type = type;
    }
}
