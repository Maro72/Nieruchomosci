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
builder.Services.AddSingleton<DbConnectionStringProvider>();
builder.Services.AddSingleton<IDbContextFactory<MieszkaniecDbContext>, DynamicDbContextFactory>();
builder.Services.AddScoped(serviceProvider =>
    serviceProvider.GetRequiredService<IDbContextFactory<MieszkaniecDbContext>>().CreateDbContext());

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
    // Funkcja pomocnicza sprawdzająca uprawnienie lub rolę Administratora
    bool HasPerm(System.Security.Claims.ClaimsPrincipal user, params string[] permNames)
    {
        if (user.IsInRole("Administrator")) return true;
        return permNames.Any(p => user.HasClaim("Permission", p));
    }

    // Nowe polityki granularne:
    options.AddPolicy("Budynki.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Budynki.Odczyt", "Budynki.Edycja", "OdczytBudynkow", "ZarzadzanieBudynkami")));
    options.AddPolicy("Budynki.Edycja", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Budynki.Edycja", "ZarzadzanieBudynkami")));
    options.AddPolicy("Dashboard.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Dashboard.Odczyt", "Budynki.Edycja", "ZarzadzanieBudynkami")));
    options.AddPolicy("HistoriaUsterek.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "HistoriaUsterek.Odczyt", "Awarie.Odczyt", "Budynki.Edycja", "ZarzadzanieBudynkami")));
    options.AddPolicy("Techniczny.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Awarie.Odczyt", "Przeglady.Odczyt", "Przeglady.Edycja", "Remonty.Odczyt", "Remonty.Edycja", "HistoriaUsterek.Odczyt", "Awarie.Obsluga", "ZarzadzanieBudynkami")));
    options.AddPolicy("Lokale.Zarzadzanie", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Lokale.Zarzadzanie", "ZarzadzanieUmowami")));
    options.AddPolicy("Lokale.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Lokale.Odczyt", "Lokale.Edycja", "Lokale.Zarzadzanie", "ZarzadzanieUmowami")));
    options.AddPolicy("Lokale.Edycja", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Lokale.Edycja", "Lokale.Zarzadzanie", "ZarzadzanieUmowami")));
    options.AddPolicy("Awarie.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Awarie.Odczyt", "Awarie.Obsluga", "OdczytBudynkow", "ZarzadzanieBudynkami")));
    options.AddPolicy("Awarie.Obsluga", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Awarie.Obsluga", "OdczytBudynkow", "ZarzadzanieBudynkami")));
    options.AddPolicy("Przeglady.Zarzadzanie", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Przeglady.Zarzadzanie", "OdczytBudynkow", "ZarzadzanieBudynkami")));
    options.AddPolicy("Przeglady.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Przeglady.Odczyt", "Przeglady.Edycja", "Przeglady.Zarzadzanie", "OdczytBudynkow", "ZarzadzanieBudynkami")));
    options.AddPolicy("Przeglady.Edycja", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Przeglady.Edycja", "Przeglady.Zarzadzanie", "ZarzadzanieBudynkami")));
    options.AddPolicy("Remonty.Zarzadzanie", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Remonty.Zarzadzanie", "OdczytBudynkow", "ZarzadzanieBudynkami")));
    options.AddPolicy("Remonty.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Remonty.Odczyt", "Remonty.Edycja", "Remonty.Zarzadzanie", "OdczytBudynkow", "ZarzadzanieBudynkami")));
    options.AddPolicy("Remonty.Edycja", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Remonty.Edycja", "Remonty.Zarzadzanie", "ZarzadzanieBudynkami")));
    options.AddPolicy("Najemcy.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Najemcy.Odczyt", "Najemcy.Edycja", "Najemcy.Zarzadzanie", "ZarzadzanieUmowami")));
    options.AddPolicy("Najemcy.Edycja", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Najemcy.Edycja", "Najemcy.Zarzadzanie", "ZarzadzanieUmowami")));
    options.AddPolicy("Najemcy.Zarzadzanie", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Najemcy.Zarzadzanie", "ZarzadzanieUmowami")));
    options.AddPolicy("Umowy.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Umowy.Odczyt", "Umowy.Zarzadzanie", "ZarzadzanieUmowami")));
    options.AddPolicy("Umowy.Zarzadzanie", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Umowy.Zarzadzanie", "ZarzadzanieUmowami")));
    options.AddPolicy("Najem.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Umowy.Odczyt", "Umowy.Zarzadzanie", "Najemcy.Odczyt", "Najemcy.Edycja", "Lokale.Odczyt", "Lokale.Edycja", "ZarzadzanieUmowami")));
    options.AddPolicy("Uzytkownicy.Zarzadzanie", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Uzytkownicy.Zarzadzanie", "ZarzadzanieUzytkownikami")));
    options.AddPolicy("Uzytkownicy.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Uzytkownicy.Odczyt", "Uzytkownicy.Edycja", "Uzytkownicy.Zarzadzanie", "ZarzadzanieUzytkownikami")));
    options.AddPolicy("Uzytkownicy.Edycja", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Uzytkownicy.Edycja", "Uzytkownicy.Zarzadzanie", "ZarzadzanieUzytkownikami")));
    options.AddPolicy("Uprawnienia.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Uprawnienia.Odczyt", "Uprawnienia.Edycja", "Uprawnienia.Nadawanie", "ZarzadzanieUzytkownikami")));
    options.AddPolicy("Uprawnienia.Edycja", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Uprawnienia.Edycja", "Uprawnienia.Nadawanie", "ZarzadzanieUzytkownikami")));
    options.AddPolicy("Administracja.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Uzytkownicy.Odczyt", "Uzytkownicy.Edycja", "Uprawnienia.Odczyt", "Uprawnienia.Edycja", "Uzytkownicy.Zarzadzanie", "Uprawnienia.Nadawanie", "ZarzadzanieUzytkownikami")));
    options.AddPolicy("Konfiguracja.Odczyt", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Konfiguracja.Odczyt", "Konfiguracja.Edycja", "Budynki.Edycja", "ZarzadzanieBudynkami")));
    options.AddPolicy("Konfiguracja.Edycja", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Konfiguracja.Edycja", "Budynki.Edycja", "ZarzadzanieBudynkami")));
    options.AddPolicy("Uprawnienia.Nadawanie", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Uprawnienia.Nadawanie", "ZarzadzanieUzytkownikami")));

    // Kompatybilność ze starszymi nazwami polityk:
    options.AddPolicy("ZarzadzanieUzytkownikami", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Uzytkownicy.Zarzadzanie", "Uprawnienia.Nadawanie", "ZarzadzanieUzytkownikami")));
    options.AddPolicy("ZarzadzanieBudynkami", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Budynki.Edycja", "ZarzadzanieBudynkami")));
    options.AddPolicy("ZarzadzanieUmowami", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Umowy.Zarzadzanie", "ZarzadzanieUmowami")));
    options.AddPolicy("OdczytBudynkow", policy => policy.RequireAssertion(ctx => HasPerm(ctx.User, "Budynki.Odczyt", "Awarie.Odczyt", "OdczytBudynkow", "ZarzadzanieBudynkami")));
});

// Rejestracja z podziaem na interfejs oraz serwis implementujcy
builder.Services.AddScoped<IDbConfigService, DbConfigService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IPraceRemontoweService, PraceRemontoweService>();
builder.Services.AddScoped<IUsterkiBudService, UsterkiBudService>();
builder.Services.AddScoped<IDbConnectionService, DbConnectionService>(); // Rejestracja serwisu do sprawdzania poczenia z baz danych

builder.Services.AddScoped<ILokalWynajemService, LokalWynajemService>();
builder.Services.AddScoped<IUmowaService, UmowaService>();
builder.Services.AddScoped<INajemcaService, NajemcaService>();
builder.Services.AddScoped<IUzytkownikService, UzytkownikService>();
builder.Services.AddScoped<ILicznikService, LicznikService>();



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
