using Identity.API.Models;

namespace Identity.API.Services.JwtTokenGenerator
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser user);
    }
}