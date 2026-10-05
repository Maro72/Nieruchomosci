using Microsoft.AspNetCore.Components;
using Mieszkaniec.Model.Entities;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.FRemonBud
{
    public partial class FRemonBudDialog : ComponentBase
    {
        [Parameter] public bool IsVisible { get; set; }
        [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

        [Parameter] public PraceRemontowe Model { get; set; } = new();
        [Parameter] public List<Obiekt> ListaObiektow { get; set; } = new();
        [Parameter] public List<RodzajUsterki> ListaRodzajow { get; set; } = new();
        [Parameter] public List<PriorytetUsterki> ListaPriorytetow { get; set; } = new();
        [Parameter] public List<string> OpcjeStatusow { get; set; } = new();

        [Parameter] public EventCallback OnSave { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }

        protected DialogOptions DialogOptions = new() { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true, BackdropClick = false };

        protected string KomunikatBledu { get; set; } = "";

        // --- ZMIENNE KOSZTORYSU ---
        // Pusty obiekt, który służy jako "formularz" do dodawania nowego wiersza materiału
        protected KosztorysMaterial NowyMaterial { get; set; } = new() { Jm = "szt.", Ilosc = 1, CenaJednostkowa = 0 };

        protected override void OnParametersSet()
        {
            // Gdy otwieramy formularz edycji, wymuszamy jednorazowe przeliczenie kosztów
            if (Model != null)
            {
                if (Model.Materialy == null) Model.Materialy = new List<KosztorysMaterial>();
                PrzeliczKoszty();
            }
        }

        // --- MATEMATYKA KOSZTORYSU ---
        public void PrzeliczKoszty()
        {
            if (Model == null) return;

            // 1. Wzór na Robociznę
            Model.KosztCalkowityRobocizny = Model.LiczbaPracownikow * Model.SzacowanaLiczbaDni * Model.GodzinyDziennie * Model.RobociznaStawkaGodzinowa;

            // 2. Sumowanie materiałów
            decimal sumaMaterialow = 0;
            foreach (var mat in Model.Materialy)
            {
                mat.WartoscCalkowita = mat.Ilosc * mat.CenaJednostkowa;
                sumaMaterialow += mat.WartoscCalkowita;
            }

            // 3. Główny Koszt Szacowany = Robocizna + Materiały
            Model.KosztSzacowany = Model.KosztCalkowityRobocizny + sumaMaterialow;
        }

        // --- ZARZĄDZANIE LISTĄ MATERIAŁÓW ---
        protected void DodajMaterial()
        {
            if (string.IsNullOrWhiteSpace(NowyMaterial.NazwaMaterialu))
            {
                KomunikatBledu = "Podaj nazwę materiału, aby dodać pozycję.";
                return;
            }

            KomunikatBledu = "";

            // Dodajemy do kolekcji w modelu
            NowyMaterial.WartoscCalkowita = NowyMaterial.Ilosc * NowyMaterial.CenaJednostkowa;
            Model.Materialy.Add(NowyMaterial);

            // "Czyścimy" wiersz wprowadzania, przygotowując go na kolejny materiał
            NowyMaterial = new KosztorysMaterial() { Jm = "szt.", Ilosc = 1, CenaJednostkowa = 0 };

            // Przeliczamy całość po dodaniu
            PrzeliczKoszty();
        }

        protected void UsunMaterial(KosztorysMaterial mat)
        {
            Model.Materialy.Remove(mat);
            PrzeliczKoszty();
        }

        // --- STANDARDOWE ZAPISYWANIE ---
        protected async Task ZapiszFormularz()
        {
            KomunikatBledu = "";

            if (Model.ObiektId == 0)
            {
                KomunikatBledu = "Proszę wybrać nieruchomość (budynek) z listy.";
                StateHasChanged(); return;
            }
            if (Model.RodzajUsterkiId == 0)
            {
                KomunikatBledu = "Proszę wybrać branżę (rodzaj prac) z listy.";
                StateHasChanged(); return;
            }
            if (string.IsNullOrWhiteSpace(Model.Nazwa))
            {
                KomunikatBledu = "Nazwa zadania (krótki opis) nie może być pusta.";
                StateHasChanged(); return;
            }
            // --- NOWY WARUNEK: WALIDACJA CHRONOLOGII DAT HARMONOGRAMU ---
            if (Model.DataZakonczeniaPlanowana.Date < Model.DataRozpoczeciaPlanowana.Date)
            {
                KomunikatBledu = "Błąd harmonogramu: Planowana data zakończenia prac nie może być wcześniejsza niż data ich rozpoczęcia.";
                Console.WriteLine($"--> PRZERWANO ZAPIS: DataZakonczenia ({Model.DataZakonczeniaPlanowana:yyyy-MM-dd}) < DataRozpoczecia ({Model.DataRozpoczeciaPlanowana:yyyy-MM-dd})");
                StateHasChanged();
                return; // Blokuje dalsze wykonanie kodu i nie wywołuje okna potwierdzenia
            }

            // Jeśli wszystkie warunki są spełnione, upewniamy się, że koszty są zsumowane i wywołujemy zapis
            PrzeliczKoszty();
            await OnSave.InvokeAsync();
            
        }

        protected async Task Anuluj()
        {
            KomunikatBledu = "";
            await OnCancel.InvokeAsync();
            await IsVisibleChanged.InvokeAsync(false);
        }
    }
}