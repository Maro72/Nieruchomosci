using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mieszkaniec.Migrations
{
    /// <inheritdoc />
    public partial class AddLiczniki : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "liczniki",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumerLicznika = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Typ = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ObiektId = table.Column<int>(type: "int", nullable: true),
                    DataDodania = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CzyAktywny = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_liczniki", x => x.Id);
                    table.ForeignKey(
                        name: "FK_liczniki_obiekty_ObiektId",
                        column: x => x.ObiektId,
                        principalTable: "obiekty",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "odczyty_wody",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LicznikId = table.Column<int>(type: "int", nullable: false),
                    StanPoczatkowy = table.Column<int>(type: "int", nullable: false),
                    StanKoncowy = table.Column<int>(type: "int", nullable: false),
                    Zuzycie = table.Column<int>(type: "int", nullable: false),
                    Korekta = table.Column<int>(type: "int", nullable: true),
                    IloscDoZafakturowania = table.Column<int>(type: "int", nullable: false),
                    DataOdczytu = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OstrzezenieNadmierneZuzycie = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_odczyty_wody", x => x.Id);
                    table.ForeignKey(
                        name: "FK_odczyty_wody_liczniki_LicznikId",
                        column: x => x.LicznikId,
                        principalTable: "liczniki",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_liczniki_ObiektId",
                table: "liczniki",
                column: "ObiektId");

            migrationBuilder.CreateIndex(
                name: "IX_odczyty_wody_LicznikId",
                table: "odczyty_wody",
                column: "LicznikId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "odczyty_wody");

            migrationBuilder.DropTable(
                name: "liczniki");
        }
    }
}
