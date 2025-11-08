using System.Runtime.CompilerServices;
using Identity.API.Data;
using Identity.API.Data.Dto;
using Identity.API.Models;
using Identity.API.Services.JwtTokenGenerator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Identity.API.Services.AuthService
{
    public class AuthService
    {
        private readonly AppIdentityDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(AppIdentityDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<ResponseDto> Register(RegistrationRequestDto dto)
        {
            var response = new ResponseDto();

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                NormalizedUserName = dto.Email.ToUpper(),
                Name = dto.Name
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                response.Success = false;
                response.Message = result.Errors.First().Description;
                return response;
            }

            response.Success = true;
            response.Message = $"User with email '{dto.Email}' registered bro.";
            return response;
        }
        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = await _userManager.FindByNameAsync(loginRequestDto.Username);

            if (user == null)
                return new LoginResponseDto { Token = string.Empty, User = null };

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

            if (!isValid)
                return new LoginResponseDto { Token = string.Empty, User = null };

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            var token = _jwtTokenGenerator.GenerateToken(user);

            return new LoginResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    Email = user.Email,
                    Name = user.Name,
                    Id = user.Id,
                    Role = role
                }
            };
        }


        public async Task<bool> AssignRole(string email, string roleName)
        {
            var user = _db.ApplicationUsers.First(user => user.UserName.ToLower() == email.ToLower());
            if (user != null)
            {
                if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                }
                await _userManager.AddToRoleAsync(user, roleName);
                return true;
            }

            return false;
        }
    }
}
