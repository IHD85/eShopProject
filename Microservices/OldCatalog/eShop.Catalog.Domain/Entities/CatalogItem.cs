namespace eShop.Catalog.Domain.Entities
{
    public class CatalogItem
    {
        public int Id { get; set; }
        public int CatalogBrandId { get; set; }
        public int CatalogTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PictureUri { get; set; } = string.Empty;
        public int AvailableStock { get; set; } = 100;
        public int RestockThreshold { get; set; } = 10;
        public int MaxStockThreshold { get; set; } = 1000;
        public bool OnReorder => AvailableStock <= RestockThreshold;

        public CatalogBrand CatalogBrand { get; set; } = default!;
        public CatalogType CatalogType { get; set; } = default!;
    }
}
