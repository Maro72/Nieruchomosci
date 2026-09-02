using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mieszkaniec.Migrations
{
    public partial class InitOdNowa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Dodanie brakujących kolumn robocizny do tabeli PraceRemontowe
            migrationBuilder.AddColumn<int>(
                name: "GodzinyDziennie",
                table: "PraceRemontowe",
                type: "int",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.AddColumn<int>(
                name: "LiczbaPracownikow",
                table: "PraceRemontowe",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "SzacowanaLiczbaDni",
                table: "PraceRemontowe",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "KosztCalkowityRobocizny",
                table: "PraceRemontowe",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // 2. Utworzenie nowej tabeli na pozycje materiałowe
            migrationBuilder.CreateTable(
                name: "KosztorysMaterial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PraceRemontoweId = table.Column<int>(type: "int", nullable: false),
                    NazwaMaterialu = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Jm = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ilosc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CenaJednostkowa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WartoscCalkowita = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KosztorysMaterial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KosztorysMaterial_PraceRemontowe_PraceRemontoweId",
                        column: x => x.PraceRemontoweId,
                        principalTable: "PraceRemontowe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // 3. Utworzenie indeksu przyspieszającego wyszukiwanie
            migrationBuilder.CreateIndex(
                name: "IX_KosztorysMaterial_PraceRemontoweId",
                table: "KosztorysMaterial",
                column: "PraceRemontoweId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Zostawiamy puste dla bezpieczeństwa obecnych danych
        }
    }

}
