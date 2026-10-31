using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class CieploPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Uprawnienia",
                columns: new[] { "Id", "NazwaSystemowa", "Opis" },
                values: new object[,]
                {
                    { 35, "Cieplo.Odczyt", "Podgląd odczytów, faktur i zużycia ciepła" },
                    { 36, "Cieplo.Edycja", "Dodawanie, edycja i usuwanie odczytów oraz faktur ciepła" }
                });

            migrationBuilder.Sql("""
                INSERT IGNORE INTO UzytkownikUprawnienie (UprawnieniaId, UzytkownicyId)
                SELECT DISTINCT 35, przypisanie.UzytkownicyId
                FROM UzytkownikUprawnienie AS przypisanie
                INNER JOIN Uprawnienia AS uprawnienie ON uprawnienie.Id = przypisanie.UprawnieniaId
                WHERE uprawnienie.NazwaSystemowa IN ('Liczniki.Odczyt', 'Liczniki.Edycja');
                """);

            migrationBuilder.Sql("""
                INSERT IGNORE INTO UzytkownikUprawnienie (UprawnieniaId, UzytkownicyId)
                SELECT DISTINCT 36, przypisanie.UzytkownicyId
                FROM UzytkownikUprawnienie AS przypisanie
                INNER JOIN Uprawnienia AS uprawnienie ON uprawnienie.Id = przypisanie.UprawnieniaId
                WHERE uprawnienie.NazwaSystemowa = 'Liczniki.Edycja';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM UzytkownikUprawnienie WHERE UprawnieniaId IN (35, 36);");

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 36);
        }
    }
}
