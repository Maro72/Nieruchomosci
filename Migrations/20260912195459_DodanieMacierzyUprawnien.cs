using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class DodanieMacierzyUprawnien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Uprawnienia",
                columns: new[] { "Id", "NazwaSystemowa", "Opis" },
                values: new object[,]
                {
                    { 17, "Dashboard.Odczyt", "Podgląd pulpitu zarządczego" },
                    { 18, "HistoriaUsterek.Odczyt", "Podgląd historii usterek" },
                    { 19, "Przeglady.Odczyt", "Podgląd przeglądów technicznych" },
                    { 20, "Przeglady.Edycja", "Zarządzanie przeglądami technicznymi" },
                    { 21, "Remonty.Odczyt", "Podgląd prac remontowych" },
                    { 22, "Remonty.Edycja", "Zarządzanie pracami remontowymi" },
                    { 23, "Lokale.Odczyt", "Podgląd lokali i pomieszczeń" },
                    { 24, "Lokale.Edycja", "Zarządzanie lokalami i rzutami" },
                    { 25, "Najemcy.Odczyt", "Podgląd bazy najemców" },
                    { 26, "Najemcy.Edycja", "Zarządzanie bazą najemców" },
                    { 27, "Uzytkownicy.Odczyt", "Podgląd kont użytkowników" },
                    { 28, "Uzytkownicy.Edycja", "Zarządzanie kontami użytkowników" },
                    { 29, "Uprawnienia.Odczyt", "Podgląd ról i uprawnień" },
                    { 30, "Uprawnienia.Edycja", "Nadawanie ról i uprawnień" },
                    { 31, "Konfiguracja.Odczyt", "Podgląd konfiguracji systemu" },
                    { 32, "Konfiguracja.Edycja", "Zarządzanie konfiguracją systemu" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 32);
        }
    }
}
