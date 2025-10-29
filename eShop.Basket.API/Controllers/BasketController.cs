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

        [HttpPost("checkout")]
        public IActionResult Checkout([FromBody] ShoppingBasket basket)
        {
            _basketService.Checkout(basket);
            return Ok("Basket checkout event published");
        }
    }
}
