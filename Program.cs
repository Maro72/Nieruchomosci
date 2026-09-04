using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Mieszkaniec;
using Mieszkaniec.Components; // Wymagane dla MapRazorComponents<App>()
using Mieszkaniec.Model.Context;
using Mieszkaniec.Services;
using Mieszkaniec.Services.Implementations;
using Mieszkaniec.Services.Interfaces;
using MudBlazor.Services;
using System.IO;
var builder = WebApplication.CreateBuilder(args);

// --- 1. Podstawowe us�ugi Blazor ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- 2. Konfiguracja bazy danych MySQL ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<MieszkaniecDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30))));

// --- 3. Rejestracja w�asnych serwis�w (Logika) ---
builder.Services.AddScoped<IObiektService, ObiektService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ITerminDefinicjaService, TerminDefinicjaService>();
builder.Services.AddScoped<IPrzegladService, PrzegladService>();
// Wymagane do globalnego dostarczania stanu autoryzacji (usuwa Tw�j b��d)
builder.Services.AddCascadingAuthenticationState();

// Polityki autoryzacji - NazwaSystemowa z tabeli Uprawnienia
// Rola Administrator automatycznie omija wszystkie polityki
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("ZarzadzanieUzytkownikami", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("Administrator") ||
            ctx.User.HasClaim("Permission", "ZarzadzanieUzytkownikami")));
    options.AddPolicy("ZarzadzanieBudynkami", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("Administrator") ||
            ctx.User.HasClaim("Permission", "ZarzadzanieBudynkami")));
    options.AddPolicy("ZarzadzanieUmowami", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("Administrator") ||
            ctx.User.HasClaim("Permission", "ZarzadzanieUmowami")));
    options.AddPolicy("OdczytBudynkow", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("Administrator") ||
            ctx.User.HasClaim("Permission", "OdczytBudynkow") ||
            ctx.User.HasClaim("Permission", "ZarzadzanieBudynkami")));
});

// Rejestracja z podzia�em na interfejs oraz serwis implementuj�cy
builder.Services.AddScoped<IPraceRemontoweService, PraceRemontoweService>();
builder.Services.AddScoped<IUsterkiBudService, UsterkiBudService>();
builder.Services.AddScoped<IDbConnectionService, DbConnectionService>(); // Rejestracja serwisu do sprawdzania po��czenia z baz� danych

builder.Services.AddScoped<ILokalWynajemService, LokalWynajemService>();
builder.Services.AddScoped<IUmowaService, UmowaService>();
builder.Services.AddScoped<INajemcaService, NajemcaService>();
builder.Services.AddScoped<IUzytkownikService, UzytkownikService>();



// Dodajemy mechanizm logowania oparty na ciasteczkach
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    });

// 2. Nasz w�a�ciwy, natywny mechanizm sesji dla Blazora
builder.Services.AddScoped<AuthenticationStateProvider, Mieszkaniec.Services.MieszkaniecAuthStateProvider>();



builder.Services.AddMudServices();

if (!EF.IsDesignTime)
{
}

var app = builder.Build();

// --- 5. Potok HTTP (Middleware) - SILNIK APLIKACJI ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // Domy�lna warto�� HSTS to 30 dni. 
    app.UseHsts();
}
app.UseAuthentication(); // 1. Najpierw sprawdzamy KIM jest u�ytkownik
app.UseAuthorization();  // 2. Potem sprawdzamy CO mu wolno zrobi�

app.UseHttpsRedirection();

// A. Standardowa obs�uga plik�w statycznych z domy�lnego katalogu wwwroot
app.UseStaticFiles();

// B. Pobranie i weryfikacja fizycznej �cie�ki do katalogu Shared/uploads (Zdefiniowane tylko raz!)
string sharedUploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Shared", "uploads");

// Zabezpieczenie: je�li katalog nie istnieje na dysku, tworzymy go automatycznie
if (!Directory.Exists(sharedUploadsPath))
{
    Directory.CreateDirectory(sharedUploadsPath);
}

// Rejestrujemy dostawc� rozszerze� (gwarantuje, �e serwer nie zablokuje ani nie wymusi pobierania plik�w .pdf)
var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".pdf"] = "application/pdf";

// Rejestracja wirtualnej �cie�ki /uploads kieruj�cej przegl�dark� bezpo�rednio do fizycznego folderu Shared/uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(sharedUploadsPath),
    RequestPath = "/uploads",
    ContentTypeProvider = contentTypeProvider
});
// --- DODAJ TO, ABY ODBLOKOWA� PLIKI UM�W (LIKWIDACJA B��DU 404) ---
string sharedUploadUmowyPath = Path.Combine(builder.Environment.ContentRootPath, "Shared", "upload_umowy");
if (!Directory.Exists(sharedUploadUmowyPath))
{
    Directory.CreateDirectory(sharedUploadUmowyPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(sharedUploadUmowyPath),
    RequestPath = "/Shared/upload_umowy",
    ContentTypeProvider = contentTypeProvider
});
// ------------------------------------------------------------------

app.UseAntiforgery(); // Zabezpieczenie przed atakami CSRF

// Mapowanie g��wnego komponentu aplikacji (App.razor) w trybie interaktywnym serwera
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Uruchomienie aplikacji!
app.Run();
