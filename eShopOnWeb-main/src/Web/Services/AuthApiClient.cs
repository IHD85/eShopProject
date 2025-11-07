using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Web.Configuration;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.eShopWeb.Web.Services;

public class AuthApiClient : IAuthApiClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly AuthApiSettings _settings;
    private readonly ILogger<AuthApiClient> _logger;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public AuthApiClient(HttpClient httpClient, IOptions<AuthApiSettings> settings, ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _tokenValidationParameters = CreateTokenValidationParameters(_settings);

        if (!string.IsNullOrWhiteSpace(_settings.BaseUrl) && httpClient.BaseAddress is null)
        {
            httpClient.BaseAddress = new Uri(_settings.BaseUrl, UriKind.Absolute);
        }

        if (_httpClient.BaseAddress is null && !Uri.IsWellFormedUriString(_settings.LoginPath, UriKind.Absolute))
        {
            throw new InvalidOperationException("Identity API base address must be configured with an absolute URI.");
        }
    }

    public async Task<AuthLoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return AuthLoginResult.Fail("Email and password are required.");
        }

        var request = new LoginRequest
        {
            Username = email,
            Password = password
        };

        HttpResponseMessage responseMessage;
        try
        {
            responseMessage = await _httpClient.PostAsJsonAsync(_settings.LoginPath, request, SerializerOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach identity service at {Url}", GetLoginUri());
            return AuthLoginResult.Fail("Unable to reach authentication service.");
        }

        using var response = responseMessage;

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("Authentication service returned {StatusCode}: {Body}", response.StatusCode, body);
            return AuthLoginResult.Fail("Invalid login attempt.");
        }

        ResponseDto<LoginResponse>? payload;
        try
        {
            payload = await response.Content.ReadFromJsonAsync<ResponseDto<LoginResponse>>(SerializerOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse authentication response.");
            return AuthLoginResult.Fail("Authentication response could not be processed.");
        }

        if (payload is null || !payload.Success || payload.Result is null)
        {
            var message = payload?.Message;
            if (!string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("Authentication failed: {Message}", message);
            }
            return AuthLoginResult.Fail(message ?? "Invalid login attempt.");
        }

        ClaimsPrincipal principal;
        try
        {
            principal = BuildPrincipal(payload.Result);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException or InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to validate authentication token.");
            return AuthLoginResult.Fail("Authentication token validation failed.");
        }

        return AuthLoginResult.Success(principal, payload.Result.Token);
    }

    private ClaimsPrincipal BuildPrincipal(LoginResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Token))
        {
            throw new ArgumentException("Token cannot be empty", nameof(response));
        }

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(response.Token, _tokenValidationParameters, out _);
        var identity = principal.Identity as ClaimsIdentity ?? throw new InvalidOperationException("Token did not contain a valid identity");

        var claims = new List<Claim>(identity.Claims);

        if (response.User is not null)
        {
            if (!string.IsNullOrEmpty(response.User.Email) && claims.All(c => c.Type != ClaimTypes.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, response.User.Email));
            }

            if (!string.IsNullOrEmpty(response.User.Id) && claims.All(c => c.Type != ClaimTypes.NameIdentifier))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, response.User.Id));
            }

            if (!string.IsNullOrEmpty(response.User.Name) && claims.All(c => c.Type != ClaimTypes.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, response.User.Name));
            }

            if (!string.IsNullOrEmpty(response.User.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, response.User.Role));
            }
        }

        claims.Add(new Claim("access_token", response.Token));

        var applicationIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme, ClaimTypes.Name, ClaimTypes.Role);

        return new ClaimsPrincipal(applicationIdentity);
    }

    private static TokenValidationParameters CreateTokenValidationParameters(AuthApiSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.JwtOptions.Secret))
        {
            throw new InvalidOperationException("Identity API JWT secret must be configured.");
        }

        var keyBytes = Encoding.ASCII.GetBytes(settings.JwtOptions.Secret);
        var signingKey = new SymmetricSecurityKey(keyBytes);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = !string.IsNullOrWhiteSpace(settings.JwtOptions.Issuer),
            ValidateAudience = !string.IsNullOrWhiteSpace(settings.JwtOptions.Audience),
            ClockSkew = TimeSpan.Zero
        };

        if (parameters.ValidateIssuer)
        {
            parameters.ValidIssuer = settings.JwtOptions.Issuer;
        }

        if (parameters.ValidateAudience)
        {
            parameters.ValidAudience = settings.JwtOptions.Audience;
        }

        return parameters;
    }

    private Uri? GetLoginUri()
    {
        if (Uri.TryCreate(_httpClient.BaseAddress, _settings.LoginPath, out var result))
        {
            return result;
        }

        return null;
    }

    private sealed class LoginRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    private sealed class LoginResponse
    {
        public UserDto? User { get; set; }

        public string Token { get; set; } = string.Empty;
    }

    private sealed class UserDto
    {
        public string? Email { get; set; }

        public string? Name { get; set; }

        public string? Id { get; set; }

        public string? Role { get; set; }
    }

    private sealed class ResponseDto<T>
    {
        public T? Result { get; set; }

        public bool Success { get; set; }

        public string? Message { get; set; }
    }
}
