using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.Wynajem
{
    public partial class LokalWynajemDialog : ComponentBase
    {
        [CascadingParameter] public IMudDialogInstance MudDialog { get; set; }
        [Inject] public ILokalWynajemService LokalService { get; set; }
        [Inject] public IJSRuntime JS { get; set; }

        [Parameter] public LokalWynajem Model { get; set; } = new LokalWynajem();
        [Parameter] public List<Obiekt> ListaBudynkow { get; set; } = new();
        [Parameter] public List<Najemca> ListaNajemcow { get; set; } = new();

        private CultureInfo _plCulture = new CultureInfo("pl-PL");

        private async Task Zapisz()
        {
            try
            {
                if (MudDialog == null) throw new Exception("MudDialog nie został wstrzyknięty.");
                if (LokalService == null) throw new Exception("LokalService nie został wstrzyknięty.");

                // KROK 1: Zapisujemy zmiany w bazie. Entity Framework potrzebuje TYLKO kluczy obcych (ObiektId, NajemcaId), 
                // które model już posiada pobrane z kontrolek <select>. Nie ładujemy tu obiektów, żeby uniknąć konfliktów.
                var sukces = await LokalService.ZapiszAsync(Model);

                if (sukces)
                {
                    // KROK 2: Po udanym zapisie aktualizujemy obiekty na podstawie ID, 
                    // tylko po to, żeby tabela w głównym widoku ładnie wyświetliła zaktualizowane nazwy, a nie same ID.
                    Model.Obiekt = ListaBudynkow?.FirstOrDefault(b => b.Id == Model.ObiektId);
                    Model.Najemca = Model.NajemcaId.HasValue
                        ? ListaNajemcow?.FirstOrDefault(n => n.Id == Model.NajemcaId)
                        : null;

                    MudDialog.Close(DialogResult.Ok(true));
                }
            }
            catch (Exception ex)
            {
                // WYCIĄGAMY INNER EXCEPTION - teraz zobaczysz dokładnie to, co rzuciła baza danych zamiast ogólnego komunikatu!
                var errorDetails = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                await JS.InvokeVoidAsync("alert", $"Krytyczny błąd podczas zapisu:\n\n{errorDetails}");
            }
        }

        private async Task Cancel()
        {
            try
            {
                MudDialog?.Cancel();
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("alert", $"Krytyczny błąd podczas anulowania:\n\n{ex.Message}");
            }
        }

        private void OnStatusChanged(ChangeEventArgs e)
        {
            Model.Status = e.Value?.ToString();

            if (Model.Status == "Wolny")
            {
                Model.NajemcaId = null;
            }
        }
    }
    }