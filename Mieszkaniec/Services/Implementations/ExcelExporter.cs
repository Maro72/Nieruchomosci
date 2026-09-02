using OfficeOpenXml;
using OfficeOpenXml.Attributes; // Opcjonalnie
using System;
using System.Collections.Generic;
using System.IO;

namespace Mieszkaniec.Services
{
    public static class ExcelExporter
    {
        public static byte[] ExportToExcel<T>(IEnumerable<T> dane, string nazwaArkusza, Dictionary<string, Func<T, object>> kolumny)
        {
            // EPPlus wymaga ustawienia licencji komercyjnej lub NonCommercial (darmowa dla osób fizycznych / open source)
           // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(nazwaArkusza);

                // 1. Tworzenie nagłówków
                int kolumnaIdx = 1;
                foreach (var kolumna in kolumny.Keys)
                {
                    worksheet.Cells[1, kolumnaIdx].Value = kolumna;
                    worksheet.Cells[1, kolumnaIdx].Style.Font.Bold = true;
                    // Opcjonalnie: delikatne szare tło dla nagłówków
                    worksheet.Cells[1, kolumnaIdx].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, kolumnaIdx].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    kolumnaIdx++;
                }

                // 2. Wypełnianie danymi
                int wierszIdx = 2;
                foreach (var item in dane)
                {
                    kolumnaIdx = 1;
                    foreach (var selector in kolumny.Values)
                    {
                        var wartosc = selector(item);
                        worksheet.Cells[wierszIdx, kolumnaIdx].Value = wartosc;

                        // Formatowanie liczb zmiennoprzecinkowych, żeby Excel widział je poprawnie
                        if (wartosc is decimal || wartosc is double)
                        {
                            worksheet.Cells[wierszIdx, kolumnaIdx].Style.Numberformat.Format = "#,##0.00";
                        }

                        kolumnaIdx++;
                    }
                    wierszIdx++;
                }

                // 3. Automatyczne dopasowanie szerokości kolumn do zawartości
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
    }
}