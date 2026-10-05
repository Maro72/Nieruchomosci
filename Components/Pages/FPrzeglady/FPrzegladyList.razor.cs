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

namespace Mieszkaniec.Components.Pages.FPrzeglady
{
    public partial class FPrzegladyList : ComponentBase
    {
        [Inject] protected IPrzegladService PrzegladService { get; set; } = default!;
        [Inject] protected IObiektService ObiektService { get; set; } = default!;
        [Inject] protected ITerminDefinicjaService DefiniceService { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        protected List<Przeglad> ListaPrzegladow { get; set; } = new();
        private List<Przeglad> _pelnaListaPrzegladow { get; set; } = new();

        protected List<Obiekt> ListaObiektow { get; set; } = new();
        protected List<TerminDefinicja> ListaDefinicji { get; set; } = new();
        protected List<string> OpcjeStatusow { get; set; } = new() { "Aktualny", "Zbliża się", "Przeterminowany" };

        protected int LiczbaPrzeterminowanych => _pelnaListaPrzegladow.Count(p => CzyAktywny(p) && WyznaczStatusTekst(p) == "Przeterminowany");
        protected int LiczbaZblizajacychSie => _pelnaListaPrzegladow.Count(p => CzyAktywny(p) && WyznaczStatusTekst(p) == "Zbliża się");
        protected int LiczbaAktualnych => _pelnaListaPrzegladow.Count(p => CzyAktywny(p) && WyznaczStatusTekst(p) == "Aktualny");

        protected int? FiltreObiektId { get; set; }
        protected int? FiltreDefinicjaId { get; set; }
        protected string? FiltreStatus { get; set; }

        protected bool PokazArchiwum { get; set; } = false;
        protected bool IsConfirmVisible { get; set; } = false;
        protected string ConfirmTitle { get; set; } = "";
        protected string ConfirmMessage { get; set; } = "";
        protected string ConfirmTheme { get; set; } = "primary";
        protected string ConfirmIcon { get; set; } = "e-warning";

        protected enum TypAkcji { Brak, Usunięcie, Zapis, Realizacja }
        protected TypAkcji OczekujacaAkcja { get; set; } = TypAkcji.Brak;
        protected Przeglad? ObiektDoPrzetworzenia { get; set; }
        protected Przeglad? WybranyPrzeglad { get; set; }
        private MudExpansionPanels? PaneleBudynkow { get; set; }

        protected override async Task OnInitializedAsync()
        {
            ListaObiektow = await ObiektService.GetAllActiveAsync();
            ListaDefinicji = await DefiniceService.GetAllActiveAsync();
            await OdswiezDane();
        }

        protected async Task OdswiezDane()
        {
            try
            {
                _pelnaListaPrzegladow = await PrzegladService.GetAllWithDetailsAsync();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd pobierania danych: {ex.Message}");
            }
        }

        protected void PrzelaczWidokArchiwum(bool stan)
        {
            PokazArchiwum = stan;
            FiltreStatus = null;
            WybranyPrzeglad = null;
            ApplyFilters();
        }

        protected void OnObiektFilterChanged(int? val) { FiltreObiektId = val; ApplyFilters(); }
        protected void OnDefinicjaFilterChanged(int? val) { FiltreDefinicjaId = val; ApplyFilters(); }
        protected void OnStatusFilterChanged(string? val) { FiltreStatus = val; ApplyFilters(); }

        protected void PokazPrzeterminowane()
        {
            UstawFiltrStatusu("Przeterminowany");
        }

        protected void PokazZblizajaceSie()
        {
            UstawFiltrStatusu("Zbliża się");
        }

        protected void PokazAktualne()
        {
            UstawFiltrStatusu("Aktualny");
        }

        private void UstawFiltrStatusu(string status)
        {
            PokazArchiwum = false;
            FiltreObiektId = null;
            FiltreDefinicjaId = null;
            FiltreStatus = status;
            WybranyPrzeglad = null;
            ApplyFilters();
        }

        protected void ApplyFilters()
        {
            var query = _pelnaListaPrzegladow.AsQueryable();

            query = PokazArchiwum ? query.Where(p => p.Status == "Wykonany") : query.Where(p => p.Status != "Wykonany");

            if (FiltreObiektId.HasValue) query = query.Where(p => p.ObiektId == FiltreObiektId.Value);
            if (FiltreDefinicjaId.HasValue) query = query.Where(p => p.TerminDefinicjaId == FiltreDefinicjaId.Value);

            var skompilowanaLista = query.ToList();

            if (!PokazArchiwum && !string.IsNullOrEmpty(FiltreStatus))
            {
                skompilowanaLista = skompilowanaLista.Where(p => WyznaczStatusTekst(p) == FiltreStatus).ToList();
            }

            ListaPrzegladow = skompilowanaLista;
            StateHasChanged();
        }

        protected async Task ResetujFiltry()
        {
            FiltreObiektId = null;
            FiltreDefinicjaId = null;
            FiltreStatus = null;
            ApplyFilters();

            if (PaneleBudynkow != null)
            {
                await PaneleBudynkow.CollapseAllAsync();
            }
        }

        protected async Task OpenCreateDialog()
        {
            var model = new Przeglad { DataWykonania = DateTime.Today, DataNastepnego = DateTime.Today, Status = "Planowany" };
            await ShowDialog(model, false);
        }

        protected async Task OpenEditDialog(Przeglad? model)
        {
            if (model == null) return;
            await ShowDialog(model, false);
        }

        protected async Task OpenRealizeDialog(Przeglad model)
        {
            if (model == null) return;
            model.DataWykonania = DateTime.Today;
            await ShowDialog(model, true);
        }

        private async Task ShowDialog(Przeglad model, bool isRealizeMode)
        {
            var parameters = new DialogParameters { { "Model", model }, { "IsRealizeMode", isRealizeMode } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };

            var dialog = await DialogService.ShowAsync<FPrzegladDialog>("", parameters, options);
            var result = await dialog.Result;

            if (result != null && !result.Canceled)
            {
                ZazadajPotwierdzeniaZapisu((Przeglad)result.Data!, isRealizeMode);
            }
        }

        protected void ZazadajPotwierdzeniaZapisu(Przeglad model, bool trybRealizacji)
        {
            ObiektDoPrzetworzenia = model;
            if (trybRealizacji)
            {
                OczekujacaAkcja = TypAkcji.Realizacja;
                ConfirmTitle = "Zamykanie i realizacja przeglądu";
                ConfirmMessage = "Czy na pewno chcesz oznaczyć ten przegląd jako WYKONANY? Protokół trafi do archiwum.";
                ConfirmTheme = "success";
                ConfirmIcon = "bi-check-circle-fill";
            }
            else
            {
                OczekujacaAkcja = TypAkcji.Zapis;
                ConfirmTitle = "Potwierdzenie zapisu";
                ConfirmMessage = "Czy chcesz zapisać zmiany w tym przeglądzie?";
                ConfirmTheme = "primary";
                ConfirmIcon = "bi-info-circle-fill";
            }
            IsConfirmVisible = true;
            StateHasChanged();
        }

        protected void RequestDelete(Przeglad? model)
        {
            if (model == null) return;
            ObiektDoPrzetworzenia = model;
            OczekujacaAkcja = TypAkcji.Usunięcie;
            ConfirmTitle = "Usuwanie przeglądu";
            ConfirmMessage = "Czy na pewno chcesz nieodwracalnie usunąć ten rekord przeglądu?";
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
                    await PrzegladService.DeleteAsync(ObiektDoPrzetworzenia.Id);
                }
                else if (OczekujacaAkcja == TypAkcji.Zapis)
                {
                    await PrzegladService.SaveAsync(ObiektDoPrzetworzenia);
                }
                else if (OczekujacaAkcja == TypAkcji.Realizacja)
                {
                    await PrzegladService.ZrealizujIArchiwizujAsync(ObiektDoPrzetworzenia);
                }
                WybranyPrzeglad = null;
                await OdswiezDane();
            }
            ObiektDoPrzetworzenia = null;
            OczekujacaAkcja = TypAkcji.Brak;
            StateHasChanged();
        }

