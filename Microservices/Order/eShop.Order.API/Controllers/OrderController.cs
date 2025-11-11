using eShop.Order.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using eShop.Order.Domain.Entities;

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

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                _logger.LogInformation($"GetAllOrders called");
                var orders = await _db.Orders.Include(o => o.Items).ToListAsync();
                _logger.LogInformation($"Retrieved {orders.Count} orders");
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in GetAllOrders");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomer(string customerId)
        {
            try
            {
                _logger.LogInformation($"GetOrdersByCustomer called for customerId: {customerId}");
                var orders = await _db.Orders
                    .Include(o => o.Items)
                    .Where(o => o.CustomerId == customerId)
                    .ToListAsync();

                if (!orders.Any())
                {
                    _logger.LogWarning($"No orders found for customer {customerId}");
                    return NotFound($"No orders found for customer {customerId}");
                }

                _logger.LogInformation($"Retrieved {orders.Count} orders for customer {customerId}");
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in GetOrdersByCustomer for customerId: {customerId}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("details/{orderId:int}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                _logger.LogInformation($"GetOrderDetails called for orderId: {orderId}");
                var order = await _db.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    _logger.LogWarning($"Order {orderId} not found");
                    return NotFound($"Order {orderId} not found");
                }

                _logger.LogInformation($"Order {orderId} retrieved");
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in GetOrderDetails for orderId: {orderId}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderEntity order)
        {
            try
            {
                _logger.LogInformation($"CreateOrder called");

                if (order == null || string.IsNullOrEmpty(order.CustomerId))
                {
                    _logger.LogWarning("Invalid order data submitted");
                    return BadRequest("Invalid order data");
                }

                order.CreatedAt = DateTime.UtcNow;
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Order {order.Id} created for customer {order.CustomerId}");
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in CreateOrder");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{orderId:int}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            try
            {
                _logger.LogInformation($"DeleteOrder called for orderId: {orderId}");
                var order = await _db.Orders.FindAsync(orderId);

                if (order == null)
                {
                    _logger.LogWarning($"Order {orderId} not found");
                    return NotFound($"Order {orderId} not found");
                }

                _db.Orders.Remove(order);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Order {orderId} deleted");
                return Ok($"Order {orderId} deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in DeleteOrder for orderId: {orderId}");
                return BadRequest(ex.Message);
            }
        }
    }
}
