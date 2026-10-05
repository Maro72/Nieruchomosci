using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class DodanieDanychProtokołuIPowierzchniZabudowy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataProtokołu",
                table: "przeglady",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumerProtokołu",
                table: "przeglady",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Uwagi",
                table: "przeglady",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "PowierzchniaZabudowy",
                table: "obiekty",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

#if false
            migrationBuilder.InsertData(
                table: "PriorytetyUsterek",
                columns: new[] { "Id", "KodKoloru", "MaksCzasReakcjiGodziny", "Nazwa", "Poziom" },
                values: new object[,]
                {
                    { 1, "info", 72, "Niski", 1 },
                    { 2, "primary", 48, "Normalny", 2 },
                    { 3, "warning", 24, "Wysoki", 3 },
                    { 4, "danger", 4, "Krytyczny / Awaria", 4 }
                });

            migrationBuilder.InsertData(
                table: "RodzajeUsterek",
                columns: new[] { "Id", "CzyWymagaUprawnien", "KlasaIkony", "Nazwa" },
                values: new object[,]
                {
                    { 1, true, "bi-lightning-charge", "Instalacje Elektryczne" },
                    { 2, true, "bi-droplet-fill", "Instalacje Wodno-Kanalizacyjne" },
                    { 3, true, "bi-fire", "Instalacje Gazowe i C.O." },
                    { 4, true, "bi-wind", "Wentylacja i Klimatyzacja" },
                    { 5, true, "bi-box-arrow-up-down", "Dźwigi i Windy" },
                    { 6, false, "bi-door-open", "Stolarka Okienna i Drzwiowa" },
                    { 7, false, "bi-house-exclamation", "Dach, Rynny i Elewacja" },
                    { 8, false, "bi-shield-lock", "Systemy Bezpieczeństwa i CCTV" },
                    { 9, false, "bi-tools", "Prace Ogólnobudowlane" },
                    { 10, false, "bi-tree", "Teren Zewnętrzny i Zieleń" }
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "Nazwa" },
                values: new object[,]
                {
                    { 1, "Administrator" },
                    { 2, "Zarządca Nieruchomości" },
                    { 3, "Konserwator / Technik" },
                    { 4, "Agent Najmu" }
                });

            migrationBuilder.InsertData(
                table: "Uprawnienia",
                columns: new[] { "Id", "NazwaSystemowa", "Opis" },
                values: new object[,]
                {
                    { 1, "Budynki.Odczyt", "Podgląd budynków i obiektów" },
                    { 2, "Budynki.Edycja", "Zarządzanie i edycja budynków" },
                    { 3, "Lokale.Zarzadzanie", "Zarządzanie lokalami i rzutami" },
                    { 4, "Awarie.Odczyt", "Podgląd zgłoszeń awarii i usterek" },
                    { 5, "Awarie.Obsluga", "Konserwacja i obsługa usterek" },
                    { 6, "Przeglady.Zarzadzanie", "Zarządzanie przeglądami technicznymi" },
                    { 7, "Remonty.Zarzadzanie", "Zarządzanie pracami remontowymi" },
                    { 8, "Najemcy.Zarzadzanie", "Zarządzanie bazą najemców" },
                    { 9, "Umowy.Odczyt", "Podgląd umów najmu" },
                    { 10, "Umowy.Zarzadzanie", "Rejestracja i edycja umów oraz aneksów" },
                    { 11, "Uzytkownicy.Zarzadzanie", "Zarządzanie kontami użytkowników" },
                    { 12, "Uprawnienia.Nadawanie", "Nadawanie ról i uprawnień" }
                });
#endif
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
#if false
            migrationBuilder.DeleteData(
                table: "PriorytetyUsterek",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PriorytetyUsterek",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PriorytetyUsterek",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PriorytetyUsterek",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RodzajeUsterek",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RodzajeUsterek",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RodzajeUsterek",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RodzajeUsterek",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RodzajeUsterek",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "RodzajeUsterek",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "RodzajeUsterek",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "RodzajeUsterek",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "RodzajeUsterek",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "RodzajeUsterek",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 12);
#endif

            migrationBuilder.DropColumn(
                name: "DataProtokołu",
                table: "przeglady");

            migrationBuilder.DropColumn(
                name: "NumerProtokołu",
                table: "przeglady");

            migrationBuilder.DropColumn(
                name: "Uwagi",
                table: "przeglady");

            migrationBuilder.DropColumn(
                name: "PowierzchniaZabudowy",
                table: "obiekty");
        }
    }
}
