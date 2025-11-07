using Identity.API.Data.Dto;
using Identity.API.Services.AuthService;
using Microsoft.AspNetCore.Mvc;

[Route("api/auth")]
[ApiController]
public class AuthAPIController : Controller
{
    private readonly AuthService _authService;
    protected ResponseDto _response;

    public AuthAPIController(AuthService authService)
    {
        _authService = authService;
        _response = new ResponseDto();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
    {
        var response = await _authService.Register(model);
        if (!response.Success) return BadRequest(response);
        return Ok(response);

    }

    [HttpPost("AssignRole")]
    public async Task<IActionResult> AssignRole(string email, string roleName)
    {
        var assignRole = await _authService.AssignRole(email, roleName);

        if (!assignRole)
        {
            _response.Success = false;
            _response.Message = "Role Assign Failed";
            return BadRequest(_response);
        }
        else
        {
            _response.Success = true;
            _response.Message = "Role Assign Success";
            return Ok(_response);
        }

    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        var userLogin = await _authService.Login(model);

        if (userLogin.User == null || string.IsNullOrEmpty(userLogin.Token))
        {
            _response.Success = false;
            _response.Message = "Invalid email or password";
            return BadRequest(_response);
        }

        _response.Success = true;
        _response.Result = userLogin;
        return Ok(_response);
    }

}