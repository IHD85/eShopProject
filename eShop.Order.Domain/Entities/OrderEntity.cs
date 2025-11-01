using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eShop.Order.Domain.Entities
{
    public class OrderEntity
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalPrice { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}


