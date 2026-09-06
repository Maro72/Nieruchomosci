using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;

namespace Mieszkaniec.Services.Implementations;

public class UzytkownikService : IUzytkownikService
{
	private readonly IDbContextFactory<MieszkaniecDbContext> _contextFactory;

	public UzytkownikService(IDbContextFactory<MieszkaniecDbContext> contextFactory)
	{
		_contextFactory = contextFactory;
	}

	public async Task<List<Uzytkownik>> PobierzUzytkownikowAsync()
	{
		await using var context = await _contextFactory.CreateDbContextAsync();
		return await context.Uzytkownicy
			.Include(uzytkownik => uzytkownik.Role)
			.Include(uzytkownik => uzytkownik.Uprawnienia)
			.OrderBy(uzytkownik => uzytkownik.Login)
			.ToListAsync();
	}

	public async Task<List<Rola>> PobierzRoleAsync()
	{
		await using var context = await _contextFactory.CreateDbContextAsync();
		return await context.Role
			.OrderBy(rola => rola.Nazwa)
			.ToListAsync();
	}

	public async Task<List<Uprawnienie>> PobierzUprawnieniaAsync()
	{
		await using var context = await _contextFactory.CreateDbContextAsync();
		return await context.Uprawnienia
			.OrderBy(uprawnienie => uprawnienie.NazwaSystemowa)
			.ToListAsync();
	}

	public async Task ZapiszUzytkownikaAsync(
		Uzytkownik uzytkownik,
		IEnumerable<Rola> wybraneRole,
		IEnumerable<Uprawnienie> wybraneUprawnienia)
	{
		await using var context = await _contextFactory.CreateDbContextAsync();

		var roleIds = wybraneRole.Select(rola => rola.Id).ToHashSet();
		var uprawnienieIds = wybraneUprawnienia.Select(uprawnienie => uprawnienie.Id).ToHashSet();
		var role = await context.Role.Where(rola => roleIds.Contains(rola.Id)).ToListAsync();
		var uprawnienia = await context.Uprawnienia
			.Where(uprawnienie => uprawnienieIds.Contains(uprawnienie.Id))
			.ToListAsync();

		if (uzytkownik.Id == 0)
		{
			uzytkownik.Role = role;
			uzytkownik.Uprawnienia = uprawnienia;
			await context.Uzytkownicy.AddAsync(uzytkownik);
		}
		else
		{
			var istniejacyUzytkownik = await context.Uzytkownicy
				.Include(uzytkownik => uzytkownik.Role)
				.Include(uzytkownik => uzytkownik.Uprawnienia)
				.SingleOrDefaultAsync(bazowyUzytkownik => bazowyUzytkownik.Id == uzytkownik.Id)
				?? throw new InvalidOperationException("Nie znaleziono użytkownika do aktualizacji.");

			istniejacyUzytkownik.Login = uzytkownik.Login;
			istniejacyUzytkownik.HasloHash = uzytkownik.HasloHash;
			istniejacyUzytkownik.Imie = uzytkownik.Imie;
			istniejacyUzytkownik.Nazwisko = uzytkownik.Nazwisko;
			istniejacyUzytkownik.CzyAktywny = uzytkownik.CzyAktywny;
			istniejacyUzytkownik.Role = role;
			istniejacyUzytkownik.Uprawnienia = uprawnienia;
		}

		await context.SaveChangesAsync();
	}

	public async Task UsunUzytkownikaAsync(int id)
	{
		await using var context = await _contextFactory.CreateDbContextAsync();
		var uzytkownik = await context.Uzytkownicy.FindAsync(id);

		if (uzytkownik is null)
		{
			return;
		}

		context.Uzytkownicy.Remove(uzytkownik);
		await context.SaveChangesAsync();
	}
}
 