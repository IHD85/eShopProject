namespace eShop.Order.Infrastructure.Entities
{
    public class OrderEntity
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<OrderItemEntity> Items { get; set; } = new();
    }

    public class OrderItemEntity
    {
        public int Id { get; set; }
        public int OrderEntityId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
