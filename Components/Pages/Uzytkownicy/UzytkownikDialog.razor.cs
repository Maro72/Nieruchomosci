using Microsoft.AspNetCore.Components;
using MudBlazor;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.Uzytkownicy
{
    public partial class UzytkownikDialog : ComponentBase
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;

        [Inject] protected IUzytkownikService UzytkownikService { get; set; } = default!;

        [Parameter] public Uzytkownik Model { get; set; } = new();
        [Parameter] public List<Rola> WszystkieRole { get; set; } = new();
        [Parameter] public List<Uprawnienie> WszystkieUprawnienia { get; set; } = new();

        protected string NoweHaslo { get; set; } = string.Empty;
        protected IReadOnlyList<Uprawnienie> UprawnieniaWidoczne => WszystkieUprawnienia
            .Where(uprawnienie => !AliasUprawnien.ContainsKey(uprawnienie.NazwaSystemowa))
            .ToList();

        // Używamy IReadOnlyCollection, co idealnie pasuje do wymogów komponentu MudSelect w widoku
        protected IReadOnlyCollection<Rola> WybraneRole { get; set; } = new HashSet<Rola>();
        protected IReadOnlyCollection<Uprawnienie> WybraneUprawnienia { get; set; } = new HashSet<Uprawnienie>();

        private static readonly IReadOnlyDictionary<string, string[]> AliasUprawnien =
            new Dictionary<string, string[]>(System.StringComparer.OrdinalIgnoreCase)
            {
                ["Lokale.Zarzadzanie"] = ["Lokale.Odczyt", "Lokale.Edycja"],
                ["Przeglady.Zarzadzanie"] = ["Przeglady.Odczyt", "Przeglady.Edycja"],
                ["Remonty.Zarzadzanie"] = ["Remonty.Odczyt", "Remonty.Edycja"],
                ["Najemcy.Zarzadzanie"] = ["Najemcy.Odczyt", "Najemcy.Edycja"],
                ["Uzytkownicy.Zarzadzanie"] = ["Uzytkownicy.Odczyt", "Uzytkownicy.Edycja"],
                ["Uprawnienia.Nadawanie"] = ["Uprawnienia.Odczyt", "Uprawnienia.Edycja"]
            };

        protected override void OnInitialized()
        {
            if (Model.Role != null)
            {
                WybraneRole = WszystkieRole.Where(r => Model.Role.Any(mr => mr.Id == r.Id)).ToHashSet();
            }

            if (CzyJestAdminem())
            {
                WybraneUprawnienia = UprawnieniaWidoczne.ToHashSet();
            }
            else if (Model.Uprawnienia != null)
            {
                var nazwyUprawnien = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                foreach (var uprawnienie in Model.Uprawnienia)
                {
                    if (AliasUprawnien.TryGetValue(uprawnienie.NazwaSystemowa, out var aliasy))
                    {
                        nazwyUprawnien.UnionWith(aliasy);
                    }
                    else
                    {
                        nazwyUprawnien.Add(uprawnienie.NazwaSystemowa);
                    }
                }

                WybraneUprawnienia = UprawnieniaWidoczne
                    .Where(uprawnienie => nazwyUprawnien.Contains(uprawnienie.NazwaSystemowa))
                    .ToHashSet();
            }
        }

        protected bool CzyJestAdminem()
        {
            return WybraneRole.Any(r => r.Nazwa.Equals("Administrator", System.StringComparison.OrdinalIgnoreCase));
        }

        protected void OnRoleCheckboxChanged(Rola rola, bool isChecked)
        {
            var tempRole = WybraneRole.ToList();
            if (isChecked)
            {
                if (!tempRole.Any(r => r.Id == rola.Id))
                {
                    tempRole.Add(rola);
                }
            }
            else
            {
                tempRole.RemoveAll(r => r.Id == rola.Id);
            }
            WybraneRole = tempRole.ToHashSet();

            if (CzyJestAdminem())
            {
                WybraneUprawnienia = UprawnieniaWidoczne.ToHashSet();
            }
        }

        protected void OnUprawnienieCheckboxChanged(Uprawnienie upr, bool isChecked)
        {
            if (CzyJestAdminem()) return;

            var tempUprawnienia = WybraneUprawnienia.ToList();
            if (isChecked)
            {
                if (!tempUprawnienia.Any(u => u.Id == upr.Id))
                {
                    tempUprawnienia.Add(upr);
                }
            }
            else
            {
                tempUprawnienia.RemoveAll(u => u.Id == upr.Id);
            }
            WybraneUprawnienia = tempUprawnienia.ToHashSet();
        }

        protected async Task Zapisz()
        {
            if (Model.Id == 0 && string.IsNullOrWhiteSpace(NoweHaslo))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(NoweHaslo))
            {
                Model.HasloHash = BCrypt.Net.BCrypt.HashPassword(NoweHaslo);
            }

            var uprawnieniaDoZapisu = CzyJestAdminem() ? UprawnieniaWidoczne.ToHashSet() : WybraneUprawnienia;

            await UzytkownikService.ZapiszUzytkownikaAsync(Model, WybraneRole, uprawnieniaDoZapisu);

            MudDialog.Close(DialogResult.Ok(true));
        }

        protected void Anuluj() => MudDialog.Cancel();
    }
}