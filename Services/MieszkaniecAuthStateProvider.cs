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
        private readonly ILogger<MieszkaniecAuthStateProvider> _logger;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public MieszkaniecAuthStateProvider(
            ProtectedSessionStorage sessionStorage,
            IDbContextFactory<MieszkaniecDbContext> dbContextFactory,
            ILogger<MieszkaniecAuthStateProvider> logger)
        {
            _sessionStorage = sessionStorage;
            _dbContextFactory = dbContextFactory;
            _logger = logger;
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
            catch (Exception exception)
            {
                _logger.LogError(exception, "Nie udało się odczytać sesji użytkownika.");
                return new AuthenticationState(_anonymous);
            }
        }

        public async Task ZalogujUzytkownika(string login)
        {
            var claims = await BudujClaimsUzytkownika(login);
            if (claims.Count == 0)
            {
                await WylogujUzytkownika();
                return;
            }

            await _sessionStorage.SetAsync("ZalogowanyUzytkownik", login);
            var identity = new ClaimsIdentity(claims, "BlazorAuth");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
        }

        public async Task OdswiezStanAutoryzacjiAsync()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(await GetAuthenticationStateAsync()));
        }

        public async Task WylogujUzytkownika()
        {
            await _sessionStorage.DeleteAsync("ZalogowanyUzytkownik");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        // Pobiera z bazy danych role i uprawnienia użytkownika i zamienia je na Claims
        private async Task<List<Claim>> BudujClaimsUzytkownika(string login)
        {
            try
            {
                await using var db = await _dbContextFactory.CreateDbContextAsync();
                var uzytkownik = await db.Uzytkownicy
                    .Include(u => u.Role)
                    .Include(u => u.Uprawnienia)
                    .FirstOrDefaultAsync(u => u.Login == login);

                if (uzytkownik is null || !uzytkownik.CzyAktywny)
                {
                    return new List<Claim>();
                }

                var claims = new List<Claim> { new(ClaimTypes.Name, login) };
                foreach (var rola in uzytkownik.Role)
                    claims.Add(new Claim(ClaimTypes.Role, rola.Nazwa));

                foreach (var upr in uzytkownik.Uprawnienia)
                    claims.Add(new Claim("Permission", upr.NazwaSystemowa));

                return claims;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Nie udało się pobrać uprawnień użytkownika {Login}.", login);
                return new List<Claim>();
            }
        }
    }
}