using Microsoft.AspNetCore.Components;
using Mieszkaniec.Components.Pages.Wynajem.Najemcy;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Implementations;
using Mieszkaniec.Services;
using MudBlazor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.Wynajem.Najemcy
{
    public partial class NajemcyList : ComponentBase
    {
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        private List<Najemca> FiltrowaniNajemcy = new();
        private string WybranyWidok { get; set; } = "Aktywni";

        // --- Panel boczny ---
        private bool CzyOtwartoPanel { get; set; } = false;
        private Najemca? WybranyNajemca { get; set; }
        private List<UmowaNajmu>? UmowyNajemcy { get; set; }
        private bool LadowanieUmow { get; set; } = false;

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
            if (najemca == null)
            {
                return;
            }

            var parametry = new DialogParameters
            {
                [nameof(NajemcaArchiwumDialog.NazwaNajemcy)] = najemca.NazwaFirmyOsoby
            };
            var opcje = new DialogOptions
            {
                CloseOnEscapeKey = true,
                CloseButton = true,
                MaxWidth = MaxWidth.Small,
                FullWidth = true
            };
            var dialog = await DialogService.ShowAsync<NajemcaArchiwumDialog>(
                "Przenieść najemcę do archiwum?",
                parametry,
                opcje);
            var wynik = await dialog.Result;

            if (wynik.Canceled || wynik.Data is not true)
            {
                return;
            }

            var sukces = await NajemcaService.PrzeniesDoArchiwumAsync(najemca.Id);
            Snackbar.Add(
                sukces
                    ? $"Najemca „{najemca.NazwaFirmyOsoby}” został przeniesiony do archiwum."
                    : "Nie udało się przenieść najemcy do archiwum.",
                sukces ? Severity.Success : Severity.Error,
                options =>
                {
                    options.Icon = sukces
                        ? Icons.Material.Filled.Archive
                        : Icons.Material.Filled.ErrorOutline;
                    options.IconColor = sukces ? Color.Success : Color.Error;
                    options.ShowCloseIcon = true;
                    options.VisibleStateDuration = 6000;
                    options.SnackbarVariant = Variant.Outlined;
                });

            if (sukces)
            {
                await ZaladujDane();
            }
        }

        private async Task PrzywrocNajemce(Najemca najemca)
        {
            if (najemca == null)
            {
                return;
            }

            var sukces = await NajemcaService.PrzywrocZArchiwumAsync(najemca.Id);
            Snackbar.Add(
                sukces
                    ? $"Najemca „{najemca.NazwaFirmyOsoby}” został przywrócony z archiwum."
                    : "Nie udało się przywrócić najemcy z archiwum.",
                sukces ? Severity.Success : Severity.Error);

            if (sukces)
            {
                await ZaladujDane();
            }
        }

        private async Task OtworzPanelNajemcy(Najemca najemca)
        {
            if (najemca == null) return;

            WybranyNajemca = najemca;
            UmowyNajemcy = null;
            LadowanieUmow = true;
            CzyOtwartoPanel = true;
            StateHasChanged();

            UmowyNajemcy = await UmowaService.PobierzUmowyNajemcyAsync(najemca.Id);
            LadowanieUmow = false;
            StateHasChanged();
        }
    }
}
