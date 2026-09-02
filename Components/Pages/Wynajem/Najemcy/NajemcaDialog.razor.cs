using Microsoft.AspNetCore.Components;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;
using MudBlazor;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.Wynajem.Najemcy
{
    public partial class NajemcaDialog : ComponentBase
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

        [Inject] INajemcaService NajemcaService { get; set; }

        [Parameter] public bool TylkoPodglad { get; set; } = false;
        [Parameter] public Najemca Model { get; set; } = new Najemca();

        private async Task Zapisz()
        {
            // Zabezpieczenie systemowe: w trybie podglądu fizycznie blokujemy możliwość zapisu
            if (TylkoPodglad) return;

            var sukces = await NajemcaService.ZapiszAsync(Model);

            if (sukces)
            {
                MudDialog.Close(DialogResult.Ok(true));
            }
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}