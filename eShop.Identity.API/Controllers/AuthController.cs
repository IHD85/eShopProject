using eShop.Identity.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Identity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IdentityService _identityService;

        public AuthController(IdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginRequest request)
        {
            var token = await _identityService.RegisterAsync(request.Username, request.Password);
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _identityService.LoginAsync(request.Username, request.Password);
            return Ok(new { token });
        }
    }

    public record LoginRequest(string Username, string Password);
}
