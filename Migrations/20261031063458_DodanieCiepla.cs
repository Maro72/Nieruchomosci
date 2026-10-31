using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class DodanieCiepla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "faktury_ciepla",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumerFaktury = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumerKsef = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RodzajFaktury = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Waluta = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sprzedawca = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrefiksVatSprzedawcy = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NipSprzedawcy = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdresSprzedawcy = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nabywca = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NipNabywcy = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdresNabywcy = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DotyczyJst = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DotyczyGv = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DodatkoweInformacje = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumerKlienta = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataWystawienia = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataOd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataDo = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Rok = table.Column<int>(type: "int", nullable: false),
                    Miesiac = table.Column<int>(type: "int", nullable: false),
                    WartoscNetto = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    WartoscVat = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    WartoscBrutto = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    DataDodania = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faktury_ciepla", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "odczyty_ciepla",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumerLicznika = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObiektId = table.Column<int>(type: "int", nullable: true),
                    Rok = table.Column<int>(type: "int", nullable: false),
                    Miesiac = table.Column<int>(type: "int", nullable: false),
                    StanPoczatkowy = table.Column<decimal>(type: "decimal(12,3)", nullable: false),
                    StanKoncowy = table.Column<decimal>(type: "decimal(12,3)", nullable: false),
                    ZuzycieGJ = table.Column<decimal>(type: "decimal(12,3)", nullable: false),
                    Korekta = table.Column<decimal>(type: "decimal(12,3)", nullable: true),
                    IloscDoZafakturowania = table.Column<decimal>(type: "decimal(12,3)", nullable: false),
                    Uwagi = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataOdczytu = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_odczyty_ciepla", x => x.Id);
                    table.ForeignKey(
                        name: "FK_odczyty_ciepla_obiekty_ObiektId",
                        column: x => x.ObiektId,
                        principalTable: "obiekty",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pozycje_faktur_ciepla",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FakturaCieplaId = table.Column<int>(type: "int", nullable: false),
                    Lp = table.Column<int>(type: "int", nullable: false),
                    NazwaTowaruUslugi = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Jm = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ilosc = table.Column<decimal>(type: "decimal(14,6)", nullable: false),
                    CenaNetto = table.Column<decimal>(type: "decimal(14,6)", nullable: false),
                    StawkaVat = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    WartoscNetto = table.Column<decimal>(type: "decimal(14,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pozycje_faktur_ciepla", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pozycje_faktur_ciepla_faktury_ciepla_FakturaCieplaId",
                        column: x => x.FakturaCieplaId,
                        principalTable: "faktury_ciepla",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_odczyty_ciepla_ObiektId",
                table: "odczyty_ciepla",
                column: "ObiektId");

            migrationBuilder.CreateIndex(
                name: "IX_pozycje_faktur_ciepla_FakturaCieplaId",
                table: "pozycje_faktur_ciepla",
                column: "FakturaCieplaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "odczyty_ciepla");

            migrationBuilder.DropTable(
                name: "pozycje_faktur_ciepla");

            migrationBuilder.DropTable(
                name: "faktury_ciepla");
        }
    }
}
