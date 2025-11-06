
namespace Catalog.API.Entitites
{
    public partial class Item
    {
        public int Id { get; set; }

        public string? Description { get; set; }

        public decimal? Price { get; set; }
        public string? Name { get; set; }

        public string? PictureUri { get; set; }

        public int? CatalogTypeId { get; set; }

        public int? CatalogBrandId { get; set; }

        public virtual Brand? CatalogBrand { get; set; }

        public virtual Type? CatalogType { get; set; }
    }
}
