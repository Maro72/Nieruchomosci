using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Mieszkaniec.Model.Entities;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.FAwarie
{
    public partial class FAwarieDialog : ComponentBase
    {
        [Inject] protected IWebHostEnvironment Env { get; set; } = default!;

        [Parameter] public bool IsVisible { get; set; }
        [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

        [Parameter] public UsterkiBud Model { get; set; } = new();
        [Parameter] public List<Obiekt> ListaObiektow { get; set; } = new();
        [Parameter] public List<RodzajUsterki> ListaRodzajow { get; set; } = new();
        [Parameter] public List<PriorytetUsterki> ListaPriorytetow { get; set; } = new();

        [Parameter] public EventCallback OnSave { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }

        protected List<string> OpcjeStatusow { get; set; } = new() { "Nowe", "W weryfikacji", "W naprawie", "Zakończone" };

        // POPRAWKA: Zmiana "DisableBackdropClick" na poprawną nazwę "BackdropClick = false" w MudBlazor
        protected DialogOptions DialogOptions = new() { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true, BackdropClick = false };

        protected string Title => Model?.Id == 0 ? "Zarejestruj nowe zgłoszenie awarii" : (Model?.CzyArchiwum == true ? "Podgląd awarii (Zablokowane)" : "Edycja zgłoszenia awarii");

        protected DateTime? DataZgloszeniaMud
        {
            get => Model?.DataZgloszenia;
            set { if (Model != null && value.HasValue) Model.DataZgloszenia = value.Value; }
        }

        protected DateTime? DataZakonczeniaMud
        {
            get => Model?.DataZakonczeniaNaprawy;
            set { if (Model != null) Model.DataZakonczeniaNaprawy = value; }
        }

        protected async Task Save()
        {
            await OnSave.InvokeAsync();
        }

        protected async Task Cancel()
        {
            await OnCancel.InvokeAsync();
            await IsVisibleChanged.InvokeAsync(false);
        }

        protected async Task OnUploadFiles(IReadOnlyList<IBrowserFile> files)
        {
            if (Model == null || files == null) return;

            if (Model.Zalaczniki == null)
                Model.Zalaczniki = new List<Zalacznik>();

            foreach (var file in files)
            {
                try
                {
                    var folderUploads = Path.Combine(Env.WebRootPath, "uploads");
                    if (!Directory.Exists(folderUploads)) Directory.CreateDirectory(folderUploads);

                    var unikalnaNazwa = $"{Guid.NewGuid()}_{file.Name}";
                    var pelnaSciezka = Path.Combine(folderUploads, unikalnaNazwa);

                    using (var stream = new FileStream(pelnaSciezka, FileMode.Create))
                    {
                        await file.OpenReadStream(long.MaxValue).CopyToAsync(stream);
                    }

                    Model.Zalaczniki.Add(new Zalacznik
                    {
                        Id = 0,
                        NazwaPliku = file.Name,
                        SciezkaMagazyn = pelnaSciezka
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd wgrywania PDF: {ex.Message}");
                }
            }
        }

        protected void UsunZalacznik(Zalacznik zalacznik)
        {
            Model?.Zalaczniki?.Remove(zalacznik);
        }
    }
}