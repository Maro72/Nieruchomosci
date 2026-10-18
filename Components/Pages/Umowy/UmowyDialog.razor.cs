using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services;
using Mieszkaniec.Services.Interfaces;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mieszkaniec.Components.Pages.Umowy
{
    public class UmowyDialogBase : ComponentBase
    {
        [CascadingParameter] public IMudDialogInstance MudDialog { get; set; }

        [Inject] protected IUmowaService UmowaService { get; set; }
        [Inject] protected INajemcaService NajemcaService { get; set; }
        [Inject] protected ILokalWynajemService LokalWynajemService { get; set; }
        [Inject] protected IJSRuntime JS { get; set; }

        [Parameter] public UmowaNajmu Model { get; set; }
        [Parameter] public List<Najemca> ListaNajemcow { get; set; } = new();

        // Lista do załadowania pomieszczeń z bazy danych
        protected List<LokalWynajem> ListaDostepnychLokali { get; set; } = new();

        // Pola sterujące formularzem dodawania lokalu do umowy
        protected int WybranyLokalId { get; set; }
        protected decimal WybranaCena { get; set; }
        protected decimal WybranaPowierzchniaM2 { get; set; }
        protected decimal WybranaDostepnaPowierzchniaM2 { get; set; }
        protected bool NowyCzyRyczalt { get; set; }

        protected AneksUmowy NowyAneks { get; set; } = new AneksUmowy { DataZawarcia = DateTime.Today };

        private IBrowserFile _wybranyPlik;
        protected string WybranyPlikNazwa = string.Empty;
        protected long WybranyPlikRozmiarKB = 0;

        private bool _trwaPrzetwarzanie = false;
        protected bool CzyPotwierdzicOperacje { get; set; }
        protected string TytulPotwierdzenia { get; set; } = string.Empty;
        protected string TrescPotwierdzenia { get; set; } = string.Empty;
        private Func<Task>? _potwierdzonaAkcja;

        protected override async Task OnInitializedAsync()
        {
            // Ładowanie najemców
            if (ListaNajemcow == null || ListaNajemcow.Count == 0)
            {
                ListaNajemcow = await NajemcaService.PobierzAktywnychAsync() ?? new List<Najemca>();
            }

            // Ładowanie pełnej umowy jeśli to edycja
            if (Model != null && Model.Id > 0)
            {
                var pelnaUmowa = await UmowaService.PobierzUmowePoIdAsync(Model.Id);
                if (pelnaUmowa != null)
                {
                    Model = pelnaUmowa;
                }
            }

            // Inicjalizacja kolekcji, jeśli jest pusta (aby uniknąć błędu NullReference)
            if (Model != null && Model.WynajmowaneLokale == null)
            {
                Model.WynajmowaneLokale = new List<UmowaLokal>();
            }

            // Pobieranie lokali z serwisu
            var lokaleZBazy = await LokalWynajemService.PobierzWszystkieAsync();
            ListaDostepnychLokali = lokaleZBazy ?? new List<LokalWynajem>();

            // Automatyczne podpięcie lokali przypisanych do tego najemcy w zakładce "Lokale i pomieszczenia"
            if (Model != null && Model.NajemcaId > 0)
            {
                SynchronizujLokaleNajemcy(Model.NajemcaId);
            }
        }

        // -------------------------------------------------------------
        // LOGIKA: PRZYPISYWANIE LOKALI DO UMOWY
        // -------------------------------------------------------------

        protected IEnumerable<LokalWynajem> DostepneLokaleDoWyboru => ListaDostepnychLokali
            .Where(lok => !lok.CzyCzescWspolna
                          && (Model.WynajmowaneLokale == null || !Model.WynajmowaneLokale.Any(rl => rl.LokalWynajemId == lok.Id))
                          && lok.PowierzchniaDostepnaM2 > 0
                          && (lok.StatusAktualny != "Zarezerwowany" || (Model.NajemcaId > 0 && lok.NajemcaId == Model.NajemcaId)));

        protected decimal PowierzchniaDostepnaDlaUmowy(LokalWynajem lokal) =>
            Math.Max(0, lokal.PowierzchniaM2 - (lokal.HistoriaUmow?
                .Where(relacja => relacja.UmowaNajmu?.CzyAktywna == true && relacja.UmowaNajmuId != Model.Id)
                .Sum(relacja => relacja.PowierzchniaWynajetaM2) ?? 0));

        protected void OnNajemcaChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out int id))
            {
                Model.NajemcaId = id;
                SynchronizujLokaleNajemcy(id);
            }
            else
            {
                Model.NajemcaId = 0;
            }
        }

        protected void SynchronizujLokaleNajemcy(int najemcaId)
        {
            if (najemcaId <= 0 || ListaDostepnychLokali == null || Model == null) return;

            if (Model.WynajmowaneLokale == null)
            {
                Model.WynajmowaneLokale = new List<UmowaLokal>();
            }

            var lokaleNajemcy = ListaDostepnychLokali.Where(l => l.NajemcaId == najemcaId).ToList();
            bool dodano = false;

            foreach (var lokal in lokaleNajemcy)
            {
                var dostepnaPowierzchnia = PowierzchniaDostepnaDlaUmowy(lokal);
                if (!lokal.CzyCzescWspolna
                    && dostepnaPowierzchnia > 0
                    && lokal.StatusAktualny != "Zarezerwowany"
                    && !Model.WynajmowaneLokale.Any(wl => wl.LokalWynajemId == lokal.Id))
                {
                    Model.WynajmowaneLokale.Add(new UmowaLokal
                    {
                        UmowaNajmuId = Model.Id,
                        LokalWynajemId = lokal.Id,
                        LokalWynajem = lokal,
                        PowierzchniaWynajetaM2 = dostepnaPowierzchnia,
                        WynegocjowanaCenaZaM2 = lokal.CenaZaM2,
                        CzyRyczalt = false
                    });
                    dodano = true;
                }
            }

            if (dodano)
            {
                StateHasChanged();
            }
        }

        protected void OnLokalChanged(int? selectedId)
        {
            var id = selectedId.GetValueOrDefault();
            if (id > 0)
            {
                WybranyLokalId = id;
                var lokal = ListaDostepnychLokali.FirstOrDefault(l => l.Id == id);
                if (lokal != null)
                {
                    WybranaCena = lokal.CenaZaM2; // Podpowiedź domyślnej ceny katalogowej
                    WybranaDostepnaPowierzchniaM2 = PowierzchniaDostepnaDlaUmowy(lokal);
                    WybranaPowierzchniaM2 = WybranaDostepnaPowierzchniaM2;
                }
                else
                {
                    WybranaCena = 0;
                    WybranaPowierzchniaM2 = 0;
                    WybranaDostepnaPowierzchniaM2 = 0;
                }
            }
            else
            {
                WybranyLokalId = 0;
                WybranaCena = 0;
                WybranaPowierzchniaM2 = 0;
                WybranaDostepnaPowierzchniaM2 = 0;
            }
            StateHasChanged();
        }

        protected async Task DodajLokalDoUmowy()
        {
            if (WybranyLokalId == 0)
            {
                await JS.InvokeVoidAsync("alert", "Wybierz pomieszczenie z listy.");
                return;
            }

            if (Model.WynajmowaneLokale.Any(l => l.LokalWynajemId == WybranyLokalId))
            {
                await JS.InvokeVoidAsync("alert", "To pomieszczenie jest już przypisane do tej umowy.");
                return;
            }

            var lokal = ListaDostepnychLokali.FirstOrDefault(l => l.Id == WybranyLokalId);
            if (lokal == null) return;

            var dostepnaPowierzchnia = PowierzchniaDostepnaDlaUmowy(lokal);
            if (WybranaPowierzchniaM2 <= 0 || WybranaPowierzchniaM2 > dostepnaPowierzchnia)
            {
                await JS.InvokeVoidAsync("alert", $"Podaj powierzchnię większą od 0 i nie większą niż dostępne {dostepnaPowierzchnia:N2} m².");
                return;
            }

            var nowaRelacja = new UmowaLokal
            {
                UmowaNajmuId = Model.Id,
                LokalWynajemId = WybranyLokalId,
                LokalWynajem = lokal,
                PowierzchniaWynajetaM2 = WybranaPowierzchniaM2,
                WynegocjowanaCenaZaM2 = WybranaCena,
                CzyRyczalt = NowyCzyRyczalt
            };

            Model.WynajmowaneLokale.Add(nowaRelacja);

            // Reset pól po dodaniu
            WybranyLokalId = 0;
            WybranaCena = 0;
            WybranaPowierzchniaM2 = 0;
            NowyCzyRyczalt = false;

            StateHasChanged();
        }

        protected void UsunLokalZUmowy(UmowaLokal relacja)
        {
            if (Model.WynajmowaneLokale != null)
            {
                Model.WynajmowaneLokale.Remove(relacja);
                StateHasChanged();
            }
        }

        // -------------------------------------------------------------
        // LOGIKA: ZAPISYWANIE UMOWY I PLIKÓW
        // -------------------------------------------------------------

        protected void OnInputFileChange(InputFileChangeEventArgs e)
        {
            _wybranyPlik = e.File;
            if (_wybranyPlik != null)
            {
                WybranyPlikNazwa = _wybranyPlik.Name;
                WybranyPlikRozmiarKB = _wybranyPlik.Size / 1024;
            }
        }

        protected async Task Zapisz()
        {
            if (_trwaPrzetwarzanie) return;
            _trwaPrzetwarzanie = true;

            try
            {
                //Model.Najemca = null;

                //if (Model.WynajmowaneLokale != null)
                //{
                //    foreach (var relacja in Model.WynajmowaneLokale)
                //    {
                //        relacja.LokalWynajem = null;
                //        relacja.UmowaNajmu = null; // Unikamy nieskończonej pętli referencji
                //    }
                //}
                bool sukces = await UmowaService.ZapiszUmoweAsync(Model);

                if (sukces)
                {
                    // Obsługa zapisu załącznika
                    if (_wybranyPlik != null)
                    {
                        var katalogSharedUploads = Path.Combine(Directory.GetCurrentDirectory(), "Shared", "upload_umowy");
                        if (!Directory.Exists(katalogSharedUploads))
                        {
                            Directory.CreateDirectory(katalogSharedUploads);
                        }

                        var unikalnaNazwa = $"{Guid.NewGuid()}_{_wybranyPlik.Name}";
                        var pelnaSciezka = Path.Combine(katalogSharedUploads, unikalnaNazwa);

                        await using var fs = new FileStream(pelnaSciezka, FileMode.Create);
                        await _wybranyPlik.OpenReadStream(maxAllowedSize: 10485760).CopyToAsync(fs);

                        string relatywnaSciezka = $"/Shared/upload_umowy/{unikalnaNazwa}";

                        var nowyZalacznik = new ZalacznikUmowy
                        {
                            UmowaId = Model.Id,
                            NazwaPliku = _wybranyPlik.Name,
                            SciezkaPliku = relatywnaSciezka,
                            DataDodania = DateTime.Now
                        };

                        bool zalacznikZapisany = await UmowaService.DodajZalacznikAsync(nowyZalacznik);
                        if (!zalacznikZapisany)
                        {
                            await JS.InvokeVoidAsync("alert", "Plik skopiowano na dysk, ale wystąpił błąd podczas zapisu w bazie danych!");
                            return;
                        }
                    }

                    await JS.InvokeVoidAsync("alert", "Zapisano umowę pomyślnie!");
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    await JS.InvokeVoidAsync("alert", "Serwis zwrócił błąd podczas zapisu umowy.");
                }
            }
            catch (Exception ex)
            {
                string dokladnyBlad = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                await JS.InvokeVoidAsync("alert", $"Błąd zapisu bazy danych: {dokladnyBlad}");
            }
            finally
            {
                _trwaPrzetwarzanie = false;
            }
        }

        protected Task UsunZalacznik(ZalacznikUmowy zalacznik)
        {
            if (zalacznik == null || _trwaPrzetwarzanie) return Task.CompletedTask;

            TytulPotwierdzenia = "Usuwanie załącznika";
            TrescPotwierdzenia = $"Czy na pewno usunąć plik „{zalacznik.NazwaPliku}”?";
            _potwierdzonaAkcja = () => UsunZalacznikPotwierdzony(zalacznik);
            CzyPotwierdzicOperacje = true;
            return Task.CompletedTask;
        }

        private async Task UsunZalacznikPotwierdzony(ZalacznikUmowy zalacznik)
        {
            _trwaPrzetwarzanie = true;
            try
            {
                bool sukces = await UmowaService.UsunZalacznikAsync(zalacznik.Id);
                if (sukces)
                {
                    Model.Zalaczniki.Remove(zalacznik);

                    var pelnaSciezka = Path.Combine(Directory.GetCurrentDirectory(), zalacznik.SciezkaPliku.TrimStart('/'));
                    if (File.Exists(pelnaSciezka))
                    {
                        File.Delete(pelnaSciezka);
                    }

                    StateHasChanged();
                    await JS.InvokeVoidAsync("alert", "Załącznik został usunięty.");
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("alert", $"Błąd usuwania załącznika: {ex.Message}");
            }
            finally
            {
                _trwaPrzetwarzanie = false;
            }
        }

        // -------------------------------------------------------------
        // LOGIKA: ANEKSY
        // -------------------------------------------------------------

        protected async Task ZapiszAneks()
        {
            if (_trwaPrzetwarzanie) return;

            try
            {
                if (string.IsNullOrWhiteSpace(NowyAneks.NumerAneksu))
                {
                    await JS.InvokeVoidAsync("alert", "Numer aneksu jest wymagany.");
                    return;
                }

                _trwaPrzetwarzanie = true;

                var aneksDoZapisu = new AneksUmowy
                {
                    UmowaNajmuId = Model.Id,
                    NumerAneksu = NowyAneks.NumerAneksu,
                    DataZawarcia = NowyAneks.DataZawarcia,
                    NowaStawkaCzynszu = NowyAneks.NowaStawkaCzynszu,
                    OpisZmian = string.IsNullOrWhiteSpace(NowyAneks.OpisZmian) ? "" : NowyAneks.OpisZmian,
                    DataDodania = DateTime.Now
                };

                bool sukces = await UmowaService.DodajAneksDoUmowyAsync(aneksDoZapisu);

                if (sukces)
                {
                    if (Model.Aneksy == null)
                    {
                        Model.Aneksy = new List<AneksUmowy>();
                    }

                    bool juzIstnieje = Model.Aneksy.Any(a => a.NumerAneksu == aneksDoZapisu.NumerAneksu);
                    if (!juzIstnieje)
                    {
                        Model.Aneksy.Add(aneksDoZapisu);
                    }

                    NowyAneks = new AneksUmowy { DataZawarcia = DateTime.Today };
                    StateHasChanged();

                    await JS.InvokeVoidAsync("alert", "Sukces! Aneks został dodany.");
                }
                else
                {
                    await JS.InvokeVoidAsync("alert", "Nie udało się zapisać aneksu w bazie.");
                }
            }
            catch (Exception ex)
            {
                string bladBazy = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                await JS.InvokeVoidAsync("alert", $"BŁĄD SYSTEMU: {bladBazy}");
            }
            finally
            {
                _trwaPrzetwarzanie = false;
            }
        }

        protected Task UsunAneks(AneksUmowy aneks)
        {
            if (aneks == null || _trwaPrzetwarzanie) return Task.CompletedTask;

            TytulPotwierdzenia = "Usuwanie aneksu";
            TrescPotwierdzenia = $"Czy na pewno chcesz usunąć aneks nr {aneks.NumerAneksu}?";
            _potwierdzonaAkcja = () => UsunAneksPotwierdzony(aneks);
            CzyPotwierdzicOperacje = true;
            return Task.CompletedTask;
        }

        private async Task UsunAneksPotwierdzony(AneksUmowy aneks)
        {
            _trwaPrzetwarzanie = true;
            try
            {
                bool sukces = await UmowaService.UsunAneksAsync(aneks.Id);
                if (sukces)
                {
                    Model.Aneksy.Remove(aneks);
                    StateHasChanged();
                    await JS.InvokeVoidAsync("alert", "Aneks usunięty.");
                }
                else
                {
                    await JS.InvokeVoidAsync("alert", "Nie udało się usunąć aneksu z bazy.");
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("alert", $"Wystąpił błąd podczas usuwania: {ex.Message}");
            }
            finally
            {
                _trwaPrzetwarzanie = false;
            }
        }

        protected async Task ObsluzPotwierdzenieAsync(bool czyPotwierdzono)
        {
            CzyPotwierdzicOperacje = false;
            var akcja = _potwierdzonaAkcja;
            _potwierdzonaAkcja = null;

            if (czyPotwierdzono && akcja != null)
            {
                await akcja();
            }
        }

        protected void Cancel() => MudDialog?.Cancel();
    }
}