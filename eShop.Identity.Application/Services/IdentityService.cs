using eShop.Identity.Domain.Entities;
using eShop.Identity.Infrastructure.Data;
using eShop.Identity.Infrastructure.Jwt;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace eShop.Identity.Application.Services
{
    public class IdentityService
    {
        private readonly IdentityDbContext _db;
        private readonly TokenGenerator _tokenGenerator;

        public IdentityService(IdentityDbContext db, TokenGenerator tokenGenerator)
        {
            _db = db;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<string> RegisterAsync(string username, string password)
        {
            if (await _db.Users.AnyAsync(u => u.Username == username))
                throw new Exception("User already exists");

            var user = new ApplicationUser
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return _tokenGenerator.GenerateToken(user);
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            return _tokenGenerator.GenerateToken(user);
        }
    }
}
