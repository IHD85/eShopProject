using eShop.Basket.Application.Services;
using eShop.Basket.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Serilog;

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
            try
            {
                Log.Information($"GetBasketByCustomerId called for customerId: {customerId}");
                var basket = await _basketService.GetBasketAsync(customerId);

                if (basket == null)
                {
                    Log.Warning($"No basket found for customer: {customerId}");
                    return NotFound(new { message = $"No basket found for customer: {customerId}" });
                }

                Log.Information($"Basket found for customer: {customerId}");
                return Ok(new
                {
                    message = $"Basket found for customer: {customerId}",
                    basket
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception in GetBasketByCustomerId for customerId: {customerId}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBasket([FromBody] ShoppingBasket basket)
        {
            try
            {
                Log.Information($"UpdateBasket called for customerId: {basket?.CustomerId}");
                if (basket == null || string.IsNullOrEmpty(basket.CustomerId))
                {
                    Log.Warning("Basket or CustomerId missing");
                    return BadRequest(new { message = "Basket or CustomerId is missing" });
                }

                var updated = await _basketService.UpdateBasketAsync(basket);

                Log.Information($"Basket for customer {basket.CustomerId} updated successfully");
                return Ok(new
                {
                    message = $"Basket for customer {basket.CustomerId} saved successfully",
                    basket = updated
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception in UpdateBasket for customerId: {basket?.CustomerId}");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{customerId}")]
        public async Task<IActionResult> DeleteBasket(string customerId)
        {
            try
            {
                Log.Information($"DeleteBasket called for customerId: {customerId}");
                var deleted = await _basketService.DeleteBasketAsync(customerId);

                if (!deleted)
                {
                    Log.Warning($"No basket found for customer: {customerId}");
                    return NotFound(new { message = $"No basket found for customer: {customerId}" });
                }

                Log.Information($"Basket for customer {customerId} deleted");
                return Ok(new { message = $"Basket for customer {customerId} deleted" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception in DeleteBasket for customerId: {customerId}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{customerId}/checkout")]
        public async Task<IActionResult> Checkout(string customerId)
        {
            try
            {
                Log.Information($"Checkout called for customerId: {customerId}");
                var basket = await _basketService.GetBasketAsync(customerId);

                if (basket == null || basket.Items == null || !basket.Items.Any())
                {
                    Log.Warning($"Checkout failed: Empty or missing basket for customer: {customerId}");
                    return BadRequest(new { message = "Cannot checkout an empty basket" });
                }

                _basketService.Checkout(basket);

                Log.Information($"Checkout event published for customer {basket.CustomerId}");
                return Ok(new
                {
                    message = $"Checkout event published for customer {basket.CustomerId}",
                    totalItems = basket.Items.Count,
                    totalPrice = basket.Items.Sum(i => i.Price * i.Quantity)
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception in Checkout for customerId: {customerId}");
                return BadRequest(ex.Message);
            }
        }
    }
}
