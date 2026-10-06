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
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;

        protected List<PraceRemontowe> ListaPrac { get; set; } = new();
        protected List<Obiekt> ListaObiektow { get; set; } = new();
        protected List<RodzajUsterki> ListaRodzajow { get; set; } = new();
        protected List<PriorytetUsterki> ListaPriorytetow { get; set; } = new();

        protected int? FiltreObiektId { get; set; }
        protected int? FiltreRodzajId { get; set; }
        protected string? FiltreStatus { get; set; }

        protected List<string> OpcjeStatusow { get; set; } = new()
        {
            "Planowany", "W realizacji", "Odbiór techniczny", "Anulowany"
        };
        protected List<string> StatusyKanban { get; set; } = new()
        {
            "Planowany", "W realizacji", "Odbiór techniczny"
        };

        protected bool CzyArchiwum { get; set; }
        protected bool IsRestoreDialogVisible { get; set; }
        protected bool IsRestoring { get; set; }
        protected string StatusPrzywrocenia { get; set; } = "Planowany";
        protected bool IsFinishDialogVisible { get; set; }
        protected DateTime? DataZakonczeniaRzeczywista { get; set; } = DateTime.Today;
        protected decimal KosztFaktyczny { get; set; }
        protected bool IsFinishing { get; set; }
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
            ListaPrac = await RemontService.GetPraceAsync(
                FiltreObiektId,
                FiltreRodzajId,
                null,
                CzyArchiwum ? null : FiltreStatus,
                CzyArchiwum);
            WybranyRemont = null;
            gridKey++;
            StateHasChanged();
        }

        protected async Task OnObiektFilterChanged(int? id) { FiltreObiektId = id; await RefreshGridAsync(); }
        protected async Task OnRodzajFilterChanged(int? id) { FiltreRodzajId = id; await RefreshGridAsync(); }
        protected async Task OnStatusFilterChanged(string? status) { FiltreStatus = status; await RefreshGridAsync(); }

        protected async Task UstawWidokArchiwum(bool archiwum)
        {
            CzyArchiwum = archiwum;
            FiltreStatus = null;
            WybranyRemont = null;
            await RefreshGridAsync();
        }

        protected Task OnWidokArchiwumChanged(bool archiwum) => UstawWidokArchiwum(archiwum);

        protected void OtworzDialogPrzywrocenia()
        {
            if (!CzyArchiwum || WybranyRemont == null)
                return;

            StatusPrzywrocenia = "Planowany";
            IsRestoreDialogVisible = true;
        }

        protected async Task PrzywrocRemont()
        {
            if (WybranyRemont == null || !CzyArchiwum)
                return;

            IsRestoring = true;
            try
            {
                var przywrocono = await RemontService.PrzywrocRemontAsync(
                    WybranyRemont.Id,
                    StatusPrzywrocenia);

                if (!przywrocono)
                {
                    Snackbar.Add("Nie udało się przywrócić remontu. Odśwież archiwum i spróbuj ponownie.", Severity.Error);
                    return;
                }

                IsRestoreDialogVisible = false;
                Snackbar.Add("Remont przywrócono do aktywnych prac.", Severity.Success);
                await RefreshGridAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Nie udało się przywrócić remontu: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsRestoring = false;
            }
        }

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

        protected void OtworzDialogZakonczenia() => OtworzDialogZakonczenia(null);

        protected void OtworzDialogZakonczenia(PraceRemontowe? remont)
        {
            WybranyRemont = remont ?? WybranyRemont;
            if (WybranyRemont?.Status != "Odbiór techniczny")
                return;

            DataZakonczeniaRzeczywista = DateTime.Today;
            KosztFaktyczny = WybranyRemont.KosztFaktyczny;
            IsFinishDialogVisible = true;
        }

        protected Task OnFinishToggleChanged(bool isChecked, PraceRemontowe remont)
        {
            if (isChecked)
                OtworzDialogZakonczenia(remont);

            return Task.CompletedTask;
        }

        protected async Task ZakonczRemont()
        {
            if (WybranyRemont == null || !DataZakonczeniaRzeczywista.HasValue)
                return;

            if (KosztFaktyczny < 0)
            {
                Snackbar.Add("Koszt faktyczny nie może być ujemny.", Severity.Warning);
                return;
            }

            IsFinishing = true;
            try
            {
                var zapisano = await RemontService.ZakonczRemontAsync(
                    WybranyRemont.Id,
                    DataZakonczeniaRzeczywista.Value,
                    KosztFaktyczny);

                if (!zapisano)
                {
                    Snackbar.Add("Nie udało się zakończyć remontu. Sprawdź datę i stan pracy.", Severity.Error);
                    return;
                }

                IsFinishDialogVisible = false;
                Snackbar.Add("Remont zakończono i przeniesiono do archiwum.", Severity.Success);
                await RefreshGridAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Nie udało się zakończyć remontu: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsFinishing = false;
            }
        }

        protected async Task ZakonczRemontZDialogu(DateTime dataZakonczenia)
        {
            if (EdytowanyRemont.Id == 0 || EdytowanyRemont.Status != "Odbiór techniczny")
            {
                Snackbar.Add("Zakończyć można tylko zapisany remont po odbiorze technicznym.", Severity.Warning);
                return;
            }

            IsFinishing = true;
            try
            {
                var zapisanoZmiany = await RemontService.SaveAsync(EdytowanyRemont);
                if (!zapisanoZmiany)
                {
                    Snackbar.Add("Nie udało się zapisać zmian remontu przed jego zakończeniem.", Severity.Error);
                    return;
                }

                var zakonczono = await RemontService.ZakonczRemontAsync(
                    EdytowanyRemont.Id,
                    dataZakonczenia,
                    EdytowanyRemont.KosztFaktyczny);

                if (!zakonczono)
                {
                    Snackbar.Add("Nie udało się zakończyć remontu. Sprawdź datę i stan pracy.", Severity.Error);
                    return;
                }

                IsDialogVisible = false;
                Snackbar.Add("Remont zakończono i przeniesiono do archiwum.", Severity.Success);
                await RefreshGridAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Nie udało się zakończyć remontu: {ex.Message}", Severity.Error);
            }
            finally
            {
                IsFinishing = false;
            }
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
                        var zapisano = await RemontService.SaveAsync(WybranyRemont);
                        if (!zapisano)
                        {
                            Snackbar.Add("Nie udało się zapisać zmian remontu.", Severity.Error);
                            return;
                        }

                        IsDialogVisible = false; // Zamykamy główne okno po udanym zapisie
                    }
                    else if (OczekujacaAkcja == TypAkcji.Usunięcie)
                    {
                        var usunieto = await RemontService.DeleteAsync(WybranyRemont.Id);
                        if (!usunieto)
                        {
                            Snackbar.Add("Nie udało się usunąć remontu.", Severity.Error);
                            return;
                        }
                    }

                    await RefreshGridAsync();
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"Nie udało się wykonać operacji na remoncie: {ex.Message}", Severity.Error);
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
            "Anulowany" => "status-anulowany",
            _ => "status-domyslny"
        };

        protected string StylStatusu(string status) => status switch
        {
            "Planowany" => "color:#475569",
            "W realizacji" => "color:#15803d",
            "Odbiór techniczny" => "color:#92400e",
            "Zakończony" => "color:#166534",
            "Anulowany" => "color:#dc2626",
            _ => "color:#212529"
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

            try
            {
                var zapisano = await RemontService.AktualizujStatusAsync(dropInfo.Item.Id, dropInfo.DropzoneIdentifier);
                if (!zapisano)
                {
                    Snackbar.Add("Nie udało się zmienić statusu remontu.", Severity.Error);
                    await RefreshGridAsync();
                    return;
                }

                await RefreshGridAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Nie udało się zmienić statusu remontu: {ex.Message}", Severity.Error);
            }
        }
        

        // Metoda pomocnicza dla dynamicznego dopasowania kolorów linii bocznych kart Kanban (Hex CSS)
        protected string GetHexColorDlaStatusu(string status) => status switch
        {
            "Planowany" => "#6c757d",       // Grey
            "W realizacji" => "#15803d",    // Green
            "Odbiór techniczny" => "#ffc107",// Yellow/Orange
            "Zakończony" => "#198754",      // Green
            "Anulowany" => "#dc2626",        // Red
            _ => "#dee2e6"
        };
    }
}