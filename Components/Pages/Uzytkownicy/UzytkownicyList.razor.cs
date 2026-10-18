using Microsoft.AspNetCore.Components;
using MudBlazor;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.Uzytkownicy
{
    public partial class UzytkownicyList : ComponentBase
    {
        [Inject] protected IUzytkownikService UzytkownikService { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;

        protected List<Uzytkownik> Uzytkownicy { get; set; } = new();
        protected List<Rola> DostepneRole { get; set; } = new();
        protected List<Uprawnienie> DostepneUprawnienia { get; set; } = new();
        protected bool CzyPotwierdzicUsuniecie { get; set; }
        private int? _uzytkownikDoUsunieciaId;

        protected override async Task OnInitializedAsync()
        {
            await OdswiezListe();
            DostepneRole = await UzytkownikService.PobierzRoleAsync();
            DostepneUprawnienia = await UzytkownikService.PobierzUprawnieniaAsync();
        }

        protected async Task OdswiezListe()
        {
            Uzytkownicy = await UzytkownikService.PobierzUzytkownikowAsync() ?? new List<Uzytkownik>();
            StateHasChanged();
        }

        protected async Task OpenDodajDialog()
        {
            var parameters = new DialogParameters
            {
                { "Model", new Uzytkownik { CzyAktywny = true } },
                { "WszystkieRole", DostepneRole },
                { "WszystkieUprawnienia", DostepneUprawnienia }
            };

            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<UzytkownikDialog>("Dodaj nowego użytkownika", parameters, options);

            var result = await dialog.Result;
            if (!result.Canceled)
            {
                await OdswiezListe();
                Snackbar.Add("Użytkownik został pomyślnie dodany.", Severity.Success);
            }
        }

        protected async Task OpenEdytujDialog(Uzytkownik user)
        {
            var parameters = new DialogParameters
            {
                { "Model", user },
                { "WszystkieRole", DostepneRole },
                { "WszystkieUprawnienia", DostepneUprawnienia }
            };

            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<UzytkownikDialog>("Edycja użytkownika", parameters, options);

            var result = await dialog.Result;
            if (!result.Canceled)
            {
                await OdswiezListe();
                Snackbar.Add("Zmiany zostały zapisane.", Severity.Success);
            }
        }

        protected Task UsunUzytkownika(int id)
        {
            _uzytkownikDoUsunieciaId = id;
            CzyPotwierdzicUsuniecie = true;
            return Task.CompletedTask;
        }

        protected async Task ObsluzPotwierdzenieUsuniecia(bool czyPotwierdzono)
        {
            CzyPotwierdzicUsuniecie = false;
            if (!czyPotwierdzono || !_uzytkownikDoUsunieciaId.HasValue)
            {
                _uzytkownikDoUsunieciaId = null;
                return;
            }

            var id = _uzytkownikDoUsunieciaId.Value;
            _uzytkownikDoUsunieciaId = null;
            await UzytkownikService.UsunUzytkownikaAsync(id);
            Snackbar.Add("Użytkownik został usunięty.", Severity.Info);
            await OdswiezListe();
        }
    }
}