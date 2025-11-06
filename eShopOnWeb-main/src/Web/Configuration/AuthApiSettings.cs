namespace Microsoft.eShopWeb.Web.Configuration;

public class AuthApiSettings
{
    public const string SectionName = "IdentityApi";

    public string? BaseUrl { get; set; }

    public string LoginPath { get; set; } = "identity/auth/login";

    public JwtSettings JwtOptions { get; set; } = new();

    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
    }
}
