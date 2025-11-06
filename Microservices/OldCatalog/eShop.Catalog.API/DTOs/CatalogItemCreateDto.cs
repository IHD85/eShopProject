namespace eShop.Catalog.API.DTOs
{
    public class CatalogItemCreateDto
    {
        public int CatalogBrandId { get; set; }
        public int CatalogTypeId { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public string PictureUri { get; set; } = default!;
        public int AvailableStock { get; set; }
    }
}
