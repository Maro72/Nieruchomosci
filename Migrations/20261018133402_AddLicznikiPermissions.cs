using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class AddLicznikiPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Uprawnienia",
                columns: new[] { "Id", "NazwaSystemowa", "Opis" },
                values: new object[,]
                {
                    { 33, "Liczniki.Odczyt", "Podgląd liczników wody" },
                    { 34, "Liczniki.Edycja", "Dodawanie i edycja liczników wody" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Uprawnienia",
                keyColumn: "Id",
                keyValue: 34);
        }
    }
}
