using System.Text;
using BeachBar.Components;
using BeachBar.Infrastructure.Data;
using BeachBar.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

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

// ── Autenticazione ─────────────────────────────────────────────────────────
// Due schemi in parallelo:
//  - Cookie  → UI Blazor (sessione del browser)
//  - JwtBearer → API REST (header Authorization: Bearer <token>)
builder.Services.AddAuthentication(options =>
    {
        // Lo schema di default per le pagine Blazor è il cookie.
        options.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath  = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var key = builder.Configuration["Jwt:Key"]!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization();

// ── REST API ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "BeachBar API",
        Version     = "v1",
        Description = "API per la gestione degli ordini dello stabilimento balneare"
    });

    // Consente di inserire il token JWT direttamente da Swagger UI.
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.ParameterLocation.Header,
        Description  = "Inserisci il JWT ottenuto da POST /api/auth/login"
    });
    c.AddSecurityRequirement(_ => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", null),
            new List<string>()
        }
    });
});

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

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// ── Swagger UI ─────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BeachBar API v1");
    c.RoutePrefix = "swagger";
});

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

app.Run();
