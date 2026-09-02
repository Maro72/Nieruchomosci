using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mieszkaniec.Services
{
    public static class ExportDoExcela
    {
        public static async Task ExportToCsvAsExcel<TItem>(
            IJSRuntime jsRuntime,
            IEnumerable<TItem> dane,
            string nazwaPlikuPodstawa,
            Dictionary<string, Func<TItem, object>> konfiguracjaKolumn)
        {
            try
            {
                if (dane == null || !dane.Any() || konfiguracjaKolumn == null || !konfiguracjaKolumn.Any())
                {
                    return;
                }

                // 1. Rejestracja dostawcy stron kodowych dla polskiego Windows-1250 (ANSI)
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                var win1250 = System.Text.Encoding.GetEncoding(1250);

                StringBuilder csv = new StringBuilder();

                // Informacja dla Excela o separatorze kolumn
                csv.AppendLine("sep=,");

                // 2. Automatyczne budowanie nagłówków na podstawie kluczy ze słownika
                string naglowki = string.Join(",", konfiguracjaKolumn.Keys);
                csv.AppendLine(naglowki);

                // 3. Automatyczne budowanie wierszy danych
                foreach (var item in dane)
                {
                    var wartosciWiersza = new List<string>();

                    foreach (var selector in konfiguracjaKolumn.Values)
                    {
                        var wartoscObj = selector(item);
                        string wartoscTekst = "";

                        if (wartoscObj != null)
                        {
                            // Jeśli to liczba zmiennoprzecinkowa, formatujemy z kropką dziesiętną dla Excela
                            if (wartoscObj is decimal || wartoscObj is double || wartoscObj is float)
                            {
                                wartoscTekst = Convert.ToDouble(wartoscObj)
                                    .ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                // Dla tekstu usuwamy przecinki, aby nie rozbijały struktury kolumn CSV
                                wartoscTekst = wartoscObj.ToString()!.Replace(",", " ");
                            }
                        }

                        wartosciWiersza.Add(wartoscTekst);
                    }

                    csv.AppendLine(string.Join(",", wartosciWiersza));
                }

                // 4. Konwersja na bajty Windows-1250 (gwarancja polskich liter w Excelu)
                byte[] finalBytes = win1250.GetBytes(csv.ToString());

                // 5. Przygotowanie nazwy pliku i wysłanie strumienia do przeglądarki przez JS
                string base64 = Convert.ToBase64String(finalBytes);
                string fileName = $"{nazwaPlikuPodstawa}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                await jsRuntime.InvokeVoidAsync("downloadFileFromBytes", fileName, base64);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GLOBALNY BŁĄD EKSPORTU]: {ex.Message}");
            }
        }
    }
}