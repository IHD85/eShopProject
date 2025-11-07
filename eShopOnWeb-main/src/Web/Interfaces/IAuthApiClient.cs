using System.Security.Claims;

namespace Microsoft.eShopWeb.Web.Interfaces;

public interface IAuthApiClient
{
    Task<AuthLoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}

public sealed class AuthLoginResult
{
    private AuthLoginResult(bool succeeded, ClaimsPrincipal? principal, string? token, string? errorMessage)
    {
        Succeeded = succeeded;
        Principal = principal;
        Token = token;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public ClaimsPrincipal? Principal { get; }

    public string? Token { get; }

    public string? ErrorMessage { get; }

    public static AuthLoginResult Success(ClaimsPrincipal principal, string token) => new(true, principal, token, null);

    public static AuthLoginResult Fail(string? errorMessage) => new(false, null, null, errorMessage);
}
