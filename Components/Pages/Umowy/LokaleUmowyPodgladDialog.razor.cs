using Microsoft.AspNetCore.Components;
using Mieszkaniec.Model.Entities;
using MudBlazor;

namespace Mieszkaniec.Components.Pages.Umowy;

public class LokaleUmowyPodgladDialogBase : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public UmowaNajmu Umowa { get; set; } = default!;

    protected int LiczbaLokali => Umowa.WynajmowaneLokale?.Count ?? 0;

    protected decimal PowierzchniaLaczna =>
        Umowa.WynajmowaneLokale?.Sum(lokal => lokal.PowierzchniaWynajetaM2) ?? 0;

    protected decimal CzynszLaczny =>
        Umowa.WynajmowaneLokale?.Sum(lokal => lokal.WartoscCzynszuMiesiecznego) ?? 0;

    protected void Zamknij() => MudDialog.Close();
}
