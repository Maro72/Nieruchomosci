using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using System.Security.Claims;

namespace Mieszkaniec.Services
{
    public class MieszkaniecAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly IDbContextFactory<MieszkaniecDbContext> _dbContextFactory;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public MieszkaniecAuthStateProvider(
            ProtectedSessionStorage sessionStorage,
            IDbContextFactory<MieszkaniecDbContext> dbContextFactory)
        {
            _sessionStorage = sessionStorage;
            _dbContextFactory = dbContextFactory;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Pobieranie informacji o sesji z bezpiecznej pamięci przeglądarki
                var userSessionResult = await _sessionStorage.GetAsync<string>("ZalogowanyUzytkownik");
                var userSession = userSessionResult.Success ? userSessionResult.Value : null;

                if (string.IsNullOrEmpty(userSession))
                    return new AuthenticationState(_anonymous);

                // Budujemy tożsamość z rolami i uprawnieniami z bazy danych
                var claims = await BudujClaimsUzytkownika(userSession);
                var identity = new ClaimsIdentity(claims, "BlazorAuth");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                return new AuthenticationState(_anonymous);
            }
        }

        public async Task ZalogujUzytkownika(string login)
        {
            // Zapis sesji w pamięci i powiadomienie aplikacji (odblokowanie dostępu)
            await _sessionStorage.SetAsync("ZalogowanyUzytkownik", login);

            var claims = await BudujClaimsUzytkownika(login);
            var identity = new ClaimsIdentity(claims, "BlazorAuth");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
        }

        public async Task WylogujUzytkownika()
        {
            await _sessionStorage.DeleteAsync("ZalogowanyUzytkownik");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        // Pobiera z bazy danych role i uprawnienia użytkownika i zamienia je na Claims
        private async Task<List<Claim>> BudujClaimsUzytkownika(string login)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, login) };
            try
            {
                using var db = await _dbContextFactory.CreateDbContextAsync();
                var uzytkownik = await db.Uzytkownicy
                    .Include(u => u.Role)
                    .Include(u => u.Uprawnienia)
                    .FirstOrDefaultAsync(u => u.Login == login);

                if (uzytkownik != null)
                {
                    // Dodajemy role (np. "Administrator") do claims
                    foreach (var rola in uzytkownik.Role)
                        claims.Add(new Claim(ClaimTypes.Role, rola.Nazwa));

                    // Dodajemy konkretne uprawnienia do stron (np. "ZarzadzanieUmowami")
                    foreach (var upr in uzytkownik.Uprawnienia)
                        claims.Add(new Claim("Permission", upr.NazwaSystemowa));
                }
            }
            catch
            {
                // W razie błędu DB zwracamy tylko podstawowy claim z loginem
            }
            return claims;
        }
    }
}