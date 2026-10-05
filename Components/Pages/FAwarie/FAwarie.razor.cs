using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services;
using Mieszkaniec.Services.Interfaces;
using MudBlazor; // Wymagane dla DialogService
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.FAwarie
{
    public partial class FAwarie : ComponentBase
    {
        [Inject] protected IUsterkiBudService UsterkiService { get; set; } = default!;
        [Inject] protected MieszkaniecDbContext DbContext { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        // Wstrzyknięcie serwisu do okienek
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;

        protected List<UsterkiBud> ListaUsterek { get; set; } = new();
        protected List<Obiekt> ListaObiektow { get; set; } = new();
        protected List<RodzajUsterki> ListaRodzajow { get; set; } = new();
        protected List<PriorytetUsterki> ListaPriorytetow { get; set; } = new();
        protected List<string> OpcjeStatusow { get; set; } = new() { "Nowe", "W weryfikacji", "W naprawie", "Zakończone" };

        protected int? FiltreObiektId { get; set; }
        protected int? FiltreRodzajId { get; set; }
        protected string? FiltreStatus { get; set; }
        protected bool PokazujArchiwum { get; set; } = false;

        protected bool IsDialogVisible { get; set; } = false;
        protected UsterkiBud EdytowanyModel { get; set; } = new();
        protected bool IsConfirmSaveVisible { get; set; } = false;

        protected string ConfirmTitle { get; set; } = "";
        protected string ConfirmMessage { get; set; } = "";
        protected string ConfirmTheme { get; set; } = "primary";
        protected string ConfirmIcon { get; set; } = "bi-info-circle-fill";

        protected enum TypAkcji { Brak, Usunięcie, Zapis }
        protected TypAkcji OczekujacaAkcja { get; set; } = TypAkcji.Brak;

        protected UsterkiBud? ObiektDoPrzetworzenia { get; set; }
        protected int gridKey = 0;

        protected override async Task OnInitializedAsync()
        {
            ListaObiektow = await DbContext.Obiekty.Where(o => !o.CzyArchiwum).OrderBy(o => o.Nazwa).ToListAsync();
            ListaRodzajow = await UsterkiService.GetRodzajeAsync();
            ListaPriorytetow = await UsterkiService.GetPriorytetyAsync();
            await RefreshGridAsync();
        }

        protected async Task RefreshGridAsync()
        {
            ListaUsterek = await UsterkiService.GetUsterkiAsync(FiltreObiektId, FiltreRodzajId, FiltreStatus, PokazujArchiwum);
            ObiektDoPrzetworzenia = null; // Czyszczenie wyboru po odświeżeniu
            gridKey++;
            StateHasChanged();
        }

        protected async Task RefreshGridAfterObiekt(int? id) { FiltreObiektId = id; await RefreshGridAsync(); }
        protected async Task RefreshGridAfterRodzaj(int? id) { FiltreRodzajId = id; await RefreshGridAsync(); }
        protected async Task RefreshGridAfterStatus(string? status) { FiltreStatus = status; await RefreshGridAsync(); }

        protected async Task PrzelaczWidokArchiwum(bool stan)
        {
            PokazujArchiwum = stan;
            FiltreStatus = null;
            await RefreshGridAsync();
        }

        protected async Task ResetujFiltry()
        {
            FiltreObiektId = null;
            FiltreRodzajId = null;
            FiltreStatus = null;
            PokazujArchiwum = false;
            await RefreshGridAsync();
        }

        protected void WybierzWiersz(UsterkiBud model)
        {
            ObiektDoPrzetworzenia = (ObiektDoPrzetworzenia != null && ObiektDoPrzetworzenia.Id == model.Id) ? null : model;
            StateHasChanged();
        }

        protected void OpenCreateDialog()
        {
            EdytowanyModel = new UsterkiBud
            {
                Id = 0,
                ObiektId = 0,
                RodzajUsterkiId = 0,
                PriorytetUsterkiId = 0,
                OsobaZglaszajaca = string.Empty,
                OpisZgłoszenia = string.Empty,
                Status = "Nowe",
                DataZgloszenia = DateTime.Now,
                DataZakonczeniaNaprawy = null,
                UwagiKonserwatora = null,
                CzyArchiwum = false,
                Zalaczniki = new List<Zalacznik>()
            };
            IsDialogVisible = true;
            StateHasChanged();
        }

        protected void OpenEditDialog(UsterkiBud? model)
        {
            var aktywnyObiekt = model ?? ObiektDoPrzetworzenia;

            if (aktywnyObiekt != null)
            {
                EdytowanyModel = new UsterkiBud
                {
                    Id = aktywnyObiekt.Id,
                    ObiektId = aktywnyObiekt.ObiektId,
                    OsobaZglaszajaca = aktywnyObiekt.OsobaZglaszajaca,
                    DataZgloszenia = aktywnyObiekt.DataZgloszenia,
                    OpisZgłoszenia = aktywnyObiekt.OpisZgłoszenia,
                    RodzajUsterkiId = aktywnyObiekt.RodzajUsterkiId,
                    PriorytetUsterkiId = aktywnyObiekt.PriorytetUsterkiId,
                    Status = aktywnyObiekt.Status,
                    DataZakonczeniaNaprawy = aktywnyObiekt.DataZakonczeniaNaprawy,
                    UwagiKonserwatora = aktywnyObiekt.UwagiKonserwatora,
                    CzyArchiwum = aktywnyObiekt.CzyArchiwum,
                    Zalaczniki = aktywnyObiekt.Zalaczniki?.ToList()
                };

                IsDialogVisible = true;
                StateHasChanged();
            }
        }

        protected void RequestDelete(UsterkiBud? model)
        {
            var aktywnyObiekt = model ?? ObiektDoPrzetworzenia;

            if (aktywnyObiekt != null)
            {
                ZazadajPotwierdzeniaUsuniecia(aktywnyObiekt);
            }
        }

        protected void ZazadajPotwierdzeniaZapisu()
        {
            if (EdytowanyModel == null) return;

            if (EdytowanyModel.ObiektId <= 0)
            {
                Snackbar.Add("Proszę wybrać nieruchomość/budynek.", Severity.Warning);
                return;
            }
            if (EdytowanyModel.RodzajUsterkiId <= 0)
            {
                Snackbar.Add("Proszę wybrać kategorię usterki.", Severity.Warning);
                return;
            }
            if (EdytowanyModel.PriorytetUsterkiId <= 0)
            {
                Snackbar.Add("Proszę wybrać priorytet usterki.", Severity.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(EdytowanyModel.OsobaZglaszajaca))
            {
                Snackbar.Add("Proszę wpisać osobę zgłaszającą.", Severity.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(EdytowanyModel.OpisZgłoszenia))
            {
                Snackbar.Add("Proszę wpisać opis usterki.", Severity.Warning);
                return;
            }

            ObiektDoPrzetworzenia = EdytowanyModel;
            OczekujacaAkcja = TypAkcji.Zapis;
            ConfirmTitle = "Zapis zgłoszenia";
            ConfirmMessage = "Czy na pewno chcesz zapisać usterkę?";
            ConfirmTheme = "primary";
            IsConfirmSaveVisible = true;
            StateHasChanged();
        }

        protected void ZazadajPotwierdzeniaUsuniecia(UsterkiBud model)
        {
            ObiektDoPrzetworzenia = model;
            OczekujacaAkcja = TypAkcji.Usunięcie;
            ConfirmTitle = "Usuwanie";
            ConfirmMessage = "Czy na pewno chcesz nieodwracalnie usunąć to zgłoszenie?";
            ConfirmTheme = "danger";
            IsConfirmSaveVisible = true;
            StateHasChanged();
        }

        protected async Task HandleConfirmationAnswer(bool czyZatwierdzono)
        {
            IsConfirmSaveVisible = false;

            if (czyZatwierdzono && ObiektDoPrzetworzenia != null)
            {
                if (OczekujacaAkcja == TypAkcji.Zapis)
                {
                    bool sukces = await UsterkiService.SaveAsync(ObiektDoPrzetworzenia);
                    if (sukces)
                    {
                        Snackbar.Add("Zgłoszenie usterki zostało pomyślnie zapisane.", Severity.Success);
                        IsDialogVisible = false;
                        await RefreshGridAsync();
                    }
                    else
                    {
                        Snackbar.Add("Nie udało się zapisać usterki. Upewnij się, że wybrane pola są poprawne.", Severity.Error);
                    }
                }
                else if (OczekujacaAkcja == TypAkcji.Usunięcie)
                {
                    bool sukces = await UsterkiService.DeleteAsync(ObiektDoPrzetworzenia.Id);
                    if (sukces)
                    {
                        Snackbar.Add("Zgłoszenie usunięte.", Severity.Info);
                        await RefreshGridAsync();
                    }
                    else
                    {
                        Snackbar.Add("Nie udało się usunąć zgłoszenia.", Severity.Error);
                    }
                }
            }
            StateHasChanged();
        }

        protected void CloseDialog()
        {
            IsDialogVisible = false;
            StateHasChanged();
        }

        protected async Task PodgladPlikuPdf(string nazwa) => await JSRuntime.InvokeVoidAsync("open", $"uploads/{nazwa}", "_blank");

        // --- OKNO ZAŁĄCZNIKÓW IDENTYCZNE JAK W PRZEGLĄDACH ---
        protected async Task PokazZalacznikiDialog(IEnumerable<Zalacznik> zalaczniki)
        {
            var parameters = new DialogParameters
            {
                { "Zalaczniki", zalaczniki },
                { "OnPodglad", new Action<string>(async (nazwaDyskowa) => await PodgladPlikuPdf(nazwaDyskowa)) }
            };

            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
            await DialogService.ShowAsync<ZalacznikiDialog>("", parameters, options);
        }

        protected string WyznaczKlasuStatusu(string status) => status switch
        {
            "Nowe" => "ust-status-nowe",
            "W weryfikacji" => "ust-status-weryfikacja",
            "W naprawie" => "ust-status-naprawa",
            "Zakończone" => "ust-status-zakonczone",
            _ => "ust-status-nowe"
        };

        protected string WyznaczKlasuPriorytetu(string? p) => p switch
        {
            "Pilne" => "prio-status-pilne",
            "Wysoki" => "prio-status-wysoki",
            _ => "prio-status-normalny"
        };
       
    }
}