        protected void WybierzWiersz(Przeglad model)
        {
            WybranyPrzeglad = (WybranyPrzeglad != null && WybranyPrzeglad.Id == model.Id) ? null : model;
            StateHasChanged();
        }

        protected string WyznaczStatusTekst(Przeglad p)
        {
            if (p.Status == "Wykonany") return "Wykonany";
            if (p.DataNastepnego < DateTime.Today) return "Przeterminowany";
            int dniWyprzedzenia = p.TerminDefinicja?.DniPowiadomienia ?? 0;
            if (p.DataNastepnego <= DateTime.Today.AddDays(dniWyprzedzenia)) return "Zbliża się";
            return "Aktualny";
        }

        private static bool CzyAktywny(Przeglad p) => p.Status != "Wykonany";

        protected string WyznaczStatusKlasa(Przeglad p)
        {
            if (p.Status == "Wykonany") return "badge-sukces";
            if (p.DataNastepnego < DateTime.Today) return "badge-alarm";
            int dniWyprzedzenia = p.TerminDefinicja?.DniPowiadomienia ?? 0;
            if (p.DataNastepnego <= DateTime.Today.AddDays(dniWyprzedzenia)) return "badge-ostrzezenie";
            return "badge-sukces";
        }

        protected string WyznaczDniTekst(Przeglad p)
        {
            if (p.Status == "Wykonany") return "(zarchiwizowany)";
            TimeSpan roznica = p.DataNastepnego.Date - DateTime.Today;
            int dni = roznica.Days;
            if (dni > 1) return $"(pozostało {dni} dni)";
            if (dni == 1) return "(pozostał 1 dzień)";
            if (dni == 0) return "(dzisiaj!)";
            if (dni == -1) return "(1 dzień po terminie)";
            return $"({Math.Abs(dni)} dni po terminie)";
        }

        protected async Task EksportujDoExcela()
        {
            var kolumny = new Dictionary<string, Func<Przeglad, object>>
            {
                { "Status", p => WyznaczStatusTekst(p) },
                { "Budynek", p => p.Obiekt?.Nazwa ?? "" },
                { "Typ Kontroli", p => p.TerminDefinicja?.NazwaTypu ?? "" },
                { "Data Wykonania", p => p.DataWykonania.ToString("yyyy-MM-dd") },
                { "Termin Następnego", p => p.DataNastepnego.ToString("yyyy-MM-dd") },
                { "Inspektor", p => p.OsobaWykonujaca ?? "" },
                { "Ocena", p => p.WynikOcena ?? "" }
            };

            await ExportDoExcela.ExportToCsvAsExcel(JSRuntime, ListaPrzegladow, "Raport_Przeglady_Techniczne", kolumny);
        }

        protected async Task PodgladPlikuPdf(string nazwaPliku)
        {
            string url = $"uploads/{nazwaPliku}";
            await JSRuntime.InvokeVoidAsync("open", url, "_blank");
        }

        protected async Task PokazZalacznikiDialog(IEnumerable<Zalacznik> zalaczniki)
        {
            var parameters = new DialogParameters { { "Zalaczniki", zalaczniki } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
            await DialogService.ShowAsync<ZalacznikiDialog>("", parameters, options);
        }
    }
}