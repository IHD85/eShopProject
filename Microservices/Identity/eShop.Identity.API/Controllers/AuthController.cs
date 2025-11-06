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

        // ✅ Register almindelig bruger
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var token = await _identityService.RegisterAsync(request.UserName, request.Password);
            return Ok(new
            {
                message = "User registered successfully",
                role = "User",
                token
            });
        }

        // ✅ Register admin (kun midlertidigt via Swagger)
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest request)
        {
            var token = await _identityService.RegisterAdminAsync(request.UserName, request.Password);
            return Ok(new
            {
                message = "Admin registered successfully",
                role = "Admin",
                token
            });
        }

        // ✅ Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _identityService.LoginAsync(request.UserName, request.Password);
            return Ok(new
            {
                message = "Login successful",
                token
            });
        }
    }

    // DTOs
    public record RegisterRequest(string UserName, string Password);
    public record LoginRequest(string UserName, string Password);
}
