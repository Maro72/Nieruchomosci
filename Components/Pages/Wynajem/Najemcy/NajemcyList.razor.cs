using Microsoft.AspNetCore.Components;
using Mieszkaniec.Components.Pages.Wynajem.Najemcy;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Implementations;
using MudBlazor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.Wynajem.Najemcy
{
    public partial class NajemcyList : ComponentBase
    {
        private List<Najemca> FiltrowaniNajemcy = new();
        private string WybranyWidok { get; set; } = "Aktywni";

        protected override async Task OnInitializedAsync()
        {
            await ZaladujDane();
        }

        private async Task OnWidokChanged(string nowyWidok)
        {
            WybranyWidok = nowyWidok;
            await ZaladujDane();
        }

        private async Task OdswiezListew()
        {
            await ZaladujDane();
        }

        private async Task ZaladujDane()
        {
            if (WybranyWidok == "Archiwum")
            {
                FiltrowaniNajemcy = await NajemcaService.PobierzArchiwalnychAsync() ?? new List<Najemca>();
            }
            else
            {
                FiltrowaniNajemcy = await NajemcaService.PobierzAktywnychAsync() ?? new List<Najemca>();
            }
            StateHasChanged();
        }

        private async Task OpenDodajDialog()
        {
            var parameters = new DialogParameters { { "Model", new Najemca() } };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };

            // Okno dialogowe stworzymy w kolejnym kroku
            var dialog = await DialogService.ShowAsync<NajemcaDialog>("Dodaj kontrahenta", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await ZaladujDane();
            }
        }

        private async Task OpenEdytujDialog(Najemca najemca)
        {
            if (najemca == null) return;

            var parameters = new DialogParameters { { "Model", najemca } };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };

            var dialog = await DialogService.ShowAsync<NajemcaDialog>("Edytuj dane dzierżawcy", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await ZaladujDane();
            }
        }

        private async Task ZarchiwizujNajemce(Najemca najemca)
        {
            if (najemca == null) return;

            // Wywołanie metody soft-delete z serwisu
            var sukces = await NajemcaService.PrzeniesDoArchiwumAsync(najemca.Id);
            if (sukces)
            {
                await ZaladujDane();
            }
        }

        private async Task PrzywrocNajemce(Najemca najemca)
        {
            if (najemca == null) return;

            var sukces = await NajemcaService.PrzywrocZArchiwumAsync(najemca.Id);
            if (sukces)
            {
                await ZaladujDane();
            }
        }
        private async Task OpenPodgladDialog(Najemca najemca)
    {
        if (najemca == null) return;

        // Przekazujemy dodatkowy parametr: TylkoPodglad = true
        var parameters = new DialogParameters { { "Model", najemca }, { "TylkoPodglad", true } };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
        
        await DialogService.ShowAsync<NajemcaDialog>("Podgląd danych dzierżawcy", parameters, options);
    }
    }
}