using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BC = BCrypt.Net.BCrypt;

namespace BeachBar.Pages;

/// <summary>
/// Gestisce il login per la UI Blazor tramite cookie di sessione.
/// Usa Razor Pages (SSR) perché è l'unico modo per scrivere un cookie HTTP
/// dall'interno di una Blazor Web App in modalità Interactive Server.
/// </summary>
public class LoginModel : PageModel
{
    private readonly IConfiguration _config;

    public LoginModel(IConfiguration config)
    {
        _config = config;
    }

    // SupportsGet = true: consente di re-popolare il campo username se il login fallisce
    // (il form fa POST, ma il valore viene riletto dal model binding prima di restituire la pagina).
    [BindProperty(SupportsGet = true)] public string Username { get; set; } = "";
    public string? Errore { get; private set; }

    public IActionResult OnGet()
    {
        // Se già autenticato, vai subito alla dashboard.
        if (User.Identity?.IsAuthenticated == true)
            return LocalRedirect("/");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string username, string password, string? returnUrl)
    {
        Username = username;

        var expectedUsername = _config["Admin:Username"] ?? "";
        var expectedHash     = _config["Admin:PasswordHash"] ?? "";

        var usernameOk = string.Equals(username, expectedUsername, StringComparison.OrdinalIgnoreCase);
        var passwordOk = usernameOk && BC.Verify(password, expectedHash);

        if (!passwordOk)
        {
            Errore = "Username o password non validi.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, expectedUsername),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        return LocalRedirect(returnUrl ?? "/");
    }
}
