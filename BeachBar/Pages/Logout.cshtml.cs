using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BeachBar.Pages;

/// <summary>
/// Cancella il cookie di sessione e reindirizza al login.
/// Implementata come Razor Page per lo stesso motivo di LoginModel:
/// la risposta HTTP (con Set-Cookie) deve poter essere scritta fuori dal circuit Blazor.
/// </summary>
public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return LocalRedirect("/login");
    }
}
