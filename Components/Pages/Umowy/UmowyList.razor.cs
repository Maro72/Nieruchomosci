using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services;
using Mieszkaniec.Services.Interfaces;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.Umowy
{
    public class UmowyListBase : ComponentBase
    {
        [Inject] protected IUmowaService UmowaService { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected INajemcaService NajemcaService { get; set; } = default!;
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

        [Parameter]
        [SupplyParameterFromQuery]
        public int? NajemcaId { get; set; }

        // Referencja do nawigacji, by móc wyczyścić filtr z paska adresu
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

        protected List<UmowaNajmu>? Umowy { get; set; }
        protected List<Najemca> ListaNajemcow { get; set; } = new();

        protected bool PokazTylkoAktywne { get; set; } = true;
        protected int WszystkieUmowy { get; set; }
        protected int AktywneUmowy { get; set; }
        protected int NieaktywneUmowy { get; set; }

        protected int WygasajaceUmowy { get; set; }



        // Stan otwarcia bocznego panelu
        protected bool CzyOtwartoSzczegoly { get; set; } = false;

        // Obiekt umowy, której szczegóły aktualnie wyświetlamy
        protected UmowaNajmu? WybranaUmowa { get; set; }

        // Metoda uruchamiana po kliknięciu ikony podglądu
        protected void OtworzSzczegoly(UmowaNajmu umowa)
        {
            WybranaUmowa = umowa;
            CzyOtwartoSzczegoly = true;
        }

        // ZMIANA: Zamiast OnInitializedAsync używamy OnParametersSetAsync,
        // aby upewnić się, że parametr NajemcaId z URL został już poprawnie przypisany przez Blazora.
        protected override async Task OnParametersSetAsync()
        {
            var najemcyBazy = await NajemcaService.PobierzAktywnychAsync();
            ListaNajemcow = najemcyBazy ?? new List<Najemca>();

            await OdswiezListe();
        }

        protected async Task OnPokazTylkoAktywneChanged(bool wartosc)
        {
            PokazTylkoAktywne = wartosc;
            await OdswiezListe();
        }

        protected async Task OdswiezListe()
        {
            // 1. Pobieramy wszystkie umowy (zależnie od przełącznika Aktywne/Archiwalne)
            var umowyZBazy = await UmowaService.PobierzWszystkieUmowyAsync(PokazTylkoAktywne) ?? new List<UmowaNajmu>();

            // 2. Pobieramy absolutnie wszystko do statystyk (kafelków)
            var wszystkieDoStatystyk = await UmowaService.PobierzWszystkieUmowyAsync(null) ?? new List<UmowaNajmu>();

            // KLUCZOWA ZMIANA: Filtrujemy w pamięci, jeśli w adresie URL jest ID najemcy
            if (NajemcaId.HasValue && NajemcaId.Value > 0)
            {
                umowyZBazy = umowyZBazy.Where(u => u.NajemcaId == NajemcaId.Value).ToList();
                wszystkieDoStatystyk = wszystkieDoStatystyk.Where(u => u.NajemcaId == NajemcaId.Value).ToList();
            }

            Umowy = umowyZBazy;

            WszystkieUmowy = wszystkieDoStatystyk.Count;
            AktywneUmowy = wszystkieDoStatystyk.Count(u => u.CzyAktywna);
            NieaktywneUmowy = wszystkieDoStatystyk.Count(u => !u.CzyAktywna);
            // NOWOŚĆ: Zliczamy umowy, które wygasają w ciągu najbliższych 30 dni
            WygasajaceUmowy = wszystkieDoStatystyk?.Count(u => u.CzyWygasaWkrotce) ?? 0;

            StateHasChanged();
        }

        // Dodana metoda do resetowania filtru
        protected void WyczyscFiltrNajemcy()
        {
            NavigationManager.NavigateTo("/umowy");
        }

        protected async Task OpenDodajDialog()
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };

            // Jeśli użytkownik jest w widoku filtrowania najemcy i klika "Dodaj umowę",
            // możemy mu od razu domyślnie podpiąć tego najemcę w formularzu!
            var nowaUmowa = new UmowaNajmu { DataOd = DateTime.Today, CzyAktywna = true };
            if (NajemcaId.HasValue && NajemcaId.Value > 0)
            {
                nowaUmowa.NajemcaId = NajemcaId.Value;
            }

            var parameters = new DialogParameters
            {
                ["Model"] = nowaUmowa,
                ["ListaNajemcow"] = ListaNajemcow
            };

            var dialog = await DialogService.ShowAsync<UmowyDialog>("Nowa umowa najmu", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await OdswiezListe();
            }
        }

        protected async Task OpenEdytujDialog(UmowaNajmu umowa)
        {
            if (umowa == null) return;

            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };

            var klonUmowy = new UmowaNajmu
            {
                Id = umowa.Id,
                NumerUmowy = umowa.NumerUmowy,
                NajemcaId = umowa.NajemcaId,
                DataOd = umowa.DataOd,
                DataDo = umowa.DataDo,
                CzyAktywna = umowa.CzyAktywna,
                Status = umowa.Status,
                DataWypowiedzenia = umowa.DataWypowiedzenia,
                OkresWypowiedzeniaDni = umowa.OkresWypowiedzeniaDni,
                DataPlanowanegoZakonczenia = umowa.DataPlanowanegoZakonczenia,
                DataFaktycznegoZakonczenia = umowa.DataFaktycznegoZakonczenia,
                PowodWypowiedzenia = umowa.PowodWypowiedzenia,
                Zalaczniki = umowa.Zalaczniki ?? new(),
                Aneksy = umowa.Aneksy ?? new()
            };

            var parameters = new DialogParameters
            {
                ["Model"] = klonUmowy,
                ["ListaNajemcow"] = ListaNajemcow
            };

            var dialog = await DialogService.ShowAsync<UmowyDialog>("Edycja umowy", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await OdswiezListe();
            }
        }

        protected async Task PokazZalacznikiDialog(IEnumerable<ZalacznikUmowy> zalaczniki)
        {
            var parameters = new DialogParameters { { "Zalaczniki", zalaczniki } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
            await DialogService.ShowAsync<ZalacznikiDialog>("", parameters, options);
        }

        protected async Task PodgladPlikuPdf(string nazwaPliku)
        {
            string url = $"/upload_umowy/{nazwaPliku}";
            await JSRuntime.InvokeVoidAsync("open", url, "_blank");
        }
    }
}