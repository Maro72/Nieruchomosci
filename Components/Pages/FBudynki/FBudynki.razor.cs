using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mieszkaniec.Components.Pages.FBudynki;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services; // Przestrzeń zawierająca ExcelExporter
using Mieszkaniec.Services.Interfaces;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.Obiekty
{
    public partial class FBudynki : ComponentBase
    {
        [Inject] protected IObiektService ObiektService { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!; // Wstrzyknięcie JS do pobierania Excela

        protected List<Obiekt> WszystkieObiekty { get; set; } = new();
        protected List<Obiekt> FiltrowanaListaObiektow { get; set; } = new();

        protected string TekstFiltru { get; set; } = "";
        protected bool PokazujArchiwum { get; set; } = false;

        protected bool IsConfirmVisible { get; set; } = false;
        protected string ConfirmTitle { get; set; } = "";
        protected string ConfirmMessage { get; set; } = "";
        protected string ConfirmTheme { get; set; } = "primary";
        protected string ConfirmIcon { get; set; } = "e-warning";

        protected enum TypAkcji { Brak, Usunięcie, Zapis }
        protected TypAkcji OczekujacaAkcja { get; set; } = TypAkcji.Brak;
        protected Obiekt? ObiektDoPrzetworzenia { get; set; }
        protected int gridKey = 0;

        protected Obiekt? WybranyObiekt { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await OdswiezDane();
        }

        protected async Task OdswiezDane()
        {
            try
            {
                if (PokazujArchiwum)
                    WszystkieObiekty = await ObiektService.GetArchivedAsync();
                else
                    WszystkieObiekty = await ObiektService.GetAllActiveAsync();

                FiltrujListe();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas odświeżania bazy obiektów: {ex.Message}");
            }
        }

        protected void FiltrujListe()
        {
            if (string.IsNullOrWhiteSpace(TekstFiltru))
            {
                FiltrowanaListaObiektow = WszystkieObiekty;
            }
            else
            {
                FiltrowanaListaObiektow = WszystkieObiekty
                    .Where(o => o.Nazwa.Contains(TekstFiltru, StringComparison.OrdinalIgnoreCase) ||
                                 o.Adres.Contains(TekstFiltru, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (WybranyObiekt != null && !FiltrowanaListaObiektow.Any(o => o.Id == WybranyObiekt.Id))
            {
                WybranyObiekt = null;
            }

            gridKey++;
            StateHasChanged();
        }

        protected void OnSearchInput(string val)
        {
            TekstFiltru = val;
            FiltrujListe();
        }

        protected async Task PrzelaczWidokArchiwum(bool stan)
        {
            PokazujArchiwum = stan;
            WybranyObiekt = null;
            await OdswiezDane();
        }

        protected async Task OpenCreateDialog()
        {
            var parameters = new DialogParameters { { "Model", new Obiekt() } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };

            var dialog = await DialogService.ShowAsync<FBudynkiDialog>("", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled)
            {
                var powroconyObiekt = (Obiekt)result.Data!;
                ZazadajPotwierdzeniaZapisu(powroconyObiekt);
            }
        }

        protected async Task OpenEditDialog(Obiekt? obiekt)
        {
            if (obiekt == null) return;

            var kopiaObiektu = new Obiekt
            {
                Id = obiekt.Id,
                Nazwa = obiekt.Nazwa,
                Adres = obiekt.Adres,
                NumerEwidencyjny = obiekt.NumerEwidencyjny,
                RokBudowy = obiekt.RokBudowy,
                LiczbaKondygnacji = obiekt.LiczbaKondygnacji,
                Wysokosc = obiekt.Wysokosc,
                Kubatura = obiekt.Kubatura,
                PowUzytkowa = obiekt.PowUzytkowa,
                Wyposazenie = obiekt.Wyposazenie,
                Opis = obiekt.Opis
            };

            var parameters = new DialogParameters { { "Model", kopiaObiektu } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true };

            var dialog = await DialogService.ShowAsync<FBudynkiDialog>("", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled)
            {
                var powroconyObiekt = (Obiekt)result.Data!;
                ZazadajPotwierdzeniaZapisu(powroconyObiekt);
            }
        }

        protected void ZazadajPotwierdzeniaZapisu(Obiekt obiekt)
        {
            ObiektDoPrzetworzenia = obiekt;
            OczekujacaAkcja = TypAkcji.Zapis;
            ConfirmTitle = "Potwierdzenie zapisu";
            ConfirmMessage = $"Czy na pewno chcesz zapisać zmiany dla obiektu '{obiekt.Nazwa}'?";
            ConfirmTheme = "success";
            ConfirmIcon = "bi-check-circle-fill";
            IsConfirmVisible = true;
            StateHasChanged();
        }

        protected void RequestDelete(Obiekt? obiekt)
        {
            if (obiekt == null) return;

            ObiektDoPrzetworzenia = obiekt;
            OczekujacaAkcja = TypAkcji.Usunięcie;
            ConfirmTitle = "Usuwanie obiektu";
            ConfirmMessage = $"Czy na pewno chcesz usunąć '{obiekt.Nazwa}'?";
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
                    await ObiektService.DeleteOrArchiveAsync(ObiektDoPrzetworzenia.Id);
                    WybranyObiekt = null;
                    await OdswiezDane();
                }
                else if (OczekujacaAkcja == TypAkcji.Zapis)
                {
                    await ObiektService.SaveAsync(ObiektDoPrzetworzenia);
                    WybranyObiekt = null;
                    await OdswiezDane();
                }
            }
            ObiektDoPrzetworzenia = null;
            OczekujacaAkcja = TypAkcji.Brak;
            StateHasChanged();
        }

        protected void WybierzWiersz(Obiekt obiekt)
        {
            if (WybranyObiekt != null && WybranyObiekt.Id == obiekt.Id)
            {
                WybranyObiekt = null;
            }
            else
            {
                WybranyObiekt = obiekt;
            }
            StateHasChanged();
        }

        protected async Task DrukujListe()
        {
            // Metoda drukowania grida wywoływana jest automatycznie przez FGrid
            // przy pomocy okna druku przeglądarki, ale tutaj możesz zostawić logowanie
            await Task.CompletedTask;
            Console.WriteLine("Wywołano uniwersalne drukowanie z poziomu widoku");
        }

        // --- KOMPLETNY I DARMOWY EKSPORT DO EXCELA (EPPLUS) ---
        protected async Task EksportujDoExcela()
        {
            var kolumny = new Dictionary<string, Func<Obiekt, object>>
    {
        { "Nazwa Obiektu", x => x.Nazwa ?? "" },
        { "Numer Ewidencyjny", x => x.NumerEwidencyjny ?? "" },
        { "Adres lokalizacji", x => x.Adres ?? "" },
        { "Rok budowy", x => x.RokBudowy ?? 0 },
        { "Liczba Kondygnacji", x => x.LiczbaKondygnacji ?? 0 },
        { "Powierzchnia Użytkowa (m²)", x => x.PowUzytkowa },
        { "Kubatura (m³)", x => x.Kubatura }
    };

            // WYWOŁANIE NOWEJ NAZWY KLASY:
           await ExportDoExcela.ExportToCsvAsExcel(JSRuntime, FiltrowanaListaObiektow, "Raport_Budynki", kolumny);
}
        }
        }
