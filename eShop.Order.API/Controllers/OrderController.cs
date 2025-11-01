using eShop.Order.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderEntity = eShop.Order.Infrastructure.Entities.OrderEntity;


namespace eShop.Order.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderDbContext _db;
        private readonly ILogger<OrderController> _logger;

        public OrderController(OrderDbContext db, ILogger<OrderController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ✅ GET: /api/order
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _db.Orders.Include(o => o.Items).ToListAsync();
            return Ok(orders);
        }

        // ✅ GET: /api/order/{customerId}
        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomer(string customerId)
        {
            var orders = await _db.Orders
                .Include(o => o.Items)
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();

            if (!orders.Any())
                return NotFound($"No orders found for customer {customerId}");

            return Ok(orders);
        }

        // ✅ GET: /api/order/details/{orderId}
        [HttpGet("details/{orderId:int}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound($"Order {orderId} not found");

            return Ok(order);
        }

        // ✅ POST: /api/order/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderEntity order)
        {
            if (order == null || string.IsNullOrEmpty(order.CustomerId))
                return BadRequest("Invalid order data");

            order.CreatedAt = DateTime.UtcNow;
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"🟢 Order {order.Id} created for customer {order.CustomerId}");
            return Ok(order);
        }

        // ✅ DELETE: /api/order/{orderId}
        [HttpDelete("{orderId:int}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound($"Order {orderId} not found");

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();

            _logger.LogInformation($"🗑️ Order {orderId} deleted");
            return Ok($"Order {orderId} deleted");
        }
    }
}
