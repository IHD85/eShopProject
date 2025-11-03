namespace eShop.Order.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        // Relation til Order
        public int OrderId { get; set; }
        public OrderEntity? Order { get; set; } 
    }
}
