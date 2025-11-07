using eShop.Basket.Application.Services;
using eShop.Basket.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Basket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly BasketService _basketService;

        public BasketController(BasketService basketService)
        {
            _basketService = basketService;
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetBasketByCustomerId(string customerId)
        {
            var basket = await _basketService.GetBasketAsync(customerId);
            if (basket == null)
                return NotFound(new { message = $"No basket found for customer: {customerId}" });

            return Ok(new
            {
                message = $"Basket found for customer: {customerId}",
                basket
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBasket([FromBody] ShoppingBasket basket)
        {
            if (basket == null || string.IsNullOrEmpty(basket.CustomerId))
                return BadRequest(new { message = "Basket or CustomerId is missing" });

            var updated = await _basketService.UpdateBasketAsync(basket);
            return Ok(new
            {
                message = $"Basket for customer {basket.CustomerId} saved successfully",
                basket = updated
            });
        }

        [HttpDelete("{customerId}")]
        public async Task<IActionResult> DeleteBasket(string customerId)
        {
            var deleted = await _basketService.DeleteBasketAsync(customerId);

            if (!deleted)
                return NotFound(new { message = $"No basket found for customer: {customerId}" });

            return Ok(new { message = $"Basket for customer {customerId} deleted" });
        }

        [HttpPost("{customerId}/checkout")]
        public async Task<IActionResult> Checkout(string customerId)
        {
            var basket = await _basketService.GetBasketAsync(customerId);
            if (basket == null || basket.Items == null || !basket.Items.Any())
                return BadRequest(new { message = "Cannot checkout an empty basket" });

            _basketService.Checkout(basket);
            return Ok(new
            {
                message = $"Checkout event published for customer {basket.CustomerId}",
                totalItems = basket.Items.Count,
                totalPrice = basket.Items.Sum(i => i.Price * i.Quantity)
            });
        }
    }
}

