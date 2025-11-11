using Identity.API.Data.Dto;
using Identity.API.Services.AuthService;
using Microsoft.AspNetCore.Mvc;
using Serilog;

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
        try
        {
            Log.Information("Register endpoint hit");
            var response = await _authService.Register(model);
            if (!response.Success)
            {
                Log.Warning("Registration failed");
                return BadRequest(response);
            }
            Log.Information("Registration successful");
            return Ok(response);
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception during registration");
            return BadRequest(e.Message);
        }
    }

    [HttpPost("AssignRole")]
    public async Task<IActionResult> AssignRole(string email, string roleName)
    {
        try
        {
            Log.Information("AssignRole endpoint hit for {Email} with role {Role}", email, roleName);
            var assignRole = await _authService.AssignRole(email, roleName);

            if (!assignRole)
            {
                Log.Warning("Role assignment failed for {Email}", email);
                _response.Success = false;
                _response.Message = "Role Assign Failed";
                return BadRequest(_response);
            }
            else
            {
                Log.Information("Role assignment succeeded for {Email}", email);
                _response.Success = true;
                _response.Message = "Role Assign Success";
                return Ok(_response);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception during role assignment for {Email}", email);
            _response.Success = false;
            _response.Message = e.Message;
            return BadRequest(_response);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        try
        {
            Log.Information("Login endpoint hit");
            var userLogin = await _authService.Login(model);

            if (userLogin.User == null || string.IsNullOrEmpty(userLogin.Token))
            {
                Log.Warning("Invalid login attempt for {Email}", model.Username);
                _response.Success = false;
                _response.Message = "Invalid email or password";
                return BadRequest(_response);
            }

            Log.Information("User login successful for {Email}", model.Username);
            _response.Success = true;
            _response.Result = userLogin;
            return Ok(_response);
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception during login");
            _response.Success = false;
            _response.Message = e.Message;
            return BadRequest(_response);
        }
    }
}
