using Microsoft.AspNetCore.Components;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.FRemonBud
{
    public partial class FRemonBud : ComponentBase
    {
        [Inject] protected IPraceRemontoweService RemontService { get; set; } = default!;

        protected List<PraceRemontowe> ListaPrac { get; set; } = new();
        protected List<Obiekt> ListaObiektow { get; set; } = new();
        protected List<RodzajUsterki> ListaRodzajow { get; set; } = new();
        protected List<PriorytetUsterki> ListaPriorytetow { get; set; } = new();

        protected int? FiltreObiektId { get; set; }
        protected int? FiltreRodzajId { get; set; }
        protected string? FiltreStatus { get; set; }

        protected List<string> OpcjeStatusow { get; set; } = new()
        {
            "Planowany", "W realizacji", "Odbiór techniczny", "Zakończony"
        };

        protected bool IsDialogVisible { get; set; } = false;
        protected PraceRemontowe EdytowanyRemont { get; set; } = new();
        protected int DomyslnyPriorytetId { get; set; }

        // --- ZMIENNE DLA FGRID ---
        protected PraceRemontowe? WybranyRemont { get; set; }
        protected int gridKey = 0;

        // --- ZMIENNE POTWIERDZEŃ ---
        protected bool IsConfirmVisible { get; set; } = false;
        protected string ConfirmTitle { get; set; } = "";
        protected string ConfirmMessage { get; set; } = "";
        protected string ConfirmTheme { get; set; } = "primary";
        protected string ConfirmIcon { get; set; } = "bi-info-circle-fill";
        protected enum TypAkcji { Brak, Usunięcie, Zapis }
        protected TypAkcji OczekujacaAkcja { get; set; } = TypAkcji.Brak;

        protected override async Task OnInitializedAsync()
        {
            ListaObiektow = await RemontService.GetObiektyAsync();
            ListaRodzajow = await RemontService.GetRodzajeAsync();
            ListaPriorytetow = await RemontService.GetPriorytetyAsync();

            var domyslny = ListaPriorytetow.FirstOrDefault(p => p.Nazwa.Contains("Normal") || p.Nazwa.Contains("Standard"))
                           ?? ListaPriorytetow.FirstOrDefault();

            DomyslnyPriorytetId = domyslny?.Id ?? 1;

            await RefreshGridAsync();
        }

        protected async Task RefreshGridAsync()
        {
            ListaPrac = await RemontService.GetPraceAsync(FiltreObiektId, FiltreRodzajId, null, FiltreStatus);
            WybranyRemont = null;
            gridKey++;
            StateHasChanged();
        }

        protected async Task OnObiektFilterChanged(int? id) { FiltreObiektId = id; await RefreshGridAsync(); }
        protected async Task OnRodzajFilterChanged(int? id) { FiltreRodzajId = id; await RefreshGridAsync(); }
        protected async Task OnStatusFilterChanged(string? status) { FiltreStatus = status; await RefreshGridAsync(); }

        protected async Task ResetujFiltry()
        {
            FiltreObiektId = null;
            FiltreRodzajId = null;
            FiltreStatus = null;
            await RefreshGridAsync();
        }

        protected void WybierzWiersz(PraceRemontowe model)
        {
            WybranyRemont = (WybranyRemont != null && WybranyRemont.Id == model.Id) ? null : model;
            StateHasChanged();
        }

        protected void OpenCreateDialog()
        {
            EdytowanyRemont = new PraceRemontowe
            {
                Status = "Planowany",
                DataRozpoczeciaPlanowana = DateTime.Today,
                DataZakonczeniaPlanowana = DateTime.Today.AddDays(7),
                KosztSzacowany = 0,
                KosztFaktyczny = 0,
                PriorytetUsterkiId = DomyslnyPriorytetId
            };
            IsDialogVisible = true;
            StateHasChanged();
        }

        protected async Task OpenEditDialog(PraceRemontowe? model)
        {
            var aktywnyObiekt = model ?? WybranyRemont;
            if (aktywnyObiekt != null)
            {
                var daneZSerwera = await RemontService.GetByIdAsync(aktywnyObiekt.Id);
                if (daneZSerwera != null)
                {
                    EdytowanyRemont = daneZSerwera;
                    if (EdytowanyRemont.PriorytetUsterkiId == 0)
                        EdytowanyRemont.PriorytetUsterkiId = DomyslnyPriorytetId;

                    IsDialogVisible = true;
                    StateHasChanged();
                }
            }
        }

        // --- OBSŁUGA POTWIERDZEŃ ---

        protected void RequestDelete(PraceRemontowe? model)
        {
            var aktywnyObiekt = model ?? WybranyRemont;
            if (aktywnyObiekt != null)
            {
                WybranyRemont = aktywnyObiekt;
                OczekujacaAkcja = TypAkcji.Usunięcie;
                ConfirmTitle = "Usuwanie remontu";
                ConfirmMessage = "Czy na pewno chcesz nieodwracalnie usunąć to zadanie remontowe ze wszystkich ewidencji?";
                ConfirmTheme = "danger";
                IsConfirmVisible = true;
                StateHasChanged();
            }
        }

        protected void ZazadajPotwierdzeniaZapisu()
        {
            WybranyRemont = EdytowanyRemont;
            OczekujacaAkcja = TypAkcji.Zapis;
            ConfirmTitle = "Zapisywanie Kosztorysu";
            ConfirmMessage = "Czy na pewno chcesz zapisać wprowadzone dane oraz informacje o kosztach do bazy?";
            ConfirmTheme = "primary";
            IsConfirmVisible = true;
            StateHasChanged();
        }

        protected async Task HandleConfirmationAnswer(bool czyZatwierdzono)
        {
            IsConfirmVisible = false;

            if (czyZatwierdzono && WybranyRemont != null)
            {
                try
                {
                    if (OczekujacaAkcja == TypAkcji.Zapis)
                    {
                        await RemontService.SaveAsync(WybranyRemont);
                        IsDialogVisible = false; // Zamykamy główne okno po udanym zapisie
                    }
                    else if (OczekujacaAkcja == TypAkcji.Usunięcie)
                    {
                        await RemontService.DeleteAsync(WybranyRemont.Id);
                    }

                    await RefreshGridAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BŁĄD ZAPISU/USUNIĘCIA] {ex.Message}");
                }
            }
            StateHasChanged();
        }

        protected void CloseDialog()
        {
            IsDialogVisible = false;
            StateHasChanged();
        }

        protected string WyznaczKlasuStatusu(string status) => status switch
        {
            "Planowany" => "status-planowany",
            "W realizacji" => "status-realizacja",
            "Odbiór techniczny" => "status-odbior",
            "Zakończony" => "status-zakonczony",
            _ => "status-domyslny"
        };
        protected bool CzyWidokKanban { get; set; } = false;

        protected void OnWidokChanged(bool val)
        {
            CzyWidokKanban = val;
            StateHasChanged();
        }

        // Ta funkcja wykonuje się automatycznie po upuszczeniu karty w innej kolumnie Kanban
        private async Task ObslugaPrzeniesieniaKartyKanban(MudItemDropInfo<PraceRemontowe> dropInfo)
        {
            if (dropInfo?.Item == null || string.IsNullOrEmpty(dropInfo.DropzoneIdentifier))
                return;

            // 1. Aktualizujemy status w obiekcie na podstawie kolumny, do której trafił
            dropInfo.Item.Status = dropInfo.DropzoneIdentifier;

            // 2. Tutaj wywołaj swoją logikę zapisu do bazy danych, np.:
            // await _praceService.UpdateStatusAsync(dropInfo.Item.Id, dropInfo.DropzoneIdentifier);

            // 3. Opcjonalnie odśwież widok, jeśli jest taka potrzeba
            StateHasChanged();
            // Opcja B: Jeśli wolisz odpalić Twój standardowy modal z pytaniem "Czy zapisać?":
            // EdytowanyRemont = remont;
            // ZazadajPotwierdzeniaZapisu(remont);
        }
        

        // Metoda pomocnicza dla dynamicznego dopasowania kolorów linii bocznych kart Kanban (Hex CSS)
        protected string GetHexColorDlaStatusu(string status) => status switch
        {
            "Planowany" => "#6c757d",       // Grey
            "W realizacji" => "#0d6efd",    // Blue
            "Odbiór techniczny" => "#ffc107",// Yellow/Orange
            "Zakończony" => "#198754",      // Green
            _ => "#dee2e6"
        };
    }
}