using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;
using Mieszkaniec.Services;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.FTerminy
{
    // KLUCZOWA POPRAWKA: Nazwa klasy musi być identyczna jak nazwa pliku widoku
    public partial class FTerminyList : ComponentBase
    {
        [Inject] protected ITerminDefinicjaService TerminService { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        protected List<TerminDefinicja> ListaDefinicji { get; set; } = new();
        protected TerminDefinicja? WybranyTermin { get; set; }

        protected bool IsConfirmVisible { get; set; } = false;
        protected string ConfirmTitle { get; set; } = "";
        protected string ConfirmMessage { get; set; } = "";
        protected string ConfirmTheme { get; set; } = "primary";
        protected string ConfirmIcon { get; set; } = "e-warning";

        protected enum TypAkcji { Brak, Usunięcie, Zapis }
        protected TypAkcji OczekujacaAkcja { get; set; } = TypAkcji.Brak;
        protected TerminDefinicja? ObiektDoPrzetworzenia { get; set; }
        protected int gridKey = 0;

        protected override async Task OnInitializedAsync()
        {
            await OdswiezDane();
        }

        protected async Task OdswiezDane()
        {
            try
            {
                ListaDefinicji = await TerminService.GetAllActiveAsync();
                gridKey++;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas ładowania słownika terminów: {ex.Message}");
            }
        }

        protected async Task OpenCreateDialog()
        {
            var parameters = new DialogParameters { { "Model", new TerminDefinicja() } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };

            var dialog = await DialogService.ShowAsync<FTerminDialog>("", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled)
            {
                var powroconyModel = (TerminDefinicja)result.Data!;
                ZazadajPotwierdzeniaZapisu(powroconyModel);
            }
        }

        protected async Task OpenEditDialog(TerminDefinicja? model)
        {
            if (model == null) return;

            var kopiaModelu = new TerminDefinicja
            {
                Id = model.Id,
                NazwaTypu = model.NazwaTypu,
                CzestoscMiesiace = model.CzestoscMiesiace,
                DniPowiadomienia = model.DniPowiadomienia
            };

            var parameters = new DialogParameters { { "Model", kopiaModelu } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };

            var dialog = await DialogService.ShowAsync<FTerminDialog>("", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled)
            {
                var powroconyModel = (TerminDefinicja)result.Data!;
                ZazadajPotwierdzeniaZapisu(powroconyModel);
            }
        }

        protected void ZazadajPotwierdzeniaZapisu(TerminDefinicja model)
        {
            ObiektDoPrzetworzenia = model;
            OczekujacaAkcja = TypAkcji.Zapis;
            ConfirmTitle = "Potwierdzenie zapisu";
            ConfirmMessage = $"Czy na pewno chcesz zapisać zmiany dla definicji '{model.NazwaTypu}'?";
            ConfirmTheme = "success";
            ConfirmIcon = "bi-check-circle-fill";
            IsConfirmVisible = true;
            StateHasChanged();
        }

        protected void RequestDelete(TerminDefinicja? model)
        {
            if (model == null) return;

            ObiektDoPrzetworzenia = model;
            OczekujacaAkcja = TypAkcji.Usunięcie;
            ConfirmTitle = "Usuwanie definicji";
            ConfirmMessage = $"Czy na pewno chcesz usunąć regułę przeglądu '{model.NazwaTypu}'?";
            ConfirmTheme = "danger";
            ConfirmIcon = "bi-trash-fill";
            IsConfirmVisible = true;
            StateHasChanged();
        }

        protected async Task HandleConfirmationAnswer(bool czyZatwierdzono)
        {
            IsConfirmVisible = false;
            if (czyZatwierdzono && ObiektDoPrzetworzenia != null)
            {
                if (OczekujacaAkcja == TypAkcji.Usunięcie)
                {
                    await TerminService.DeleteOrArchiveAsync(ObiektDoPrzetworzenia.Id);
                    WybranyTermin = null;
                    await OdswiezDane();
                }
                else if (OczekujacaAkcja == TypAkcji.Zapis)
                {
                    await TerminService.SaveAsync(ObiektDoPrzetworzenia);
                    WybranyTermin = null;
                    await OdswiezDane();
                }
            }
            ObiektDoPrzetworzenia = null;
            OczekujacaAkcja = TypAkcji.Brak;
            StateHasChanged();
        }

        protected void WybierzWiersz(TerminDefinicja model)
        {
            if (WybranyTermin != null && WybranyTermin.Id == model.Id)
                WybranyTermin = null;
            else
                WybranyTermin = model;

            StateHasChanged();
        }

        protected async Task EksportujDoExcela()
        {
            var kolumny = new Dictionary<string, Func<TerminDefinicja, object>>
            {
                { "Typ Przeglądu", x => x.NazwaTypu ?? "" },
                { "Częstotliwość (miesiące)", x => x.CzestoscMiesiace ?? 0 },
                { "Wyprzedzenie powiadomienia (dni)", x => x.DniPowiadomienia ?? 0 }
            };

            await ExportDoExcela.ExportToCsvAsExcel(JSRuntime, ListaDefinicji, "Slownik_Terminy_Przegladow", kolumny);
        }
    }
}