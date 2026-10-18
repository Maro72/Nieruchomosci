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
        private bool CzyPotwierdzicUsuniecie { get; set; }
        private string TrescPotwierdzeniaUsuniecia { get; set; } = string.Empty;
        private LokalWynajem? LokalDoUsuniecia { get; set; }


        protected override async Task OnInitializedAsync()
        {
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

        protected async Task PokazNajemcowDialog(LokalWynajem lokal)
        {
            var parameters = new DialogParameters
            {
                ["Lokal"] = lokal
            };
            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true,
                CloseButton = true,
                CloseOnEscapeKey = true
            };

            await DialogService.ShowAsync<NajemcyLokaluDialog>(
                $"Najemcy lokalu {lokal.NumerLokalu}",
                parameters,
                options);
        }

        private async Task ZaktualizujWidok()
        {
            ListaBudynkow = await ObiektService.GetAllActiveAsync();
            if (WybranyBudynekId != 0 && !ListaBudynkow.Any(budynek => budynek.Id == WybranyBudynekId))
            {
                WybranyBudynekId = 0;
            }

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
            var lokaleDoWynajmu = pobraneLokale.Where(x => !x.CzyCzescWspolna).ToList();
            var lokaleDoStatystyk = lokaleDoWynajmu.Select(x => x.StatusAktualny).ToList();
            Statystyki = new WynajemStatystyki
            {
                WszystkieLokale = lokaleDoWynajmu.Count,
                Wolne = lokaleDoStatystyk.Count(status => status == "Wolny"),
                Wynajete = lokaleDoStatystyk.Count(status => status is "Wynajęty" or "Częściowo wynajęty"),
                Zarezerwowane = lokaleDoStatystyk.Count(status => status == "Zarezerwowany")
            };

            // 3. Obliczanie sum podsumowania
            SumaPowierzchniWolnej = lokaleDoWynajmu
                .Where(x => x.StatusAktualny is "Wolny" or "Częściowo wynajęty")
                .Sum(x => x.PowierzchniaDostepnaM2);
            SumaPowierzchniWynajetej = lokaleDoWynajmu.Sum(x => x.PowierzchniaWynajetaM2);
            SumaCzynszu = lokaleDoWynajmu.Sum(x => x.CzynszWynajetyMiesiecznie);

            // 4. Aplikowanie filtra statusu tylko do widoku tabeli (statystyki zliczają wszystko z punktu 2)
            if (WybranyStatus != "Wszystkie")
            {
                Lokale = pobraneLokale.Where(x => x.StatusAktualny == WybranyStatus).ToList();
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
                "Częściowo wynajęty" => Color.Warning,
                "Wynajęty" => Color.Error,
                "Zarezerwowany" => Color.Warning,
                _ => Color.Default
            };
        }

        private static bool CzyLokalMoznaUsunac(LokalWynajem lokal) =>
            lokal.PowierzchniaWynajetaM2 <= 0 &&
            lokal.StatusAktualny == "Wolny" &&
            lokal.Status != "Zarezerwowany";

        private Task PotwierdzUsuniecieLokalu(LokalWynajem lokal)
        {
            if (!CzyLokalMoznaUsunac(lokal))
            {
                Snackbar.Add("Nie można usunąć lokalu wynajętego, częściowo wynajętego ani zarezerwowanego.", Severity.Warning);
                return Task.CompletedTask;
            }

            LokalDoUsuniecia = lokal;
            TrescPotwierdzeniaUsuniecia =
                $"Czy na pewno chcesz usunąć wolny lokal „{lokal.NumerLokalu}” z budynku „{lokal.Obiekt?.Nazwa ?? "nieznany"}”?";
            CzyPotwierdzicUsuniecie = true;
            return Task.CompletedTask;
        }

        private async Task ObsluzPotwierdzenieUsuniecia(bool czyPotwierdzono)
        {
            CzyPotwierdzicUsuniecie = false;
            var lokal = LokalDoUsuniecia;
            LokalDoUsuniecia = null;

            if (!czyPotwierdzono || lokal is null)
            {
                return;
            }

            try
            {
                var usunieto = await LokalService.UsunAsync(lokal.Id);
                if (!usunieto)
                {
                    Snackbar.Add("Nie można usunąć lokalu, ponieważ jest wynajęty, zarezerwowany lub już nie istnieje.", Severity.Warning);
                    await ZaktualizujWidok();
                    return;
                }

                Snackbar.Add($"Lokal „{lokal.NumerLokalu}” został usunięty.", Severity.Success);
                await ZaktualizujWidok();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Nie udało się usunąć lokalu: {ex.Message}", Severity.Error);
            }
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