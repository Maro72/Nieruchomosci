using Microsoft.AspNetCore.Components;
using Mieszkaniec.Model.Entities;
using MudBlazor;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.Wynajem
{
    public partial class WynajemList : ComponentBase
    {
        private List<Obiekt> ListaBudynkow = new();
        private List<LokalWynajem> Lokale = new();
        private WynajemStatystyki Statystyki = new();
        private List<Najemca> ListaNajemcow = new();

        private int WybranyBudynekId { get; set; } = 0;
        private string WybranyStatus { get; set; } = "Wszystkie"; // NOWOŚĆ: Stan filtra statusu

        private decimal SumaPowierzchniWolnej { get; set; } = 0;
        private decimal SumaPowierzchniWynajetej { get; set; } = 0;
        private decimal SumaCzynszu { get; set; } = 0;


        protected override async Task OnInitializedAsync()
        {
            ListaBudynkow = await ObiektService.PobierzWszystkieAsync() ?? new List<Obiekt>();
            ListaNajemcow = await NajemcaService.PobierzAktywnychAsync() ?? new List<Najemca>();
            await ZaktualizujWidok();
        }

        private async Task OnBudynekChanged(int obiektId)
        {
            WybranyBudynekId = obiektId;
            await ZaktualizujWidok();
        }

        // NOWOŚĆ: Obsługa zmiany w liście statusów
        private async Task OnStatusChanged(string status)
        {
            WybranyStatus = status;
            await ZaktualizujWidok();
        }

        private async Task WyczyscFiltry()
        {
            // Przycisk czyści teraz oba filtry
            if (WybranyBudynekId != 0 || WybranyStatus != "Wszystkie")
            {
                WybranyBudynekId = 0;
                WybranyStatus = "Wszystkie";
                await ZaktualizujWidok();
            }
        }

        private async Task ZaktualizujWidok()
        {
            List<LokalWynajem> pobraneLokale;

            // 1. Pobieranie danych z bazy (wszystkich lub dla wybranego budynku)
            if (WybranyBudynekId == 0)
            {
                pobraneLokale = await LokalService.PobierzWszystkieAsync() ?? new List<LokalWynajem>();
            }
            else
            {
                pobraneLokale = await LokalService.PobierzDlaObiektuAsync(WybranyBudynekId) ?? new List<LokalWynajem>();
            }

            // 2. NOWOŚĆ: Generowanie statystyk ZAWSZE na podstawie pobranych danych z bazy
            Statystyki = new WynajemStatystyki
            {
                WszystkieLokale = pobraneLokale.Count,
                Wolne = pobraneLokale.Count(x => x.Status == "Wolny"),
                Wynajete = pobraneLokale.Count(x => x.Status == "Wynajęty"),
                Zarezerwowane = pobraneLokale.Count(x => x.Status == "Zarezerwowany")
            };

            // 3. Obliczanie sum podsumowania
            SumaPowierzchniWolnej = pobraneLokale.Where(x => x.Status == "Wolny").Sum(x => x.PowierzchniaM2);
            SumaPowierzchniWynajetej = pobraneLokale.Where(x => x.Status == "Wynajęty").Sum(x => x.PowierzchniaM2);
            SumaCzynszu = pobraneLokale.Where(x => x.Status == "Wynajęty").Sum(x => x.CenaWynajmu);

            // 4. Aplikowanie filtra statusu tylko do widoku tabeli (statystyki zliczają wszystko z punktu 2)
            if (WybranyStatus != "Wszystkie")
            {
                Lokale = pobraneLokale.Where(x => x.Status == WybranyStatus).ToList();
            }
            else
            {
                Lokale = pobraneLokale;
            }

            StateHasChanged();
        }

        private Color WyznaczKolorStatusu(string status)
        {
            return status switch
            {
                "Wolny" => Color.Success,
                "Wynajęty" => Color.Error,
                "Zarezerwowany" => Color.Warning,
                _ => Color.Default
            };
        }

        private async Task OpenDodajDialog()
        {
            var parameters = new DialogParameters
            {
                { "Model", new LokalWynajem { ObiektId = WybranyBudynekId > 0 ? WybranyBudynekId : 0 } },
                { "ListaBudynkow", ListaBudynkow },
                { "ListaNajemcow", ListaNajemcow }
            };

            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<LokalWynajemDialog>("Dodaj nowy lokal", parameters, options);

            var result = await dialog.Result;
            if (!result.Canceled)
            {
                await ZaktualizujWidok();
            }
        }

        private async Task OpenEdytujDialog(LokalWynajem lokal)
        {
            if (lokal == null) return;

            var parameters = new DialogParameters
            {
                { "Model", lokal },
                { "ListaBudynkow", ListaBudynkow },
                { "ListaNajemcow", ListaNajemcow }
            };

            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<LokalWynajemDialog>("Edytuj lokal", parameters, options);

            var result = await dialog.Result;
            if (!result.Canceled)
            {
                await ZaktualizujWidok();
            }
        }
    }
}