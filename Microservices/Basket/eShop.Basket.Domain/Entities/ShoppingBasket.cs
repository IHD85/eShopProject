using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShop.Basket.Domain.Entities
{
    public class ShoppingBasket
    {
        public string CustomerId { get; set; } = default!;
        public List<BasketItem> Items { get; set; } = new();

        public decimal Total() => Items.Sum(i => i.Price * i.Quantity);
    }

    public class BasketItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
