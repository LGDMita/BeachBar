using System.Text;
using BeachBar.Components;
using BeachBar.Infrastructure.Data;
using BeachBar.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── Frontend Blazor ────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── Razor Pages (usate per login/logout, unico modo per scrivere cookie) ──
builder.Services.AddRazorPages();

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<BeachBarDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Servizi applicativi ────────────────────────────────────────────────────
builder.Services.AddScoped<IProdottiService, ProdottiService>();
builder.Services.AddScoped<ISessioniService, SessioniService>();
builder.Services.AddScoped<IConsumazioniService, ConsumazioniService>();
builder.Services.AddScoped<IImpostazioniService, ImpostazioniService>();
builder.Services.AddScoped<BeachBar.Services.DateContext>();

// ── Autenticazione ─────────────────────────────────────────────────────────
// Due schemi in parallelo:
//  - Cookie  → UI Blazor (sessione del browser)
//  - JwtBearer → API REST (header Authorization: Bearer <token>)
builder.Services.AddAuthentication(options =>
    {
        // Lo schema di default per le pagine Blazor è il cookie.
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var key = builder.Configuration["Jwt:Key"]!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

// ── REST API ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

var app = builder.Build();

// ── Pipeline HTTP ──────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    // In sviluppo il redirect HTTPS viene saltato: ngrok espone HTTP su localhost
    // e non può seguire il redirect su https://localhost con certificato self-signed.
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ── Routing ────────────────────────────────────────────────────────────────
app.MapStaticAssets();
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// MapControllers deve stare dopo MapRazorComponents per evitare conflitti di routing.
app.MapControllers();

app.MapFallback(() => Results.Redirect("/not-found"));

app.Run();
