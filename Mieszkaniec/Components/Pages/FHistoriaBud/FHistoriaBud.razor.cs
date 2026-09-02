using Microsoft.AspNetCore.Components;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services;
using Mieszkaniec.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MudBlazor;

namespace Mieszkaniec.Components.Pages.Usterki
{
    public partial class FHistoriaBud : ComponentBase
    {
        [Inject] protected IObiektService ObiektService { get; set; } = default!;
        [Inject] protected IUsterkiBudService UsterkiBudService { get; set; } = default!;

        protected List<Obiekt> WszystkieObiekty { get; set; } = new();
        protected List<Obiekt> FiltrowaneObiekty { get; set; } = new();
        protected List<UsterkiBud> HistoriaUsterek { get; set; } = new();
        protected UsterkiSummary KpiStats { get; set; } = new();

        protected int? WybranyObiektId { get; set; }
        protected string WybranyBudynekNazwa { get; set; } = "";
        protected string TekstFiltruBudynkow { get; set; } = "";
        protected bool PokazujArchiwalne { get; set; } = false;
        protected int gridKey = 0;

        protected override async Task OnInitializedAsync()
        {
            WszystkieObiekty = await ObiektService.GetAllActiveAsync();
            FiltrowaneObiekty = WszystkieObiekty;

            // 2. Ładujemy WSZYSTKIE usterki na start (obiektId = null)
            // Dzięki temu po otwarciu strony grid od razu pokaże wszystkie zgłoszenia z bazy
            HistoriaUsterek = await UsterkiBudService.GetUsterkiAsync(obiektId: null);

            // 3. Pobieramy ogólne statystyki dla wszystkich budynków
            KpiStats = await UsterkiBudService.GetKpiSummaryAsync(null);
            StateHasChanged();

            Console.WriteLine($"[DEBUG] Liczba załadowanych usterek: {HistoriaUsterek.Count}");
        }

        protected void FiltrujBudynki()
        {
            if (string.IsNullOrWhiteSpace(TekstFiltruBudynkow))
            {
                FiltrowaneObiekty = WszystkieObiekty;
            }
            else
            {
                FiltrowaneObiekty = WszystkieObiekty
                    .Where(o => o.Nazwa.Contains(TekstFiltruBudynkow, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        
            private async Task OnBudynekWybrany(Obiekt obiekt)
        {
            WybranyObiektId = obiekt.Id;
            WybranyBudynekNazwa = obiekt.Nazwa;
            await LadujHistorieUsterek();
            StateHasChanged();
        }
        

        protected async Task LadujHistorieUsterek()
        {
            HistoriaUsterek = await UsterkiBudService.GetUsterkiAsync(
         obiektId: WybranyObiektId.HasValue ? WybranyObiektId.Value : null,
         rodzajId: null,
         status: null,
         czyArchiwum: PokazujArchiwalne
     );
            KpiStats = await UsterkiBudService.GetKpiSummaryAsync(WybranyObiektId);
            gridKey++;
            StateHasChanged();
        }
        protected async Task ResetujFiltr()
        {
            WybranyObiektId = null;
            WybranyBudynekNazwa = "Wszystkie nieruchomości";
            HistoriaUsterek = await UsterkiBudService.GetUsterkiAsync(
                obiektId: null,
                rodzajId: null,
                status: null,
                czyArchiwum: PokazujArchiwalne
            );
            KpiStats = await UsterkiBudService.GetKpiSummaryAsync(null);
            gridKey++;
            StateHasChanged();
        }
    }
}