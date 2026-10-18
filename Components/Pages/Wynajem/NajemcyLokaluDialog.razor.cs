using Microsoft.AspNetCore.Components;
using Mieszkaniec.Model.Entities;
using MudBlazor;

namespace Mieszkaniec.Components.Pages.Wynajem;

public class NajemcyLokaluDialogBase : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public LokalWynajem Lokal { get; set; } = default!;

    protected List<UmowaLokal> AktywnePrzypisania =>
        Lokal.HistoriaUmow?
            .Where(relacja => relacja.UmowaNajmu?.CzyAktywna == true)
            .ToList() ?? new List<UmowaLokal>();

    protected decimal PowierzchniaWynajeta =>
        AktywnePrzypisania.Sum(relacja => relacja.PowierzchniaWynajetaM2);

    protected void Zamknij() => MudDialog.Close();
}
