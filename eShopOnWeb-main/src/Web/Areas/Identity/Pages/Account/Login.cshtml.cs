using System.ComponentModel.DataAnnotations;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Interfaces;

namespace Microsoft.eShopWeb.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly IBasketService _basketService;
    private readonly IAuthApiClient _authApiClient;

    public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, IBasketService basketService, IAuthApiClient authApiClient)
    {
        _signInManager = signInManager;
        _logger = logger;
        _basketService = basketService;
        _authApiClient = authApiClient;
    }

    [BindProperty]
    public required InputModel Input { get; set; }

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl = returnUrl ?? Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var loginResult = await _authApiClient.LoginAsync(Input!.Email!, Input!.Password!, HttpContext.RequestAborted);

        if (loginResult.Succeeded && loginResult.Principal is not null)
        {
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = Input.RememberMe,
                AllowRefresh = true
            };

            if (!string.IsNullOrEmpty(loginResult.Token))
            {
                authProperties.StoreTokens(new[]
                {
                    new AuthenticationToken { Name = "access_token", Value = loginResult.Token }
                });
            }

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, loginResult.Principal, authProperties);

            _logger.LogInformation("User logged in via identity microservice.");
            await TransferAnonymousBasketToUserAsync(Input?.Email);
            return LocalRedirect(returnUrl);
        }

        var errorMessage = loginResult.ErrorMessage ?? "Invalid login attempt.";
        _logger.LogWarning("User login failed via identity microservice: {ErrorMessage}", errorMessage);
        ModelState.AddModelError(string.Empty, errorMessage);
        return Page();
    }

    private async Task TransferAnonymousBasketToUserAsync(string? userName)
    {
        if (Request.Cookies.ContainsKey(Constants.BASKET_COOKIENAME))
        {
            var anonymousId = Request.Cookies[Constants.BASKET_COOKIENAME];
            if (Guid.TryParse(anonymousId, out var _))
            {
                Guard.Against.NullOrEmpty(userName, nameof(userName));
                await _basketService.TransferBasketAsync(anonymousId, userName);
            }
            Response.Cookies.Delete(Constants.BASKET_COOKIENAME);
        }
    }
}
