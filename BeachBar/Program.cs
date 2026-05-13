using BeachBar.Components;
using BeachBar.Infrastructure.Data;
using BeachBar.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ── Frontend Blazor ────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── Database ───────────────────────────────────────────────────────────────
// La stringa di connessione viene letta da appsettings.json ("DefaultConnection").
builder.Services.AddDbContext<BeachBarDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Servizi applicativi ────────────────────────────────────────────────────
// Ogni servizio è registrato tramite la propria interfaccia per consentire
// la sostituzione o il mock nei test senza modificare il codice chiamante.
builder.Services.AddScoped<IProdottiService, ProdottiService>();
builder.Services.AddScoped<ISessioniService, SessioniService>();
builder.Services.AddScoped<IConsumazioniService, ConsumazioniService>();
builder.Services.AddScoped<IImpostazioniService, ImpostazioniService>();

// ── REST API ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BeachBar API",
        Version = "v1",
        Description = "API per la gestione degli ordini dello stabilimento balneare"
    });
});

var app = builder.Build();

// ── Pipeline HTTP ──────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// ── Swagger UI ─────────────────────────────────────────────────────────────
// Disponibile su /swagger — solo per esplorare e testare le API dal browser.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BeachBar API v1");
    c.RoutePrefix = "swagger";
});

app.UseAntiforgery();

// ── Routing ────────────────────────────────────────────────────────────────
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// MapControllers deve stare dopo MapRazorComponents per evitare conflitti di routing.
app.MapControllers();

app.Run();
